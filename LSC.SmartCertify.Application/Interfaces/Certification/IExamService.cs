using LSC.SmartCertify.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSC.SmartCertify.Application.Interfaces.Certification
{
    public interface IExamService
    {
        Task<ExamDto> StartExamAsync(int courseId, int userId);
        Task UpdateUserChoiceAsync(int id, UpdateUserQuestionChoiceDto dto);
        Task<List<UserExamQuestionsDto>> GetExamQuestionsAsync(int examId);
        Task<List<UserExam>> GetUserExamsAsync(int userId);

        Task<ExamDto?> GetExamMetaData(int examId);
        Task SaveExamStatus(ExamFeedbackDto examFeedback);

        Task<ExamResponseDto> GetExamDetailsAsync(int examId);
    }

}
