namespace LSC.SmartCertify.Application.DTOs
{
    public class ExamFeedbackDto
    {
        public int ExamId { get; set; }
        public string Feedback { get; set; }
    }
    public class ExamDto
    {
        public int ExamId { get; set; }
        public int CourseId { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime StartedOn { get; set; }
        public DateTime? FinishedOn { get; set; }
        public List<int> QuestionIds { get; set; } = new List<int>();
    }

    public class StartExamRequest
    {
        public int CourseId { get; set; }
        public int UserId { get; set; }
    }

    public class UserExamQuestionsDto : UpdateUserQuestionChoiceDto
    {
        public int QuestionId { get; set; }
        public bool IsCorrect { get; set; }
    }
    public class UpdateUserQuestionChoiceDto
    {
        public int ExamId { get; set; }
        public int ExamQuestionId { get; set; }
        public int SelectedChoiceId { get; set; }
        public bool ReviewLater { get; set; }

    }

    public class UserExam
    {
        public int ExamId { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;

        public string? Description { get; set; }
        public string Status { get; set; } = null!;

        public DateTime StartedOn { get; set; }

        public DateTime? FinishedOn { get; set; }
    }
}