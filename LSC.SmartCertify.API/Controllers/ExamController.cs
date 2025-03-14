using LSC.SmartCertify.Application.DTOs;
using LSC.SmartCertify.Application.DTOValidations;
using LSC.SmartCertify.Application.Interfaces.Certification;
using LSC.SmartCertify.Application.Interfaces.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LSC.SmartCertify.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExamController : ControllerBase
    {
        private readonly IExamService _examService;
        private readonly IUserClaims userClaims;

        public ExamController(IExamService examService, IUserClaims userClaims)
        {
            _examService = examService;
            this.userClaims = userClaims;
        }

        [HttpPost("start-exam")]
        public async Task<IActionResult> StartExam([FromBody] StartExamRequest request)
        {
            // Validate the input using FluentValidation
            var validator = new StartExamRequestValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            try
            {
                var result = await _examService.StartExamAsync(request.CourseId, request.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        /// <summary>
        /// id is examQuestionId
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("update-user-choice/{id}")]
        public async Task<IActionResult> UpdateUserChoice(int id, [FromBody] UpdateUserQuestionChoiceDto dto)
        {
            //TODO Validation of request model pending
            await _examService.UpdateUserChoiceAsync(id, dto);
            return NoContent();
        }

        [HttpGet("get-user-exam-questions/{examId}")]
        public async Task<IActionResult> GetUserExamQuestions(int examId)
        {
            //TODO Validation of request model pending
            var result = await _examService.GetExamQuestionsAsync(examId);
            return Ok(result);
        }


        [HttpGet("get-user-exams/{userId}")]
        public async Task<IActionResult> GetUserExams(int userId = 0)
        {
            userId = userId == 0 ? userClaims.GetUserId() : userId;

            //TODO Validation of request model pending
            var result = await _examService.GetUserExamsAsync(userId);
            return Ok(result);
        }

        [HttpGet("exam-meta-data/{examId}")]
        public async Task<IActionResult> GetExamMetaData(int examId)
        {
            //TODO Validation of request model pending
            var result = await _examService.GetExamMetaData(examId);

            if (result == null)
                return NotFound();

            if (result.UserId != userClaims.GetUserId())
            {
                return new ForbidResult();
            }
            return Ok(result);
        }

        [HttpPut("update-exam-status/{examId}")]
        public async Task<IActionResult> UpdateExamStatus(int examId, [FromBody] ExamFeedbackDto feedback)
        {           
            //TODO Validation of request model pending
            await _examService.SaveExamStatus(feedback);
            return NoContent();
        }

        [HttpGet("exam-details/{examId}")]
        public async Task<IActionResult> GetExamDetails(int examId)
        {
            var result = await _examService.GetExamDetailsAsync(examId);

            if (result == null)
                return NotFound(new { Message = "Exam not found." });

            return Ok(result);
        }

    }

}
