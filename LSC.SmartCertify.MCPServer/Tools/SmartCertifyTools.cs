using LSC.SmartCertify.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace LSC.SmartCertify.MCPServer.Tools;

/// <summary>
/// MCP tool set exposed to Claude via the Model Context Protocol.
/// The main API's ChatController connects to this server as an MCP client,
/// lists these tools, passes their schemas to Claude, and routes Claude's
/// tool_use blocks back here for execution.
/// </summary>
[McpServerToolType]
public class SmartCertifyTools(SmartCertifyContext db)
{
    private static readonly JsonSerializerOptions _json = new() { WriteIndented = false };

    // ── Tool 1 ─────────────────────────────────────────────────────────────

    [McpServerTool]
    [Description(
        "List all available courses in SmartCertify with their question counts. " +
        "Only returns courses that have at least one question available. " +
        "Use this to show the user what topics they can be tested on.")]
    public async Task<string> list_courses(CancellationToken ct)
    {
        var courses = await db.Courses
            .Select(c => new
            {
                courseId = c.CourseId,
                title = c.Title,
                questionCount = c.Questions.Count()
            })
            .Where(c => c.questionCount > 0)
            .OrderBy(c => c.title)
            .ToListAsync(ct);

        return JsonSerializer.Serialize(courses, _json);
    }

    // ── Tool 2 ─────────────────────────────────────────────────────────────

    [McpServerTool]
    [Description(
        "Search courses by topic keyword. Returns courses whose title contains the keyword " +
        "(case-insensitive). For keywords longer than 3 characters the description is also " +
        "searched. Use this to find courses relevant to a technology or role the user wants " +
        "to practice (e.g. 'angular', 'dotnet', 'azure', 'ai', 'machine learning').")]
    public async Task<string> search_courses(
        [Description("The technology or topic keyword to search for (e.g. 'angular', '.net', 'azure', 'javascript', 'ai')")]
        string topic,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(topic))
            return "[]";

        var lower = topic.Trim().ToLower();

        // Short keywords (≤ 3 chars, e.g. "ai", "mvc", "net") are matched against the
        // title only. Searching descriptions with such short strings produces false
        // positives — e.g. "ai" matches "maintainability", "training", etc.
        bool searchDescription = lower.Length > 3;

        var courses = await db.Courses
            .Select(c => new
            {
                courseId = c.CourseId,
                title = c.Title,
                description = c.Description,
                questionCount = c.Questions.Count()
            })
            .Where(c =>
                c.questionCount > 0 &&
                (c.title.ToLower().Contains(lower) ||
                 (searchDescription && c.description != null && c.description.ToLower().Contains(lower))))
            .OrderBy(c => c.title)
            .ToListAsync(ct);

        return JsonSerializer.Serialize(courses, _json);
    }

    // ── Tool 3 ─────────────────────────────────────────────────────────────

    [McpServerTool]
    [Description(
        "Fetch a random set of question IDs from a specific course. " +
        "Returns an array of question IDs that can later be passed to create_custom_exam. " +
        "Call this for each course you want to include in the custom exam.")]
    public async Task<string> fetch_questions(
        [Description("The courseId to fetch questions from (from list_courses or search_courses)")]
        int courseId,
        [Description("How many questions to fetch from this course (1-20 recommended per course)")]
        int count,
        CancellationToken ct)
    {
        count = Math.Clamp(count, 1, 20);

        var questionIds = await db.Questions
            .Where(q => q.CourseId == courseId)
            .OrderBy(_ => Guid.NewGuid()) // random order
            .Take(count)
            .Select(q => q.QuestionId)
            .ToListAsync(ct);

        return JsonSerializer.Serialize(new
        {
            courseId,
            fetchedCount = questionIds.Count,
            questionIds
        }, _json);
    }

    // ── Tool 4 ─────────────────────────────────────────────────────────────

    [McpServerTool]
    [Description(
        "Create a custom exam with the specified question IDs. " +
        "Call this ONLY after the user has confirmed the exam plan. " +
        "Pass ALL question IDs collected from fetch_questions calls across all chosen courses. " +
        "The userId is supplied securely by the server — do NOT ask the user for it.")]
    public async Task<string> create_custom_exam(
        [Description("All question IDs to include in the exam (combined from multiple fetch_questions calls)")]
        List<int> question_ids,
        [Description("The courseId of the primary/main subject (used for record keeping)")]
        int primary_course_id,
        [Description("The authenticated user's ID — supplied automatically by the server")]
        int user_id,
        CancellationToken ct)
    {
        if (question_ids == null || question_ids.Count == 0)
            return JsonSerializer.Serialize(new { success = false, error = "No question IDs provided." }, _json);

        question_ids = question_ids.Take(60).ToList(); // safety cap

        if (!await db.UserProfiles.AnyAsync(u => u.UserId == user_id, ct))
            return JsonSerializer.Serialize(new { success = false, error = $"User {user_id} not found." }, _json);

        // Validate question IDs against the database before inserting.
        // Claude may carry stale or failed-fetch IDs into this call (e.g. if a previous
        // fetch_questions call partially failed). Any ID not present in Questions is
        // silently dropped here rather than causing a FK constraint violation.
        var validQuestionIds = await db.Questions
            .Where(q => question_ids.Contains(q.QuestionId))
            .Select(q => q.QuestionId)
            .ToListAsync(ct);

        if (validQuestionIds.Count == 0)
            return JsonSerializer.Serialize(new { success = false, error = "None of the provided question IDs exist." }, _json);

        var exam = new LSC.SmartCertify.Domain.Entities.Exam
        {
            CourseId = primary_course_id,
            UserId = user_id,
            Status = "In Progress",
            StartedOn = DateTime.UtcNow,
            IsPracticeMode = true
        };

        await db.Exams.AddAsync(exam, ct);
        await db.SaveChangesAsync(ct);

        var examQuestions = validQuestionIds.Select(qId => new LSC.SmartCertify.Domain.Entities.ExamQuestion
        {
            ExamId = exam.ExamId,
            QuestionId = qId
        }).ToList();

        await db.ExamQuestions.AddRangeAsync(examQuestions, ct);
        await db.SaveChangesAsync(ct);

        return JsonSerializer.Serialize(new
        {
            success = true,
            examId = exam.ExamId,
            questionCount = validQuestionIds.Count,
            message = $"Custom exam created with {validQuestionIds.Count} questions. ExamId: {exam.ExamId}"
        }, _json);
    }
}
