using LSC.SmartCertify.Application.DTOs;
using LSC.SmartCertify.Application.DTOValidations;
using LSC.SmartCertify.Application.Interfaces.Certification;
using LSC.SmartCertify.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LSC.SmartCertify.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExamController : ControllerBase
    {
        private readonly IExamService _examService;
        private readonly ICurrentUserService currentUserService;

        public ExamController(IExamService examService, ICurrentUserService currentUserService)
        {
            _examService = examService;
            this.currentUserService = currentUserService;
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
                var currentUser = await currentUserService.GetCurrentUserProfileAsync();
                if (currentUser is null)
                {
                    return Unauthorized("Current user was not found in UserProfile.");
                }

                var result = await _examService.StartExamAsync(request.CourseId, currentUser.UserId, request.IsPracticeMode, request.NoOfQuestions);
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
            var ownershipResult = await ValidateCurrentUserExamAsync(dto.ExamId);
            if (ownershipResult is not null)
            {
                return ownershipResult;
            }

            //TODO Validation of request model pending
            await _examService.UpdateUserChoiceAsync(id, dto);
            return NoContent();
        }

        [HttpGet("get-user-exam-questions/{examId}")]
        public async Task<IActionResult> GetUserExamQuestions(int examId)
        {
            var ownershipResult = await ValidateCurrentUserExamAsync(examId);
            if (ownershipResult is not null)
            {
                return ownershipResult;
            }

            //TODO Validation of request model pending
            var result = await _examService.GetExamQuestionsAsync(examId);
            return Ok(result);
        }


        [HttpGet("get-user-exams/{userId?}")]
        public async Task<IActionResult> GetUserExams()
        {
            var currentUser = await currentUserService.GetCurrentUserProfileAsync();
            if (currentUser is null)
            {
                return Unauthorized("Current user was not found in UserProfile.");
            }

            //TODO Validation of request model pending
            var result = await _examService.GetUserExamsAsync(currentUser.UserId);
            return Ok(result);
        }

        [HttpGet("exam-meta-data/{examId}")]
        public async Task<IActionResult> GetExamMetaData(int examId)
        {
            //TODO Validation of request model pending
            var result = await _examService.GetExamMetaData(examId);

            if (result == null)
                return NotFound();

            var currentUser = await currentUserService.GetCurrentUserProfileAsync();
            if (currentUser is null)
            {
                return Unauthorized("Current user was not found in UserProfile.");
            }

            if (result.UserId != currentUser.UserId)
            {
                return new ForbidResult();
            }
            return Ok(result);
        }

        [HttpPut("update-exam-status/{examId}")]
        public async Task<IActionResult> UpdateExamStatus(int examId, [FromBody] ExamFeedbackDto feedback)
        {
            var ownershipResult = await ValidateCurrentUserExamAsync(examId);
            if (ownershipResult is not null)
            {
                return ownershipResult;
            }

            feedback.ExamId = examId;

            //TODO Validation of request model pending
            await _examService.SaveExamStatus(feedback);
            return NoContent();
        }

        [HttpGet("exam-details/{examId}")]
        public async Task<IActionResult> GetExamDetails(int examId)
        {
            var ownershipResult = await ValidateCurrentUserExamAsync(examId);
            if (ownershipResult is not null)
            {
                return ownershipResult;
            }

            var result = await _examService.GetExamDetailsAsync(examId);

            if (result == null)
                return NotFound(new { Message = "Exam not found." });

            return Ok(result);
        }

        /// <summary>
        /// Creates a custom multi-course exam from AI chat-selected question IDs.
        /// Called by ChatController after the AI assistant builds an exam plan.
        /// </summary>
        [HttpPost("start-custom-exam")]
        public async Task<IActionResult> StartCustomExam([FromBody] StartCustomExamRequest request)
        {
            if (request.QuestionIds == null || request.QuestionIds.Count == 0)
                return BadRequest("At least one question ID is required.");

            if (request.QuestionIds.Count > 60)
                return BadRequest("Maximum 60 questions allowed per custom exam.");

            if (request.PrimaryCourseId <= 0)
                return BadRequest("A valid PrimaryCourseId is required.");

            try
            {
                var currentUser = await currentUserService.GetCurrentUserProfileAsync();
                if (currentUser is null)
                    return Unauthorized("Current user was not found in UserProfile.");

                var result = await _examService.StartCustomExamAsync(
                    currentUser.UserId,
                    request.PrimaryCourseId,
                    request.QuestionIds,
                    request.IsPracticeMode);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task<IActionResult?> ValidateCurrentUserExamAsync(int examId)
        {
            var currentUser = await currentUserService.GetCurrentUserProfileAsync();
            if (currentUser is null)
            {
                return Unauthorized("Current user was not found in UserProfile.");
            }

            var exam = await _examService.GetExamMetaData(examId);
            if (exam is null)
            {
                return NotFound();
            }

            return exam.UserId == currentUser.UserId ? null : Forbid();
        }

    }

}
