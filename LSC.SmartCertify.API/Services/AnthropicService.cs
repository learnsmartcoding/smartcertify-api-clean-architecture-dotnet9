using LSC.SmartCertify.API.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LSC.SmartCertify.API.Services;

/// <summary>
/// Orchestrates the Claude API + MCP tool-calling loop.
///
/// Tool schemas are hardcoded here (they never change at runtime).
/// Tool execution is delegated to the MCPServer via a plain REST call to
/// /api/tools/{toolName} — this avoids the SseClientTransport / Streamable-HTTP
/// incompatibility that occurs when using the MCP SDK client directly.
///
/// Flow per chat turn:
///   1. Build messages array from history + new user message.
///   2. Pass the hardcoded tool schemas to Claude.
///   3. POST to Claude's Messages API.
///   4. If Claude returns stop_reason="tool_use", forward each tool call to
///      the MCPServer REST bridge and loop back with the results.
///   5. When stop_reason="end_turn", extract the final text and return it.
/// </summary>
public class AnthropicService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<AnthropicService> logger)
{
    // Haiku 4.5 is ~4× cheaper than Sonnet 4.5 and handles structured tool calling
    // (search → fetch → create exam) with the same quality for this use case.
    private const string ClaudeModel = "claude-haiku-4-5";
    private const int MaxTokens = 1024;   // chat replies don't need 2 048 tokens
    private const int MaxToolLoops = 8;

    private static readonly JsonSerializerOptions _opts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    // Structured as an array so Anthropic can cache it between requests.
    // The cache_control block tells the API to store this content for 5 minutes;
    // subsequent requests pay only 10% of the normal input token price for it.
    private static readonly object[] SystemPromptCached =
    [
        new
        {
            type = "text",
            text = """
        You are an AI exam assistant for SmartCertify, a developer certification platform.
        Your ONLY purpose is to help users build a custom practice exam from the available course catalog.

        ## Your workflow
        1. Understand which technology/role the user wants to practice.
        2. Call search_courses or list_courses to find relevant courses.
        3. Call fetch_questions for each course you want to include (adjust counts to match user preferences).
        4. Propose the exam plan clearly: list each course, how many questions, and the total count.
        5. Wait for the user to confirm ("yes", "go ahead", "start", "looks good", etc.).
        6. Once confirmed, call create_custom_exam with ALL collected question IDs.
        7. Tell the user their exam is ready and encourage them to click the Start Exam button.

        ## Rules
        - Only discuss exam preparation and developer topics (Angular, .NET, Azure, JavaScript, AI, etc.).
        - If asked about anything unrelated, politely decline and redirect to exam prep.
        - Never create an exam without user confirmation first.
        - Keep responses concise and friendly — this is a chat, not a lecture.
        - When proposing a plan, format it as a short bullet list.
        - Maximum questions per exam: 40. Suggest a sensible mix if the user wants "everything".

        ## CRITICAL — never expose internal IDs
        Internal numeric IDs (question IDs, exam IDs, course IDs) are implementation details.
        NEVER mention them in any message to the user under any circumstances.

        BAD (never do this):
          "✅ JavaScript – 3 questions (IDs: 12, 14, 15)"
          "Exam ID: 14"
          "courseId 7"

        GOOD (always do this):
          "✅ JavaScript – 3 questions"
          "Your exam is ready! Click Start Exam below."

        The Start Exam button already knows where to go — you do not need to communicate any ID to the user.
        """,
            cache_control = new { type = "ephemeral" }
        }
    ];

