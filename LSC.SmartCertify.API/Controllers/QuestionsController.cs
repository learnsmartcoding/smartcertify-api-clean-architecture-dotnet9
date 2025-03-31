using LSC.SmartCertify.API.Filters.LSC.OnlineCourse.API.Common;
using LSC.SmartCertify.Application.DTOs;
using LSC.SmartCertify.Application.Interfaces.Common;
using LSC.SmartCertify.Application.Interfaces.QuestionsChoice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace LSC.SmartCertify.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes:Read")]
    [Authorize]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _service;
        private readonly IUserClaims userClaims;

        public QuestionsController(IQuestionService service, IUserClaims userClaims)
        {
            _service = service;
            this.userClaims = userClaims;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestionDto>>> GetQuestions()
        {
            return Ok(await _service.GetAllQuestionsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionDto>> GetQuestion(int id)
        {
            var question = await _service.GetQuestionByIdAsync(id);
            var isAdmin = this.userClaims.GetUserRoles().Any(s => s.ToLower().Equals("admin"));
            if (!isAdmin)
            {
                //let's mark choice's answer as false so we dont let user know the answer
                question?.Choices.ForEach(c => c.IsCorrect = false);
            }
            return question == null ? NotFound() : Ok(question);
        }

        [HttpPost]
        [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes:Write")]
        [Authorize]
        [AdminRole]
        public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionDto dto)
        {
            await _service.AddQuestionAsync(dto);
            return CreatedAtAction(nameof(GetQuestion), new { id = dto.CourseId }, dto);
        }

        [HttpPut("{id}")]
        [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes:Write")]
        [Authorize]
        [AdminRole]
        public async Task<IActionResult> UpdateQuestion(int id, [FromBody] UpdateQuestionDto dto)
        {
            await _service.UpdateQuestionAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes:Write")]
        [Authorize]
        [AdminRole]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            await _service.DeleteQuestionAsync(id);
            return NoContent();
        }


        [HttpPost("CreateQuestionChoices")]
        [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes:Write")]
        [Authorize]
        [AdminRole]
        public async Task<IActionResult> CreateQuestionChoices([FromBody] QuestionDto dto)
        {
            var createdResource = await _service.AddQuestionAndChoicesAsync(dto);
            return CreatedAtAction(nameof(GetQuestion), new { id = createdResource.QuestionId }, createdResource);
        }

        [HttpPut("UpdateQuestionAndChoices/{id}")]
        [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes:Write")]
        [Authorize]
        [AdminRole]
        public async Task<IActionResult> UpdateQuestionAndChoices(int id, [FromBody] QuestionDto dto)
        {
            await _service.UpdateQuestionAndChoicesAsync(id, dto);
            return NoContent();
        }
    }

}
