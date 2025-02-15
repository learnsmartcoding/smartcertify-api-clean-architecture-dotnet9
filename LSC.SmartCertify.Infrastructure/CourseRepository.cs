using LSC.SmartCertify.Application.Interfaces.Courses;
using LSC.SmartCertify.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LSC.SmartCertify.Infrastructure
{
    public class CourseRepository : ICourseRepository
    {
        private readonly SmartCertifyContext _dbContext;

        public CourseRepository(SmartCertifyContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _dbContext.Courses.Include(i=>i.Questions).ToListAsync();
        }

        public async Task<Course?> GetCourseByIdAsync(int courseId)
        {
            return await _dbContext.Courses.FindAsync(courseId);
        }

        public async Task<bool> IsTitleDuplicateAsync(string title)
        {
            return await _dbContext.Courses.AnyAsync(c => c.Title == title);
        }

        public async Task AddCourseAsync(Course course)
        {
            _dbContext.Courses.Add(course);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateCourseAsync(Course course)
        {
            _dbContext.Courses.Update(course);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteCourseAsync(Course course)
        {
            _dbContext.Courses.Remove(course);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateDescriptionAsync(int courseId, string description)
        {
            var course = await _dbContext.Courses.FindAsync(courseId);
            if (course == null) throw new KeyNotFoundException("Course not found.");

            course.Description = description;
            await _dbContext.SaveChangesAsync();
        }

    }

}