    // ── Hardcoded Claude tool schemas ─────────────────────────────────────────
    // These mirror the [McpServerTool] methods in SmartCertifyTools exactly.
    // Hardcoding removes the need for dynamic MCP discovery (and the transport
    // incompatibility that caused that to fail).
    private static readonly List<object> ClaudeTools =
    [
        new
        {
            name = "list_courses",
            description = "List all available courses in SmartCertify with their question counts. " +
                          "Only returns courses that have at least one question. " +
                          "Use this to show the user what topics they can be tested on.",
            input_schema = new
            {
                type = "object",
                properties = new { },
                required = Array.Empty<string>()
            }
        },
        new
        {
            name = "search_courses",
            description = "Search courses by topic keyword. Returns courses whose title or description " +
                          "contains the keyword (case-insensitive). Use this to find courses relevant to " +
                          "a technology or role the user wants to practice.",
            input_schema = new
            {
                type = "object",
                properties = new
                {
                    topic = new
                    {
                        type = "string",
                        description = "The technology or topic keyword (e.g. 'angular', '.net', 'azure', 'javascript', 'ai')"
                    }
                },
                required = new[] { "topic" }
            }
        },
        new
        {
            name = "fetch_questions",
            description = "Fetch a random set of question IDs from a specific course. " +
                          "Returns an array of question IDs to pass to create_custom_exam. " +
                          "Call this for each course you want to include.",
            input_schema = new
            {
                type = "object",
                properties = new
                {
                    courseId = new { type = "integer", description = "The courseId (from list_courses or search_courses)" },
                    count    = new { type = "integer", description = "How many questions to fetch (1-20 recommended per course)" }
                },
                required = new[] { "courseId", "count" }
            }
        },
        new
        {
            name = "create_custom_exam",
            description = "Create a custom exam with the specified question IDs. " +
                          "Call this ONLY after the user has confirmed the exam plan. " +
                          "Pass ALL question IDs collected from fetch_questions calls. " +
                          "The userId is supplied securely by the server — do NOT ask the user for it.",
            input_schema = new
            {
                type = "object",
                properties = new
                {
                    question_ids     = new { type = "array",   items = new { type = "integer" }, description = "All question IDs to include" },
                    primary_course_id = new { type = "integer", description = "CourseId of the primary subject" },
                    user_id          = new { type = "integer", description = "Authenticated user ID — supplied automatically by the server" }
                },
                required = new[] { "question_ids", "primary_course_id", "user_id" }
            },
            // cache_control on the LAST tool caches the entire tools array.
            // After the first request, all tool schemas are served from cache
            // at 10% of normal input token cost.
            cache_control = new { type = "ephemeral" }
        }
    ];

    // ── Public entry point ────────────────────────────────────────────────────

