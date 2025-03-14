using LSC.SmartCertify.Application.DTOs;
using LSC.SmartCertify.Application.Interfaces.Certification;
using LSC.SmartCertify.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LSC.SmartCertify.Infrastructure
{
    public class ExamRepository : IExamRepository
    {
        private readonly SmartCertifyContext _context;

        public ExamRepository(SmartCertifyContext context)
        {
            _context = context;
        }

        public async Task<List<Question>> GetRandomQuestionsAsync(int courseId, int count)
        {
            var mainCourses = new List<string>() { "Angular", ".NET Core", "Azure" };
            var courseIds = new List<int>();
            var courses = await _context.Courses.FindAsync(courseId);
            mainCourses.ForEach(e =>
            {
                if (e.ToLower().Equals(courses?.Title.ToLower()))
                {
                    courseIds = _context.Courses.Where(w => w.Title.ToLower().
                    StartsWith(courses.Title.ToLower())).Select(s => s.CourseId).ToList();
                }
            });

            if (courseIds.Any())
            {
                return await _context.Questions
                                         .Where(q => courseIds.Contains(q.CourseId))
                                         .OrderBy(q => Guid.NewGuid())
                                         .Take(count)
                                         .ToListAsync();
            }
            else
            {
                return await _context.Questions
                            .Where(q => q.CourseId == courseId)
                            .OrderBy(q => Guid.NewGuid())
                            .Take(count)
                            .ToListAsync();
            }
        }

        public async Task CreateExamWithQuestionsAsync(Exam exam, List<Question> questions)
        {
            if (!await _context.UserProfiles.AnyAsync(u => u.UserId == exam.UserId))
            {
                throw new Exception($"UserId {exam.UserId} does not exist in the database.");
            }


            await _context.Exams.AddAsync(exam);
            await _context.SaveChangesAsync();

            var examQuestions = questions.Select(q => new ExamQuestion
            {
                ExamId = exam.ExamId,
                QuestionId = q.QuestionId
            }).ToList();

            await _context.ExamQuestions.AddRangeAsync(examQuestions);
            await _context.SaveChangesAsync();
        }

        public Task<ExamQuestion?> GetExamQuestionAsync(int examId, int examQuestionId)
        {
            return _context.ExamQuestions
                        .FirstOrDefaultAsync(eq => eq.ExamId == examId && eq.ExamQuestionId == examQuestionId);
        }

        public async Task UpdateExamQuestionAsync(ExamQuestion examQuestion)
        {
            _context.ExamQuestions.Update(examQuestion);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ExamQuestion>> GetExamQuestionsAsync(int examId)
        {
            return await _context.ExamQuestions
                        .Where(eq => eq.ExamId == examId)
                        .ToListAsync();
        }

        public async Task<List<UserExam>> GetUserExamsAsync(int userId)
        {
            var result = await _context.Exams
                .Join(_context.Courses,
                      exam => exam.CourseId,
                      course => course.CourseId,
                      (exam, course) => new { exam, course })
                .Where(e => e.exam.UserId == userId)
                .Select(e => new UserExam()
                {
                    ExamId = e.exam.ExamId,
                    CourseId = e.exam.CourseId,
                    Title = e.course.Title,
                    Description = e.course.Description,
                    Status = e.exam.Status,
                    StartedOn = e.exam.StartedOn,
                    FinishedOn = e.exam.FinishedOn
                }).ToListAsync();


            return result;

        }

        public async Task<ExamDto?> GetExamMetaDataAsync(int examId)
        {
            return await _context.Exams.Include(i => i.ExamQuestions).Select(s => new ExamDto()
            {
                ExamId = s.ExamId,
                CourseId = s.CourseId,
                UserId = s.UserId,
                Status = s.Status,
                StartedOn = s.StartedOn,
                FinishedOn = s.FinishedOn,
                QuestionIds = s.ExamQuestions.Select(s => s.QuestionId).ToList()
            }).FirstOrDefaultAsync(w => w.ExamId == examId);
        }

        public async Task SaveExamStatusAsync(int examId, string feedback)
        {
            var existingExam = await _context.Exams.FirstOrDefaultAsync(e => e.ExamId == examId);
            if (existingExam != null)
            {
                existingExam.Feedback = feedback;
                existingExam.Status = "Completed";
                existingExam.FinishedOn = DateTime.UtcNow;
                _context.Exams.Update(existingExam);
                await _context.SaveChangesAsync();
                await UpdateExamResults(examId);
            }
        }

        protected internal async Task UpdateExamResults(int examId)
        {
            // Fetch all exam questions for the given ExamId
            var examQuestions = await _context.ExamQuestions
                .Where(eq => eq.ExamId == examId)
                .ToListAsync();

            // Fetch all relevant choices for the questions in the exam
            var questionIds = examQuestions.Select(eq => eq.QuestionId).Distinct();
            var correctChoices = await _context.Choices
                .Where(c => questionIds.Contains(c.QuestionId) && c.IsCorrect)
                .ToListAsync();

            // Create a dictionary for quick lookup of correct choices by QuestionId
            var correctChoiceMap = correctChoices
                .GroupBy(c => c.QuestionId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(c => c.ChoiceId).ToList()
                );

            // Update the IsCorrect field for each ExamQuestion
            foreach (var eq in examQuestions)
            {
                if (eq.SelectedChoiceId.HasValue && correctChoiceMap.ContainsKey(eq.QuestionId))
                {
                    eq.IsCorrect = correctChoiceMap[eq.QuestionId].Contains(eq.SelectedChoiceId.Value);
                }
                else
                {
                    eq.IsCorrect = false; // No correct choice or no selection made
                }
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
        }


        public async Task<ExamResponseDto> GetExamDetailsAsync(int examId)
        {
            var examData = await (from exam in _context.Exams
                                  join course in _context.Courses on exam.CourseId equals course.CourseId
                                  join eq in _context.ExamQuestions on exam.ExamId equals eq.ExamId
                                  join question in _context.Questions on eq.QuestionId equals question.QuestionId
                                  where exam.ExamId == examId
                                  select new
                                  {
                                      exam.ExamId,
                                      course.Title,
                                      exam.Status,
                                      exam.StartedOn,
                                      exam.FinishedOn,
                                      question.QuestionText,
                                      eq.IsCorrect,
                                      question.DifficultyLevel
                                  }).ToListAsync();

            if (!examData.Any()) return null;

            // Encapsulate shared data
            var firstRow = examData.First();
            var response = new ExamResponseDto
            {
                ExamId = firstRow.ExamId,
                Title = firstRow.Title,
                Status = firstRow.Status,
                StartedOn = firstRow.StartedOn,
                FinishedOn = firstRow.FinishedOn,
                Questions = examData.Select(e => new UserExamQuestionDto
                {
                    QuestionText = e.QuestionText,                    
                    IsCorrect = Convert.ToBoolean(e.IsCorrect),
                    DifficultyLevel = e.DifficultyLevel                    
                }).ToList()
            };

            return response;
        }

    }

}
