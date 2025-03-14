using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSC.SmartCertify.Application.DTOs
{
    // DTO for encapsulated exam data
    public class ExamResponseDto
    {
        public int ExamId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime? FinishedOn { get; set; }
        public List<UserExamQuestionDto> Questions { get; set; } = new();
    }

   

}
