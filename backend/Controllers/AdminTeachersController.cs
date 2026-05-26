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
    [Route("api/admin/crud/teachers")]
    public class AdminTeachersController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;

        public AdminTeachersController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var teachers = await _mongoDBService.Teachers.Find(_ => true).SortBy(t => t.Name).ToListAsync();
            return Ok(teachers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var teacher = await _mongoDBService.Teachers.Find(t => t.Id == id).FirstOrDefaultAsync();
            return teacher == null ? NotFound(new { msg = "Teacher not found." }) : Ok(teacher);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TeacherUpsertRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { msg = "Teacher name is required." });
            }

            var teacher = new Teacher
            {
                Name = request.Name.Trim(),
                Email = (request.Email ?? string.Empty).Trim().ToLower(),
                Department = string.IsNullOrWhiteSpace(request.Department) ? "Computer Science" : request.Department.Trim(),
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _mongoDBService.Teachers.InsertOneAsync(teacher);
            return CreatedAtAction(nameof(GetById), new { id = teacher.Id }, teacher);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] TeacherUpsertRequest request)
        {
            var teacher = await _mongoDBService.Teachers.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (teacher == null)
            {
                return NotFound(new { msg = "Teacher not found." });
            }
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { msg = "Teacher name is required." });
            }

            teacher.Name = request.Name.Trim();
            teacher.Email = (request.Email ?? string.Empty).Trim().ToLower();
            teacher.Department = string.IsNullOrWhiteSpace(request.Department) ? "Computer Science" : request.Department.Trim();
            teacher.IsActive = request.IsActive;
            teacher.UpdatedAt = DateTime.UtcNow;

            await _mongoDBService.Teachers.ReplaceOneAsync(t => t.Id == id, teacher);
            return Ok(teacher);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _mongoDBService.Teachers.DeleteOneAsync(t => t.Id == id);
            return result.DeletedCount == 0 ? NotFound(new { msg = "Teacher not found." }) : Ok(new { msg = "Teacher deleted successfully." });
        }
    }

    public class TeacherUpsertRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Department { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
