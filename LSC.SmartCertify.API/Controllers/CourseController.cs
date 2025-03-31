using FluentValidation;
using LSC.SmartCertify.API.Filters.LSC.OnlineCourse.API.Common;
using LSC.SmartCertify.Application.DTOs;
using LSC.SmartCertify.Application.Interfaces.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace LSC.SmartCertify.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _service;
        private readonly IValidator<CreateCourseDto> validator;
        private readonly IValidator<UpdateCourseDto> updateValidator;

        public CoursesController(
            ICourseService service,
            IValidator<CreateCourseDto> validator,
            IValidator<UpdateCourseDto> updateValidator)
        {
            _service = service;
            this.validator = validator;
            this.updateValidator = updateValidator;
        }

        /// <summary>
        /// Retrieves all courses.
        /// </summary>
        /// <returns>A list of courses.</returns>
        /// <response code="200">Returns the list of courses.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CourseDto>), StatusCodes.Status200OK)]        
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
        {            
            var mainCourses = new List<string> { "Angular", ".NET Core", "Azure" };
            var model = await _service.GetAllCoursesAsync();

            foreach (var course in mainCourses)
            {
                var mainCourseItem = model.FirstOrDefault(w =>
                    w.Title.Equals(course, StringComparison.OrdinalIgnoreCase));

                if (mainCourseItem != null)
                {
                    mainCourseItem.QuestionCount = model
                        .Where(w => w.Title.StartsWith(course, StringComparison.OrdinalIgnoreCase))
                        .Sum(s => s.QuestionCount);
                }
            }


            return Ok(model);
        }

        /// <summary>
        /// Retrieves a specific course by ID.
        /// </summary>
        /// <param name="id">The ID of the course to retrieve.</param>
        /// <returns>The course with the specified ID.</returns>
        /// <response code="200">Returns the course if found.</response>
        /// <response code="404">If the course is not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]        
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [AllowAnonymous]
        public async Task<ActionResult<CourseDto>> GetCourse(int id)
        {
            var course = await _service.GetCourseByIdAsync(id);
            return course == null ? NotFound() : Ok(course);
        }

        /// <summary>
        /// Creates a new course.
        /// </summary>
        /// <param name="createCourseDto">The details of the course to create.</param>
        /// <returns>The newly created course.</returns>
        /// <response code="201">Returns the created course.</response>
        /// <response code="400">If the input is invalid.</response>
        [HttpPost]
        [ProducesResponseType(typeof(CreateCourseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(IEnumerable<FluentValidation.Results.ValidationFailure>), StatusCodes.Status400BadRequest)]        
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes:Write")]        
        [AdminRole]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto createCourseDto)
        {
            var validationResult = await validator.ValidateAsync(createCourseDto);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            await _service.AddCourseAsync(createCourseDto);
            return CreatedAtAction(nameof(GetCourse), new { id = createCourseDto.Title }, createCourseDto);
        }

        /// <summary>
        /// Updates an existing course.
        /// </summary>
        /// <param name="id">The ID of the course to update.</param>
        /// <param name="updateCourseDto">The updated course details.</param>
        /// <response code="204">Indicates the update was successful.</response>
        /// <response code="400">If the input is invalid.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(IEnumerable<FluentValidation.Results.ValidationFailure>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes:Write")]
        [AdminRole]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto updateCourseDto)
        {
            var validationResult = await updateValidator.ValidateAsync(updateCourseDto);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            await _service.UpdateCourseAsync(id, updateCourseDto);
            return NoContent();
        }

        /// <summary>
        /// Deletes a course.
        /// </summary>
        /// <param name="id">The ID of the course to delete.</param>
        /// <response code="204">Indicates the deletion was successful.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes:Write")]
        [AdminRole]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            await _service.DeleteCourseAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Updates the description of a course.
        /// </summary>
        /// <param name="id">The ID of the course to update.</param>
        /// <param name="model">The updated course description.</param>
        /// <response code="204">Indicates the update was successful.</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes:Write")]
        [AdminRole]
        public async Task<IActionResult> UpdateDescription([FromRoute] int id, [FromBody] CourseUpdateDescriptionDto model)
        {
            await _service.UpdateDescriptionAsync(id, model.Description);
            return NoContent();
        }
    }


}
