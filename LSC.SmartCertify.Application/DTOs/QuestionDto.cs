using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSC.SmartCertify.Application.DTOs
{
    public class ExamQuestionDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public bool IsCode { get; set; }
        public bool HasMultipleAnswers { get; set; }
        public List<ChoiceDto> Choices { get; set; } = new();
    }

    public class QuestionDto
    {
        public int QuestionId { get; set; }
        public int CourseId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public bool IsCode { get; set; }
        public bool HasMultipleAnswers { get; set; }
        public List<ChoiceDto> Choices { get; set; } = new();   

    }

    public class UserExamQuestionDto
    {        
        public bool IsCorrect { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
    }

    public class CreateQuestionDto
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Question text cannot exceed 500 characters.")]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        [StringLength(20, ErrorMessage = "Difficulty level cannot exceed 20 characters.")]
        public string DifficultyLevel { get; set; } = string.Empty;

        public bool IsCode { get; set; }
        public bool HasMultipleAnswers { get; set; }
    }

    public class UpdateQuestionDto
    {
        [Required]
        [StringLength(500, ErrorMessage = "Question text cannot exceed 500 characters.")]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        [StringLength(20, ErrorMessage = "Difficulty level cannot exceed 20 characters.")]
        public string DifficultyLevel { get; set; } = string.Empty;

        public bool IsCode { get; set; }
        public bool HasMultipleAnswers { get; set; }
    }

}