    public async Task<ChatTurnResult> ProcessTurnAsync(
        string userMessage,
        List<ConversationMessage> history,
        int userId,
        CancellationToken ct = default)
    {
        if (userMessage.Length > 600)
            userMessage = userMessage[..600];

        var messages = new List<object>();
        foreach (var h in history)
            messages.Add(new { role = h.Role, content = h.Content });
        messages.Add(new { role = "user", content = userMessage });

        // Derive the MCPServer REST base URL from the config entry.
        // Config example: "https://localhost:60952/mcp"  →  "https://localhost:60952"
        var mcpServerUrl = config["McpServer:BaseUrl"] ?? "https://localhost:60952/mcp";
        var mcpRestBase  = DeriveRestBase(mcpServerUrl);

        logger.LogInformation("MCPServer REST base: {Base}", mcpRestBase);

        string?      finalText     = null;
        int?         createdExamId = null;
        var          toolsInvoked  = new List<string>();
        int          inputTokens   = 0;
        int          outputTokens  = 0;

        for (int loop = 0; loop < MaxToolLoops && finalText is null; loop++)
        {
            var claudeResponse = await CallClaudeAsync(messages, ct);
            var stopReason     = claudeResponse?["stop_reason"]?.GetValue<string>();
            var content        = claudeResponse?["content"]?.AsArray();

            // Accumulate token usage across all Claude calls in the loop
            var usage = claudeResponse?["usage"];
            if (usage is not null)
            {
                inputTokens  += usage["input_tokens"]?.GetValue<int>()  ?? 0;
                outputTokens += usage["output_tokens"]?.GetValue<int>() ?? 0;
            }

            if (stopReason == "end_turn" || content is null)
            {
                finalText = content?
                    .FirstOrDefault(c => c?["type"]?.GetValue<string>() == "text")
                    ?["text"]?.GetValue<string>()
                    ?? "I'm not sure how to help with that. Could you tell me which technology you'd like to practice?";
                break;
            }

            if (stopReason == "tool_use")
            {
                messages.Add(new { role = "assistant", content = content });

                var toolResults = new List<object>();
                foreach (var block in content)
                {
                    if (block?["type"]?.GetValue<string>() != "tool_use") continue;

                    var toolId    = block["id"]!.GetValue<string>();
                    var toolName  = block["name"]!.GetValue<string>();
                    var toolInput = block["input"]?.AsObject() ?? new JsonObject();

                    toolsInvoked.Add(toolName);

                    logger.LogInformation("Claude calling tool: {Tool} input: {Input}", toolName, toolInput.ToJsonString());

                    string toolResult;
                    try
                    {
                        if (toolName == "create_custom_exam")
                        {
                            // Override user_id with the server-authenticated value so Claude
                            // cannot create exams for arbitrary users.
                            toolInput["user_id"] = userId;
                        }

                        toolResult = await CallToolRestAsync(mcpRestBase, toolName, toolInput, ct);

                        if (toolName == "create_custom_exam")
                        {
                            var resultObj = JsonNode.Parse(toolResult)?.AsObject();
                            if (resultObj?["success"]?.GetValue<bool>() == true)
                                createdExamId = resultObj["examId"]?.GetValue<int>();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Tool {Tool} REST call failed", toolName);
                        toolResult = $"{{\"error\": \"{toolName} failed: {ex.Message}\"}}";
                    }

                    logger.LogInformation("Tool {Tool} result: {Result}", toolName, toolResult);

                    toolResults.Add(new
                    {
                        type        = "tool_result",
                        tool_use_id = toolId,
                        content     = toolResult
                    });
                }

                messages.Add(new { role = "user", content = toolResults });
            }
        }

        finalText ??= "I've finished processing. Is there anything else you'd like to adjust?";

        return new ChatTurnResult
        {
            Message      = finalText,
            ExamId       = createdExamId,
            ToolsInvoked = toolsInvoked.Count > 0 ? string.Join(",", toolsInvoked.Distinct()) : null,
            InputTokens  = inputTokens,
            OutputTokens = outputTokens
        };
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>Strips the path from the MCP base URL to get the server root.</summary>
    private static string DeriveRestBase(string mcpBaseUrl)
    {
        // e.g. "https://localhost:60952/mcp"  →  "https://localhost:60952"
        //      "http://localhost:5001/mcp"    →  "http://localhost:5001"
        try
        {
            var uri = new Uri(mcpBaseUrl);
            return $"{uri.Scheme}://{uri.Authority}";
        }
        catch
        {
            return mcpBaseUrl.TrimEnd('/').Replace("/mcp", "");
        }
    }

    /// <summary>
    /// Calls the MCPServer's REST bridge for a given tool.
    /// POST {mcpRestBase}/api/tools/{toolName}  with the tool input as the JSON body.
    /// </summary>
    private async Task<string> CallToolRestAsync(
        string mcpRestBase, string toolName, JsonObject toolInput, CancellationToken ct)
    {
        // Use an HttpClient that tolerates the dev self-signed certificate.
        var client = httpClientFactory.CreateClient("McpServer");

        var url  = $"{mcpRestBase}/api/tools/{toolName}";
        var body = new StringContent(toolInput.ToJsonString(), Encoding.UTF8, "application/json");

        logger.LogInformation("Calling MCPServer REST: POST {Url}", url);

        var response = await client.PostAsync(url, body, ct);
        var text     = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"MCPServer REST {toolName} → {(int)response.StatusCode}: {text}");

        return text;
    }

    /// <summary>Posts to Claude's Messages API with the hardcoded tool schemas.</summary>
    private async Task<JsonNode?> CallClaudeAsync(List<object> messages, CancellationToken ct)
    {
        var apiKey = config["Anthropic:ApiKey"]
            ?? throw new InvalidOperationException("Anthropic:ApiKey is not configured.");

        var client = httpClientFactory.CreateClient("Anthropic");

        var requestBody = new
        {
            model      = ClaudeModel,
            max_tokens = MaxTokens,
            system     = SystemPromptCached,   // array form required for prompt caching
            tools      = ClaudeTools,
            messages
        };

        var json    = JsonSerializer.Serialize(requestBody, _opts);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        // Required to enable prompt caching — charges 10% on cache hits vs 100% on misses.
        client.DefaultRequestHeaders.Add("anthropic-beta", "prompt-caching-2024-07-31");

        var response     = await client.PostAsync("https://api.anthropic.com/v1/messages", content, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Claude API error {response.StatusCode}: {responseBody}");

        return JsonNode.Parse(responseBody);
    }
}
