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
    [Authorize(Roles = "admin")]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;
        private readonly SentimentService _sentimentService;

        public AdminController(MongoDBService mongoDBService, SentimentService sentimentService)
        {
            _mongoDBService = mongoDBService;
            _sentimentService = sentimentService;
        }

        private string GetCurrentAdminId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        private async Task LogAdminAction(string action, string entityType, string entityId, string details)
        {
            var log = new AuditLog
            {
                Actor = GetCurrentAdminId(),
                ActorModel = "Admin",
                ActorRole = "admin",
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _mongoDBService.AuditLogs.InsertOneAsync(log);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var activeElections = await _mongoDBService.Elections.CountDocumentsAsync(e => e.Status == "active");
            var closedElections = await _mongoDBService.Elections.CountDocumentsAsync(e => e.Status == "closed");
            var totalVotes = await _mongoDBService.Votes.CountDocumentsAsync(_ => true);
            var totalStudents = await _mongoDBService.Users.CountDocumentsAsync(u => u.Role == "user");
            var totalFeedback = await _mongoDBService.Feedbacks.CountDocumentsAsync(_ => true);

            var recentElections = await _mongoDBService.Elections
                .Find(_ => true)
                .SortByDescending(e => e.CreatedAt)
                .Limit(6)
                .ToListAsync();

            var recentLogs = await _mongoDBService.AuditLogs
                .Find(_ => true)
                .SortByDescending(l => l.CreatedAt)
                .Limit(6)
                .ToListAsync();

            // Populate Actor name for recent logs
            var logsList = new List<object>();
            foreach (var log in recentLogs)
            {
                string actorName = "System";
                if (log.ActorModel == "Admin")
                {
                    var admin = await _mongoDBService.Admins.Find(a => a.Id == log.Actor).FirstOrDefaultAsync();
                    if (admin != null) actorName = admin.Name;
                }
                else if (log.ActorModel == "User")
                {
                    var user = await _mongoDBService.Users.Find(u => u.Id == log.Actor).FirstOrDefaultAsync();
                    if (user != null) actorName = user.Name;
                }

                logsList.Add(new
                {
                    log.Id,
                    log.Action,
                    log.EntityType,
                    log.Details,
                    log.CreatedAt,
                    actor = new { name = actorName, role = log.ActorRole }
                });
            }

            var allFeedbacks = await _mongoDBService.Feedbacks.Find(_ => true).ToListAsync();
            var sentimentSummary = _sentimentService.CalculatePercentages(
                allFeedbacks.Select(f => f.Sentiment).ToList()
            );

            return Ok(new
            {
                metrics = new
                {
                    activeElections,
                    closedElections,
                    totalVotes,
                    totalStudents,
                    totalFeedback,
                    sentiment = sentimentSummary
                },
                recentElections = recentElections.Select(e => new { e.Id, e.Title, e.ElectionType, e.ClassKey, e.Status }),
                recentLogs = logsList
            });
        }

        [HttpGet("classes")]
        public async Task<IActionResult> GetClasses()
        {
            var classes = await _mongoDBService.ClassSections.Find(_ => true)
                .SortBy(c => c.Degree).ThenBy(c => c.Year).ThenBy(c => c.Section)
                .ToListAsync();

            return Ok(classes);
        }

        [HttpGet("elections")]
        public async Task<IActionResult> GetElections()
        {
            var elections = await _mongoDBService.Elections.Find(_ => true).SortByDescending(e => e.CreatedAt).ToListAsync();
            
            var list = new List<object>();
            foreach (var e in elections)
            {
                var voteCount = await _mongoDBService.Votes.CountDocumentsAsync(v => v.Election == e.Id);
                var candidateCount = await _mongoDBService.Candidates.CountDocumentsAsync(c => c.Election == e.Id && c.IsActive);
                
                list.Add(new
                {
                    e.Id,
                    e.Title,
                    e.ElectionType,
                    e.ClassKey,
                    e.Status,
                    e.StartAt,
                    e.EndAt,
                    votesCount = voteCount,
                    candidatesCount = candidateCount
                });
            }

            return Ok(list);
        }

        [HttpPost("elections")]
        public async Task<IActionResult> CreateElection([FromBody] CreateElectionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < 5 || request.Title.Length > 180)
            {
                return BadRequest(new { msg = "Election title must be between 5 and 180 characters." });
            }

            if (request.CandidateNames == null || request.CandidateNames.Count < 2)
            {
                return BadRequest(new { msg = "Provide at least two candidate names." });
            }

            var classKey = $"{request.Degree}-Y{request.Year}-{request.Section}";
            var adminId = GetCurrentAdminId();

            var election = new Election
            {
                Title = request.Title.Trim(),
                Description = (request.Description ?? "").Trim(),
                ElectionType = request.ElectionType,
                Degree = request.Degree,
                Year = request.Year,
                Section = request.Section,
                ClassKey = classKey,
                Status = request.Status ?? "draft",
                StartAt = request.StartAt,
                EndAt = request.EndAt,
                CreatedBy = adminId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _mongoDBService.Elections.InsertOneAsync(election);

            // Add Candidates
            for (int i = 0; i < request.CandidateNames.Count; i++)
            {
                var candidate = new Candidate
                {
                    Election = election.Id ?? "",
                    Name = request.CandidateNames[i].Trim(),
                    RollNo = (request.CandidateRollNos != null && request.CandidateRollNos.Count > i) 
                        ? request.CandidateRollNos[i].Trim() 
                        : "",
                    Order = i + 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _mongoDBService.Candidates.InsertOneAsync(candidate);
            }

            await LogAdminAction("ELECTION_CREATED", "Election", election.Id ?? "", $"Created {election.ElectionType} election for {election.ClassKey}.");

            return Ok(new { msg = "CR/GR election created successfully.", id = election.Id });
        }

        [HttpPost("elections/{id}/status/{status}")]
        public async Task<IActionResult> ChangeElectionStatus(string id, string status)
        {
            if (status != "draft" && status != "active" && status != "closed")
            {
                return BadRequest(new { msg = "Invalid status requested." });
            }

            var election = await _mongoDBService.Elections.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (election == null)
            {
                return NotFound(new { msg = "Election not found." });
            }

            election.Status = status;
            election.UpdatedAt = DateTime.UtcNow;

            await _mongoDBService.Elections.ReplaceOneAsync(e => e.Id == id, election);

            await LogAdminAction("ELECTION_STATUS_CHANGED", "Election", id, $"Status changed to {status}.");

            return Ok(new { msg = $"Election marked as {status}.", id = election.Id });
        }

        [HttpDelete("elections/{id}")]
        [HttpPost("elections/{id}/delete")] // support both POST and DELETE
        public async Task<IActionResult> DeleteElection(string id)
        {
            var election = await _mongoDBService.Elections.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (election == null)
            {
                return NotFound(new { msg = "Election not found." });
            }

            await _mongoDBService.Candidates.DeleteManyAsync(c => c.Election == id);
            await _mongoDBService.Votes.DeleteManyAsync(v => v.Election == id);
            await _mongoDBService.Elections.DeleteOneAsync(e => e.Id == id);

            await LogAdminAction("ELECTION_DELETED", "Election", id, "Deleted election with candidates and votes.");

            return Ok(new { msg = "Election deleted successfully." });
        }

        [HttpGet("masters")]
        public async Task<IActionResult> GetMasters()
        {
            var teachers = await _mongoDBService.Teachers.Find(_ => true).SortBy(t => t.Name).ToListAsync();
            var courses = await _mongoDBService.Courses.Find(_ => true).SortBy(c => c.Code).ToListAsync();
            var events = await _mongoDBService.Events.Find(_ => true).SortByDescending(e => e.EventDate).ToListAsync();

            return Ok(new
            {
                teachers,
                courses,
                events
            });
        }

        [HttpPost("masters/teachers")]
        public async Task<IActionResult> CreateTeacher([FromBody] TeacherRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { msg = "Teacher name is required." });
            }

            var teacher = new Teacher
            {
                Name = request.Name.Trim(),
                Email = (request.Email ?? "").Trim().ToLower(),
                Department = "Computer Science",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _mongoDBService.Teachers.InsertOneAsync(teacher);
            await LogAdminAction("TEACHER_CREATED", "Teacher", teacher.Id ?? "", $"Added teacher: {teacher.Name}");

            return Ok(new { msg = "Teacher added successfully.", teacher });
        }

        [HttpPost("masters/courses")]
        public async Task<IActionResult> CreateCourse([FromBody] CourseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { msg = "Course code and title are required." });
            }

            var course = new Course
            {
                Code = request.Code.Trim().ToUpper(),
                Title = request.Title.Trim(),
                Degree = request.Degree ?? "Both",
                Year = request.Year,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _mongoDBService.Courses.InsertOneAsync(course);
            await LogAdminAction("COURSE_CREATED", "Course", course.Id ?? "", $"Added course: {course.Code} {course.Title}");

            return Ok(new { msg = "Course added successfully.", course });
        }

        [HttpPost("masters/events")]
        public async Task<IActionResult> CreateEvent([FromBody] EventRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { msg = "Event name is required." });
            }

            var evObj = new Event
            {
                Name = request.Name.Trim(),
                EventDate = request.EventDate,
                Description = (request.Description ?? "").Trim(),
                Status = request.Status ?? "completed",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _mongoDBService.Events.InsertOneAsync(evObj);
            await LogAdminAction("EVENT_CREATED", "Event", evObj.Id ?? "", $"Added event: {evObj.Name}");

            return Ok(new { msg = "Event added successfully.", eventObj = evObj });
        }

        [HttpGet("feedback")]
        public async Task<IActionResult> GetFeedbackList([FromQuery] string? category, [FromQuery] string? sentiment)
        {
            var builder = Builders<Feedback>.Filter;
            var filter = builder.Empty;

            if (!string.IsNullOrEmpty(category))
            {
                filter = filter & builder.Eq(f => f.Category, category);
            }
            if (!string.IsNullOrEmpty(sentiment))
            {
                filter = filter & builder.Eq(f => f.Sentiment.Label, sentiment);
            }

            var feedbacks = await _mongoDBService.Feedbacks
                .Find(filter)
                .SortByDescending(f => f.CreatedAt)
                .Limit(200)
                .ToListAsync();

            var list = new List<object>();
            foreach (var f in feedbacks)
            {
                var userObj = await _mongoDBService.Users.Find(u => u.Id == f.User).FirstOrDefaultAsync();
                list.Add(new
                {
                    f.Id,
                    f.Category,
                    f.TargetName,
                    f.AverageRating,
                    f.Comment,
                    f.Suggestion,
                    sentiment = f.Sentiment,
                    f.ClassKey,
                    f.CreatedAt,
                    user = userObj != null ? new { name = userObj.Name, email = userObj.Email } : null
                });
            }

            return Ok(list);
        }

        [HttpGet("sentiment")]
        public async Task<IActionResult> GetSentimentMetrics()
        {
            var feedbacks = await _mongoDBService.Feedbacks.Find(_ => true).ToListAsync();
            var overallSummary = _sentimentService.CalculatePercentages(feedbacks.Select(f => f.Sentiment).ToList());

            // Simple group-by in C# memory (extremely clean and robust!)
            var byCategory = feedbacks.GroupBy(f => f.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count(),
                    AvgRating = Math.Round(g.Average(f => f.AverageRating ?? 0), 2),
                    Sentiment = _sentimentService.CalculatePercentages(g.Select(f => f.Sentiment).ToList())
                })
                .OrderBy(c => c.Category)
                .ToList();

            var byClass = feedbacks.GroupBy(f => f.ClassKey)
                .Select(g => new
                {
                    ClassKey = g.Key,
                    Count = g.Count(),
                    Sentiment = _sentimentService.CalculatePercentages(g.Select(f => f.Sentiment).ToList())
                })
                .OrderBy(c => c.ClassKey)
                .ToList();

            return Ok(new
            {
                summary = overallSummary,
                byCategory,
                byClass
            });
        }

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs()
        {
            var logs = await _mongoDBService.AuditLogs
                .Find(_ => true)
                .SortByDescending(l => l.CreatedAt)
                .Limit(100)
                .ToListAsync();

            var list = new List<object>();
            foreach (var log in logs)
            {
                string actorName = "System";
                if (log.ActorModel == "Admin")
                {
                    var admin = await _mongoDBService.Admins.Find(a => a.Id == log.Actor).FirstOrDefaultAsync();
                    if (admin != null) actorName = admin.Name;
                }
                else if (log.ActorModel == "User")
                {
                    var user = await _mongoDBService.Users.Find(u => u.Id == log.Actor).FirstOrDefaultAsync();
                    if (user != null) actorName = user.Name;
                }

                list.Add(new
                {
                    log.Id,
                    log.Action,
                    log.EntityType,
                    log.EntityId,
                    log.Details,
                    log.IpAddress,
                    log.CreatedAt,
                    actor = new { name = actorName, role = log.ActorRole }
                });
            }

            return Ok(list);
        }
    }

    public class CreateElectionRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ElectionType { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Section { get; set; } = string.Empty;
        public string? Status { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public List<string> CandidateNames { get; set; } = new List<string>();
        public List<string>? CandidateRollNos { get; set; }
    }

    public class TeacherRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    public class CourseRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Degree { get; set; }
        public int? Year { get; set; }
    }

    public class EventRequest
    {
        public string Name { get; set; } = string.Empty;
        public DateTime? EventDate { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
    }
}
