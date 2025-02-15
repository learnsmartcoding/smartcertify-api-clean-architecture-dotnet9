using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSC.SmartCertify.Application.DTOs
{
    public class ChoiceDto
    {
        public int ChoiceId { get; set; }
        public int QuestionId { get; set; }
        public string ChoiceText { get; set; } = string.Empty;
        public bool IsCode { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class CreateChoiceDto
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Choice text cannot exceed 200 characters.")]
        public string ChoiceText { get; set; } = string.Empty;

        public bool IsCode { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class UpdateChoiceDto: UpdateUserChoice
    {
        [Required]
        [StringLength(200, ErrorMessage = "Choice text cannot exceed 200 characters.")]
        public string ChoiceText { get; set; } = string.Empty;

        public bool IsCode { get; set; }
        
    }

    public class UpdateUserChoice
    {
        public int ChoiceId { get; set; }
        public bool IsCorrect { get; set; }
    }

}
