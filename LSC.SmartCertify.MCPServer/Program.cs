using LSC.SmartCertify.Infrastructure;
using LSC.SmartCertify.MCPServer.Tools;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);

// ── Database ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<SmartCertifyContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DbContext"),
        sql => sql.EnableRetryOnFailure()));

// ── MCP Server (Streamable HTTP transport) ───────────────────────────────────
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<SmartCertifyTools>();

// Register SmartCertifyTools as a scoped service so the REST bridge can inject it.
// The MCP SDK registers it separately for its own lifecycle — this adds a second
// registration that is used only by the /api/tools/* routes below.
builder.Services.AddScoped<SmartCertifyTools>();

// ── CORS — allow the SmartCertify main API to call this ────────────────────
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.WithOrigins(
        builder.Configuration["AllowedOrigins:MainApi"] ?? "https://localhost:7209")
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();

app.UseCors();

// ── Health-check ─────────────────────────────────────────────────────────────
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "SmartCertify MCP Server" }));

// ── MCP protocol endpoint (Streamable HTTP — POST /mcp) ──────────────────────
// Used by direct MCP clients. Note: SseClientTransport (old GET-based SSE) is NOT
// compatible with this transport; use the /api/tools/* REST bridge instead.
app.MapMcp("/mcp");

// ── REST tool bridge ─────────────────────────────────────────────────────────
// Called by AnthropicService in the main API.
// Avoids MCP SDK transport compatibility issues while reusing the same tool logic.
var tools = app.MapGroup("/api/tools");

tools.MapPost("/list_courses", async (SmartCertifyTools t, CancellationToken ct) =>
    Results.Text(await t.list_courses(ct), "application/json"));

tools.MapPost("/search_courses", async (HttpRequest req, SmartCertifyTools t, CancellationToken ct) =>
{
    var body = await JsonNode.ParseAsync(req.Body, cancellationToken: ct);
    var topic = body?["topic"]?.GetValue<string>() ?? "";
    return Results.Text(await t.search_courses(topic, ct), "application/json");
});

tools.MapPost("/fetch_questions", async (HttpRequest req, SmartCertifyTools t, CancellationToken ct) =>
{
    var body = await JsonNode.ParseAsync(req.Body, cancellationToken: ct);
    var courseId = body?["courseId"]?.GetValue<int>() ?? 0;
    var count    = body?["count"]?.GetValue<int>()    ?? 10;
    return Results.Text(await t.fetch_questions(courseId, count, ct), "application/json");
});

tools.MapPost("/create_custom_exam", async (HttpRequest req, SmartCertifyTools t, CancellationToken ct) =>
{
    var body = await JsonNode.ParseAsync(req.Body, cancellationToken: ct);
    var questionIds     = body?["question_ids"]?.AsArray()
                              .Select(x => x!.GetValue<int>()).ToList() ?? [];
    var primaryCourseId = body?["primary_course_id"]?.GetValue<int>() ?? 0;
    var userId          = body?["user_id"]?.GetValue<int>()           ?? 0;
    return Results.Text(await t.create_custom_exam(questionIds, primaryCourseId, userId, ct), "application/json");
});

app.Run();
