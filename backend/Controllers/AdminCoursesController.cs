using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PollpulseBackend.Models;
using PollpulseBackend.Services;

namespace PollpulseBackend.Controllers
{
    [Authorize(Roles = "admin")]
    [ApiController]
    [Route("api/admin/crud/courses")]
    public class AdminCoursesController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;

        public AdminCoursesController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courses = await _mongoDBService.Courses.Find(_ => true).SortBy(c => c.Code).ToListAsync();
            return Ok(courses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var course = await _mongoDBService.Courses.Find(c => c.Id == id).FirstOrDefaultAsync();
            return course == null ? NotFound(new { msg = "Course not found." }) : Ok(course);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CourseUpsertRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { msg = "Course code and title are required." });
            }

            var course = new Course
            {
                Code = request.Code.Trim().ToUpper(),
                Title = request.Title.Trim(),
                Degree = string.IsNullOrWhiteSpace(request.Degree) ? "Both" : request.Degree.Trim(),
                Year = request.Year,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _mongoDBService.Courses.InsertOneAsync(course);
            return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CourseUpsertRequest request)
        {
            var course = await _mongoDBService.Courses.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (course == null)
            {
                return NotFound(new { msg = "Course not found." });
            }
            if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { msg = "Course code and title are required." });
            }

            course.Code = request.Code.Trim().ToUpper();
            course.Title = request.Title.Trim();
            course.Degree = string.IsNullOrWhiteSpace(request.Degree) ? "Both" : request.Degree.Trim();
            course.Year = request.Year;
            course.IsActive = request.IsActive;
            course.UpdatedAt = DateTime.UtcNow;

            await _mongoDBService.Courses.ReplaceOneAsync(c => c.Id == id, course);
            return Ok(course);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _mongoDBService.Courses.DeleteOneAsync(c => c.Id == id);
            return result.DeletedCount == 0 ? NotFound(new { msg = "Course not found." }) : Ok(new { msg = "Course deleted successfully." });
        }
    }

    public class CourseUpsertRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Degree { get; set; }
        public int? Year { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
