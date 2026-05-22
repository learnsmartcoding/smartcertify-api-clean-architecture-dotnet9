using LSC.SmartCertify.API.Models;
using LSC.SmartCertify.API.Services;
using LSC.SmartCertify.Application.Interfaces.Chat;
using LSC.SmartCertify.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LSC.SmartCertify.API.Controllers;

/// <summary>
/// AI-powered exam chat endpoint.
///
/// The Angular chat component posts each user message here.
/// This controller runs the Claude + MCP tool-calling loop via AnthropicService,
/// persists the conversation to ChatSessions/ChatMessages, and returns Claude's
/// reply along with an optional examId when an exam is created.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController(
    AnthropicService anthropicService,
    IChatHistoryService chatHistoryService,
    ICurrentUserService currentUserService,
    ILogger<ChatController> logger) : ControllerBase
{
    private const int MaxHistoryTurns = 10; // prevent runaway sessions

    [HttpPost("exam-assistant")]
    public async Task<IActionResult> ExamAssistant([FromBody] ChatRequest request, CancellationToken ct)
    {
        // ── Validation ────────────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message cannot be empty.");

        if (request.Message.Length > 500)
            return BadRequest("Message must be 500 characters or fewer.");

        if (request.History.Count > MaxHistoryTurns * 2)
            return BadRequest("Conversation is too long. Please start a new session.");

        // ── Auth: identify the current user ───────────────────────────────
        var currentUser = await currentUserService.GetCurrentUserProfileAsync();
        if (currentUser is null)
            return Unauthorized("User profile not found.");

        // ── Run the Claude + MCP loop ─────────────────────────────────────
        try
        {
            logger.LogInformation("Chat turn for user {UserId}: {Message}", currentUser.UserId, request.Message);

            var result = await anthropicService.ProcessTurnAsync(
                request.Message,
                request.History,
                currentUser.UserId,
                ct);

            // ── Persist chat history ──────────────────────────────────────
            int sessionId;

            if (request.SessionId.HasValue)
            {
                // Continuing an existing session
                sessionId = request.SessionId.Value;
            }
            else
            {
                // First message — create a new session
                sessionId = await chatHistoryService.StartSessionAsync(
                    currentUser.UserId, request.Message, ct);
            }

            // Save both the user message and Claude's response
            await chatHistoryService.AppendMessagesAsync(
                sessionId,
                request.Message,
                result.Message,
                result.ToolsInvoked,
                result.InputTokens,
                result.OutputTokens,
                ct);

            // If an exam was created, link it to the session and close it
            if (result.ExamId.HasValue)
                await chatHistoryService.LinkExamAsync(sessionId, result.ExamId.Value, ct);

            // ── Build response ────────────────────────────────────────────
            var updatedHistory = new List<ConversationMessage>(request.History)
            {
                new() { Role = "user",      Content = request.Message },
                new() { Role = "assistant", Content = result.Message }
            };

            return Ok(new ChatResponse
            {
                Message        = result.Message,
                ExamId         = result.ExamId,
                ExamCreated    = result.ExamId.HasValue,
                UpdatedHistory = updatedHistory,
                SessionId      = sessionId
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Chat processing failed for user {UserId}", currentUser.UserId);
            return StatusCode(500, "Something went wrong processing your request. Please try again.");
        }
    }

    /// <summary>
    /// Called by Angular when the user navigates away from the chat
    /// (component ngOnDestroy). Marks the session as ended.
    /// </summary>
    [HttpPost("end-session/{sessionId:int}")]
    public async Task<IActionResult> EndSession(int sessionId, CancellationToken ct)
    {
        try
        {
            await chatHistoryService.EndSessionAsync(sessionId, ct);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to end chat session {SessionId}", sessionId);
            return StatusCode(500, "Failed to end session.");
        }
    }
}
