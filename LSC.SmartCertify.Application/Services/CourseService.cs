using AutoMapper;
using LSC.SmartCertify.Application.DTOs;
using LSC.SmartCertify.Application.Interfaces.Courses;
using LSC.SmartCertify.Domain.Entities;

namespace LSC.SmartCertify.Application.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public CourseService(ICourseRepository repository, IMapper mapper)
        {
            _courseRepository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
        {
            var courses = await _courseRepository.GetAllCoursesAsync();
            var courseData = _mapper.Map<IEnumerable<CourseDto>>(courses);
            courseData.ToList().ForEach(c =>
            {
                c.QuestionsAvailable = courses.Any(w => w.CourseId == c.CourseId && w.Questions.Count > 0);
                c.QuestionCount = courses.Where(w => w.CourseId == c.CourseId)
       .SelectMany(s => s.Questions)
       .Count();
            });

            return courseData;
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int courseId)
        {
            var course = await _courseRepository.GetCourseByIdAsync(courseId);
            return course == null ? null : _mapper.Map<CourseDto>(course);
        }

        public async Task<bool> IsTitleDuplicateAsync(string title)
        {
            return await _courseRepository.IsTitleDuplicateAsync(title);
        }

        public async Task AddCourseAsync(CreateCourseDto createCourseDto)
        {
            var course = _mapper.Map<Course>(createCourseDto);
            course.CreatedBy = 1; // Replace with actual user context
            course.CreatedOn = DateTime.UtcNow;

            await _courseRepository.AddCourseAsync(course);
        }

        public async Task UpdateCourseAsync(int courseId, UpdateCourseDto updateCourseDto)
        {
            var course = await _courseRepository.GetCourseByIdAsync(courseId);
            if (course == null) throw new KeyNotFoundException("Course not found");

            _mapper.Map(updateCourseDto, course);
            await _courseRepository.UpdateCourseAsync(course);
        }

        public async Task DeleteCourseAsync(int courseId)
        {
            var course = await _courseRepository.GetCourseByIdAsync(courseId);
            if (course == null) throw new KeyNotFoundException($"Course with id {courseId} not found");

            await _courseRepository.DeleteCourseAsync(course);
        }

        public async Task UpdateDescriptionAsync(int courseId, string description)
        {
            await _courseRepository.UpdateDescriptionAsync(courseId, description);
        }
    }

}
