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
    [Route("api/admin/crud/classes")]
    public class AdminClassSectionsController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;

        public AdminClassSectionsController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var classes = await _mongoDBService.ClassSections.Find(_ => true)
                .SortBy(c => c.Degree).ThenBy(c => c.Year).ThenBy(c => c.Section)
                .ToListAsync();
            return Ok(classes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var classSection = await _mongoDBService.ClassSections.Find(c => c.Id == id).FirstOrDefaultAsync();
            return classSection == null ? NotFound(new { msg = "Class section not found." }) : Ok(classSection);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClassSectionUpsertRequest request)
        {
            if (!IsValidRequest(request, out var validationMessage))
            {
                return BadRequest(new { msg = validationMessage });
            }

            var classSection = BuildClassSection(request);
            classSection.CreatedAt = DateTime.UtcNow;
            classSection.UpdatedAt = DateTime.UtcNow;

            await _mongoDBService.ClassSections.InsertOneAsync(classSection);
            return CreatedAtAction(nameof(GetById), new { id = classSection.Id }, classSection);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] ClassSectionUpsertRequest request)
        {
            var classSection = await _mongoDBService.ClassSections.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (classSection == null)
            {
                return NotFound(new { msg = "Class section not found." });
            }
            if (!IsValidRequest(request, out var validationMessage))
            {
                return BadRequest(new { msg = validationMessage });
            }

            classSection.Degree = request.Degree.Trim().ToUpper();
            classSection.Year = request.Year;
            classSection.Section = request.Section.Trim().ToUpper();
            classSection.ClassKey = $"{classSection.Degree}-Y{classSection.Year}-{classSection.Section}";
            classSection.Title = string.IsNullOrWhiteSpace(request.Title) ? classSection.ClassKey : request.Title.Trim();
            classSection.IsActive = request.IsActive;
            classSection.UpdatedAt = DateTime.UtcNow;

            await _mongoDBService.ClassSections.ReplaceOneAsync(c => c.Id == id, classSection);
            return Ok(classSection);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _mongoDBService.ClassSections.DeleteOneAsync(c => c.Id == id);
            return result.DeletedCount == 0 ? NotFound(new { msg = "Class section not found." }) : Ok(new { msg = "Class section deleted successfully." });
        }

        private static bool IsValidRequest(ClassSectionUpsertRequest request, out string message)
        {
            if (string.IsNullOrWhiteSpace(request.Degree) || request.Year < 1 || request.Year > 6 || string.IsNullOrWhiteSpace(request.Section))
            {
                message = "Degree, year between 1 and 6, and section are required.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private static ClassSection BuildClassSection(ClassSectionUpsertRequest request)
        {
            var degree = request.Degree.Trim().ToUpper();
            var section = request.Section.Trim().ToUpper();
            var classKey = $"{degree}-Y{request.Year}-{section}";

            return new ClassSection
            {
                Degree = degree,
                Year = request.Year,
                Section = section,
                ClassKey = classKey,
                Title = string.IsNullOrWhiteSpace(request.Title) ? classKey : request.Title.Trim(),
                IsActive = request.IsActive
            };
        }
    }

    public class ClassSectionUpsertRequest
    {
        public string Degree { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Section { get; set; } = string.Empty;
        public string? Title { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
