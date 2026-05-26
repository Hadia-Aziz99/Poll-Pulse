using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PollpulseBackend.Services;
using EventModel = PollpulseBackend.Models.Event;

namespace PollpulseBackend.Controllers
{
    [Authorize(Roles = "admin")]
    [ApiController]
    [Route("api/admin/crud/events")]
    public class AdminEventsController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;

        public AdminEventsController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var eventsList = await _mongoDBService.Events.Find(_ => true).SortByDescending(e => e.EventDate).ToListAsync();
            return Ok(eventsList);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var eventObj = await _mongoDBService.Events.Find(e => e.Id == id).FirstOrDefaultAsync();
            return eventObj == null ? NotFound(new { msg = "Event not found." }) : Ok(eventObj);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EventUpsertRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { msg = "Event name is required." });
            }

            var eventObj = new EventModel
            {
                Name = request.Name.Trim(),
                EventDate = request.EventDate,
                Description = (request.Description ?? string.Empty).Trim(),
                Status = string.IsNullOrWhiteSpace(request.Status) ? "completed" : request.Status.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _mongoDBService.Events.InsertOneAsync(eventObj);
            return CreatedAtAction(nameof(GetById), new { id = eventObj.Id }, eventObj);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] EventUpsertRequest request)
        {
            var eventObj = await _mongoDBService.Events.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (eventObj == null)
            {
                return NotFound(new { msg = "Event not found." });
            }
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { msg = "Event name is required." });
            }

            eventObj.Name = request.Name.Trim();
            eventObj.EventDate = request.EventDate;
            eventObj.Description = (request.Description ?? string.Empty).Trim();
            eventObj.Status = string.IsNullOrWhiteSpace(request.Status) ? "completed" : request.Status.Trim();
            eventObj.UpdatedAt = DateTime.UtcNow;

            await _mongoDBService.Events.ReplaceOneAsync(e => e.Id == id, eventObj);
            return Ok(eventObj);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _mongoDBService.Events.DeleteOneAsync(e => e.Id == id);
            return result.DeletedCount == 0 ? NotFound(new { msg = "Event not found." }) : Ok(new { msg = "Event deleted successfully." });
        }
    }

    public class EventUpsertRequest
    {
        public string Name { get; set; } = string.Empty;
        public DateTime? EventDate { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
    }
}
