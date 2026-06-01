using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PollpulseBackend.Models;
using PollpulseBackend.Services;

namespace PollpulseBackend.Controllers
{
    [ApiController]
    [Route("api/contacts")]
    public class ContactController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;

        public ContactController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitContactMessage([FromBody] ContactSubmitRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { msg = "Please provide valid contact details." });
            }

            if (request.Rating < 1 || request.Rating > 5)
            {
                return BadRequest(new { msg = "Rating must be between 1 and 5." });
            }

            var contactMessage = new ContactMessage
            {
                Name = request.Name.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                Rating = request.Rating,
                Message = request.Message.Trim(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                UserAgent = Request.Headers["User-Agent"].ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _mongoDBService.ContactMessages.InsertOneAsync(contactMessage);

            return Ok(new
            {
                msg = "Contact message submitted successfully.",
                id = contactMessage.Id
            });
        }
    }

    public class ContactSubmitRequest
    {
        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [MinLength(5)]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;
    }
}