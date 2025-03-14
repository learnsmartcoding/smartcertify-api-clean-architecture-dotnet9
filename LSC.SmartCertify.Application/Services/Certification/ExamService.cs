using AutoMapper;
using LSC.SmartCertify.Application.DTOs;
using LSC.SmartCertify.Application.Interfaces.Certification;
using LSC.SmartCertify.Domain.Entities;

namespace LSC.SmartCertify.Application.Services.Certification
{
    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;
        private readonly IMapper mapper;

        public ExamService(IExamRepository examRepository, IMapper mapper)
        {
            _examRepository = examRepository;
            this.mapper = mapper;
        }

        public async Task<ExamDto> StartExamAsync(int courseId, int userId)
        {
            // Fetch 10 random questions for the course
            var questions = await _examRepository.GetRandomQuestionsAsync(courseId, 10);

            if (!questions.Any())
            {
                throw new Exception("No questions found for the specified course.");
            }

            // Create a new exam
            var exam = new Exam
            {
                CourseId = courseId,
                UserId = userId,
                Status = "In Progress",
                StartedOn = DateTime.UtcNow
            };

            // Save exam and associate questions
            await _examRepository.CreateExamWithQuestionsAsync(exam, questions);

            // Map to DTO and return
            return new ExamDto
            {
                ExamId = exam.ExamId,
                CourseId = courseId,
                UserId = userId,
                Status = exam.Status,
                StartedOn = exam.StartedOn,
                QuestionIds = questions.Select(q => q.QuestionId).ToList()
            };
        }

        public async Task UpdateUserChoiceAsync(int id, UpdateUserQuestionChoiceDto dto)
        {
            var examQuestion = await _examRepository.GetExamQuestionAsync(dto.ExamId, dto.ExamQuestionId);
            if (examQuestion == null)
                throw new KeyNotFoundException($"Exam ID {dto.ExamId} with ExamQuestionId {dto.ExamQuestionId} not found.");

            mapper.Map(dto, examQuestion);
            await _examRepository.UpdateExamQuestionAsync(examQuestion);
        }

        public async Task<List<UserExamQuestionsDto>> GetExamQuestionsAsync(int examId)
        {
            // Fetch the exam questions from the repository
            var examQuestions = await _examRepository.GetExamQuestionsAsync(examId);

            // Map the result to a list of UserExamQuestionsDto if not null; otherwise, return an empty list
            return examQuestions != null
                ? mapper.Map<List<UserExamQuestionsDto>>(examQuestions)
                : new List<UserExamQuestionsDto>();
        }

        public async Task<List<UserExam>> GetUserExamsAsync(int userId)
        {
            return await _examRepository.GetUserExamsAsync(userId);
        }

        public Task<ExamDto?> GetExamMetaData(int examId)
        {
            return _examRepository.GetExamMetaDataAsync(examId);
        }

        public async Task SaveExamStatus(ExamFeedbackDto examFeedback)
        {
            await _examRepository.SaveExamStatusAsync(examFeedback.ExamId, examFeedback.Feedback.ToString());
        }

        public async Task<ExamResponseDto> GetExamDetailsAsync(int examId)
        {
            return await _examRepository.GetExamDetailsAsync(examId);
        }
    }

}
