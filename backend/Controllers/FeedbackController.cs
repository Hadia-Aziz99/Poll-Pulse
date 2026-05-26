using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PollpulseBackend.Models;
using PollpulseBackend.Services;

namespace PollpulseBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/feedbacks")]
    public class FeedbackController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;
        private readonly SentimentService _sentimentService;

        private readonly Dictionary<string, string> _categoryLabels = new Dictionary<string, string>
        {
            { "faculty", "Faculty Feedback" },
            { "teacher", "Teacher Feedback" },
            { "course", "Course Feedback" },
            { "transport", "Transport Feedback" },
            { "cafeteria", "Cafeteria Food Feedback" },
            { "sports", "Sports Facilities Feedback" },
            { "library", "Library Feedback" },
            { "event", "Events Feedback" }
        };

        public FeedbackController(MongoDBService mongoDBService, SentimentService sentimentService)
        {
            _mongoDBService = mongoDBService;
            _sentimentService = sentimentService;
        }

        private string GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        private string GetCurrentClassKey() => User.FindFirst("classKey")?.Value ?? "";
        private string GetClaimValue(string type) => User.FindFirst(type)?.Value ?? "";

        [HttpGet("masters")]
        public async Task<IActionResult> GetMasters()
        {
            var teachers = await _mongoDBService.Teachers.Find(t => t.IsActive).SortBy(t => t.Name).ToListAsync();
            var courses = await _mongoDBService.Courses.Find(c => c.IsActive).SortBy(c => c.Code).ToListAsync();
            var events = await _mongoDBService.Events.Find(e => e.Status == "completed" || e.Status == "upcoming")
                .SortByDescending(e => e.EventDate)
                .Limit(20)
                .ToListAsync();

            return Ok(new
            {
                teachers = teachers.Select(t => new { t.Id, t.Name, t.Department }),
                courses = courses.Select(c => new { c.Id, c.Code, c.Title, c.Degree }),
                events = events.Select(e => new { e.Id, e.Name, e.EventDate })
            });
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentFeedbacks()
        {
            var userId = GetCurrentUserId();
            var feedbacks = await _mongoDBService.Feedbacks
                .Find(f => f.User == userId)
                .SortByDescending(f => f.CreatedAt)
                .Limit(10)
                .ToListAsync();

            return Ok(feedbacks.Select(f => new
            {
                f.Id,
                f.Category,
                f.TargetName,
                f.AverageRating,
                sentiment = f.Sentiment.Label,
                f.CreatedAt
            }));
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackSubmitRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Category) || !_categoryLabels.ContainsKey(request.Category))
            {
                return BadRequest(new { msg = "Select a valid feedback category." });
            }

            if (string.IsNullOrWhiteSpace(request.Comment) || request.Comment.Length < 5 || request.Comment.Length > 1200)
            {
                return BadRequest(new { msg = "Comment must be between 5 and 1200 characters." });
            }

            var userId = GetCurrentUserId();
            var degree = GetClaimValue("degree");
            var yearStr = GetClaimValue("year");
            var section = GetClaimValue("section");
            var classKey = GetCurrentClassKey();

            int.TryParse(yearStr, out int year);

            // Compute Ratings and Average
            var ratingLabels = GetRatingLabelsForCategory(request.Category);
            var ratingsList = new List<Rating>();

            var values = new[] { request.Rating1, request.Rating2, request.Rating3, request.Rating4, request.RatingOverall };
            for (int i = 0; i < ratingLabels.Length; i++)
            {
                int val = Math.Min(5, Math.Max(1, values[i] == 0 ? request.RatingOverall : values[i]));
                ratingsList.Add(new Rating { Label = ratingLabels[i], Value = val });
            }

            double averageRating = Math.Round(ratingsList.Average(r => r.Value), 2);

            // Determine Target Name
            string targetName = request.TargetName ?? "";
            if (string.IsNullOrWhiteSpace(targetName))
            {
                if (request.Category == "teacher" && !string.IsNullOrEmpty(request.Teacher))
                {
                    var teacherObj = await _mongoDBService.Teachers.Find(t => t.Id == request.Teacher).FirstOrDefaultAsync();
                    targetName = teacherObj?.Name ?? "Teacher";
                }
                else if (request.Category == "course" && !string.IsNullOrEmpty(request.Course))
                {
                    var courseObj = await _mongoDBService.Courses.Find(c => c.Id == request.Course).FirstOrDefaultAsync();
                    targetName = courseObj != null ? $"{courseObj.Code} {courseObj.Title}" : "Course";
                }
                else if (request.Category == "event" && !string.IsNullOrEmpty(request.Event))
                {
                    var eventObj = await _mongoDBService.Events.Find(e => e.Id == request.Event).FirstOrDefaultAsync();
                    targetName = eventObj?.Name ?? "Event";
                }
                else
                {
                    targetName = _categoryLabels[request.Category];
                }
            }

            // Sentiment Analysis
            var analysisText = $"{request.Comment} {request.Suggestion ?? ""}";
            var sentimentDto = _sentimentService.AnalyzeSentiment(analysisText);

            var feedback = new Feedback
            {
                User = userId,
                Category = request.Category,
                TargetName = targetName,
                Teacher = request.Category == "teacher" ? request.Teacher : null,
                Course = request.Category == "course" ? request.Course : null,
                Event = request.Category == "event" ? request.Event : null,
                Ratings = ratingsList,
                AverageRating = averageRating,
                Comment = request.Comment.Trim(),
                Suggestion = (request.Suggestion ?? "").Trim(),
                Sentiment = new SentimentResult
                {
                    Score = sentimentDto.Score,
                    Comparative = sentimentDto.Comparative,
                    Label = sentimentDto.Label
                },
                Degree = degree,
                Year = year,
                Section = section,
                ClassKey = classKey,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _mongoDBService.Feedbacks.InsertOneAsync(feedback);

            // Log action
            var log = new AuditLog
            {
                Actor = userId,
                ActorModel = "User",
                ActorRole = "user",
                Action = "FEEDBACK_SUBMITTED",
                EntityType = "Feedback",
                EntityId = feedback.Id ?? "",
                Details = $"{_categoryLabels[request.Category]} submitted with {feedback.Sentiment.Label} sentiment.",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _mongoDBService.AuditLogs.InsertOneAsync(log);

            return Ok(new
            {
                msg = $"Feedback submitted successfully. Sentiment: {feedback.Sentiment.Label}",
                sentiment = feedback.Sentiment.Label,
                averageRating
            });
        }

        private string[] GetRatingLabelsForCategory(string category)
        {
            if (category == "teacher") return new[] { "Teaching Quality", "Communication", "Punctuality", "Course Coverage", "Behavior" };
            if (category == "course") return new[] { "Course Content", "Difficulty Management", "Practical Relevance", "Assessment Fairness", "Learning Value" };
            if (category == "event") return new[] { "Organization", "Management", "Usefulness", "Environment", "Overall Experience" };
            if (category == "transport") return new[] { "Timing", "Cleanliness", "Safety", "Availability", "Overall Service" };
            if (category == "cafeteria") return new[] { "Food Quality", "Cleanliness", "Price Fairness", "Menu Variety", "Overall Service" };
            if (category == "sports") return new[] { "Equipment", "Ground/Court Quality", "Availability", "Management", "Overall Facilities" };
            if (category == "library") return new[] { "Book Availability", "Study Environment", "Timing", "Staff Support", "Overall Service" };
            return new[] { "Management", "Communication", "Support", "Responsiveness", "Overall Experience" };
        }
    }

    public class FeedbackSubmitRequest
    {
        public string Category { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string? Suggestion { get; set; }
        public string? TargetName { get; set; }
        public string? Teacher { get; set; }
        public string? Course { get; set; }
        public string? Event { get; set; }
        public int Rating1 { get; set; }
        public int Rating2 { get; set; }
        public int Rating3 { get; set; }
        public int Rating4 { get; set; }
        public int RatingOverall { get; set; }
    }
}
