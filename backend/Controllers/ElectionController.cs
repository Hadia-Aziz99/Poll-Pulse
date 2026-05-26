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
    [Route("api/elections")]
    public class ElectionController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;
        private readonly SentimentService _sentimentService;

        public ElectionController(MongoDBService mongoDBService, SentimentService sentimentService)
        {
            _mongoDBService = mongoDBService;
            _sentimentService = sentimentService;
        }

        private string GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        private string GetCurrentClassKey() => User.FindFirst("classKey")?.Value ?? "";

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = GetCurrentUserId();
            var classKey = GetCurrentClassKey();

            if (string.IsNullOrEmpty(classKey))
            {
                return BadRequest(new { msg = "Class info not associated with user." });
            }

            var now = DateTime.UtcNow;

            // 1. Get active elections for student class
            var activeElections = await _mongoDBService.Elections
                .Find(e => e.ClassKey == classKey && e.Status == "active")
                .ToListAsync();

            // Filter open ones based on date (same as isOpen virtual)
            var openElections = activeElections.Where(e => e.IsOpen).ToList();

            // 2. Get recently closed elections for student class
            var closedElections = await _mongoDBService.Elections
                .Find(e => e.ClassKey == classKey && e.Status == "closed")
                .SortByDescending(e => e.UpdatedAt)
                .Limit(5)
                .ToListAsync();

            // 3. Count votes cast by this user
            var votesCast = await _mongoDBService.Votes.CountDocumentsAsync(v => v.User == userId);

            // 4. Count feedbacks submitted by this user
            var feedbacksSubmitted = await _mongoDBService.Feedbacks.CountDocumentsAsync(f => f.User == userId);

            // 5. Get recent feedback summary for this class
            var classFeedbacks = await _mongoDBService.Feedbacks
                .Find(f => f.ClassKey == classKey)
                .SortByDescending(f => f.CreatedAt)
                .Limit(5)
                .ToListAsync();

            var sentimentPercentages = _sentimentService.CalculatePercentages(
                classFeedbacks.Select(f => f.Sentiment).ToList()
            );

            var openElectionsList = new List<object>();
            foreach (var e in openElections)
            {
                var hasVoted = await _mongoDBService.Votes.Find(v => v.Election == e.Id && v.User == userId).AnyAsync();
                openElectionsList.Add(new
                {
                    e.Id,
                    e.Title,
                    e.ElectionType,
                    e.EndAt,
                    hasVoted
                });
            }

            return Ok(new
            {
                metrics = new
                {
                    openElectionsCount = openElections.Count,
                    closedElectionsCount = closedElections.Count,
                    votesCast,
                    feedbacksSubmitted,
                    classSentiment = sentimentPercentages
                },
                openElections = openElectionsList,
                closedElections = closedElections.Select(e => new { e.Id, e.Title, e.ElectionType, e.UpdatedAt }),
                recentFeedbacks = classFeedbacks.Select(f => new { f.Category, f.Comment, f.CreatedAt, sentiment = f.Sentiment.Label })
            });
        }

        [HttpGet]
        public async Task<IActionResult> ListElections()
        {
            var classKey = GetCurrentClassKey();
            if (string.IsNullOrEmpty(classKey))
            {
                return BadRequest(new { msg = "Class info not available." });
            }

            var elections = await _mongoDBService.Elections
                .Find(e => e.ClassKey == classKey && (e.Status == "active" || e.Status == "closed"))
                .SortByDescending(e => e.CreatedAt)
                .ToListAsync();

            var list = new List<object>();
            var userId = GetCurrentUserId();

            foreach (var e in elections)
            {
                // Check if user already voted in this election
                var hasVoted = await _mongoDBService.Votes.Find(v => v.Election == e.Id && v.User == userId).AnyAsync();
                
                list.Add(new
                {
                    e.Id,
                    e.Title,
                    e.Description,
                    e.ElectionType,
                    e.Status,
                    e.StartAt,
                    e.EndAt,
                    e.ClassKey,
                    isOpen = e.IsOpen,
                    hasVoted
                });
            }

            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetElectionDetails(string id)
        {
            var election = await _mongoDBService.Elections.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (election == null)
            {
                return NotFound(new { msg = "Election not found." });
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            var classKey = GetCurrentClassKey();
            if (userRole != "admin" && election.ClassKey != classKey)
            {
                return Forbid(); // Student cannot view other classes' elections
            }

            var candidates = await _mongoDBService.Candidates
                .Find(c => c.Election == id && c.IsActive)
                .SortBy(c => c.Order)
                .ToListAsync();

            var userId = GetCurrentUserId();
            var hasVoted = await _mongoDBService.Votes.Find(v => v.Election == id && v.User == userId).AnyAsync();

            return Ok(new
            {
                election = new
                {
                    election.Id,
                    election.Title,
                    election.Description,
                    election.ElectionType,
                    election.Status,
                    election.EndAt,
                    isOpen = election.IsOpen,
                    hasVoted
                },
                candidates = candidates.Select(c => new { c.Id, c.Name, c.RollNo, c.Manifesto })
            });
        }

        [HttpPost("{id}/vote")]
        public async Task<IActionResult> CastVote(string id, [FromBody] VoteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CandidateId))
            {
                return BadRequest(new { msg = "Please select a candidate." });
            }

            var election = await _mongoDBService.Elections.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (election == null)
            {
                return NotFound(new { msg = "Election not found." });
            }

            if (!election.IsOpen)
            {
                return BadRequest(new { msg = "Voting is currently closed for this election." });
            }

            var userId = GetCurrentUserId();
            var classKey = GetCurrentClassKey();

            if (election.ClassKey != classKey)
            {
                return BadRequest(new { msg = "You are not authorized to vote in this class section." });
            }

            // Enforce single-voting check
            var hasVoted = await _mongoDBService.Votes.Find(v => v.Election == id && v.User == userId).AnyAsync();
            if (hasVoted)
            {
                return BadRequest(new { msg = "You have already voted in this election." });
            }

            var candidate = await _mongoDBService.Candidates
                .Find(c => c.Id == request.CandidateId && c.Election == id && c.IsActive)
                .FirstOrDefaultAsync();

            if (candidate == null)
            {
                return BadRequest(new { msg = "Invalid candidate selected." });
            }

            var vote = new Vote
            {
                Election = id,
                Candidate = request.CandidateId,
                User = userId,
                ClassKey = classKey,
                ElectionType = election.ElectionType,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                UserAgent = Request.Headers["User-Agent"].ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                await _mongoDBService.Votes.InsertOneAsync(vote);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                return BadRequest(new { msg = "You have already cast a vote in this election (Duplicate Check Failure)." });
            }

            return Ok(new { msg = "Vote submitted successfully!" });
        }

        [HttpGet("{id}/results")]
        public async Task<IActionResult> GetElectionResults(string id)
        {
            var election = await _mongoDBService.Elections.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (election == null)
            {
                return NotFound(new { msg = "Election not found." });
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            var isAdmin = userRole == "admin";
            var classKey = GetCurrentClassKey();
            if (!isAdmin && election.ClassKey != classKey)
            {
                return Forbid();
            }

            if (!isAdmin && election.Status != "closed")
            {
                return BadRequest(new { msg = "Results are only visible after the admin closes the election." });
            }

            var candidates = await _mongoDBService.Candidates.Find(c => c.Election == id).ToListAsync();
            var votes = await _mongoDBService.Votes.Find(v => v.Election == id).ToListAsync();

            var totalVotes = votes.Count;

            var candidateResults = candidates.Select(c =>
            {
                var voteCount = votes.Count(v => v.Candidate == c.Id);
                double percentage = totalVotes > 0 ? Math.Round((double)voteCount / totalVotes * 100) : 0;

                return new
                {
                    c.Id,
                    c.Name,
                    c.RollNo,
                    votes = voteCount,
                    percentage
                };
            })
            .OrderByDescending(r => r.votes)
            .ToList();

            // Determine Winner
            var winner = candidateResults.FirstOrDefault(r => r.votes > 0);

            // Related class feedback sentiment
            var classFeedbacks = await _mongoDBService.Feedbacks
                .Find(f => f.ClassKey == election.ClassKey)
                .ToListAsync();

            var sentimentSummary = _sentimentService.CalculatePercentages(
                classFeedbacks.Select(f => f.Sentiment).ToList()
            );

            return Ok(new
            {
                election = new { election.Id, election.Title, election.ElectionType, election.Status, election.UpdatedAt },
                totalVotes,
                winner = winner != null ? new { winner.Id, winner.Name, winner.RollNo } : null,
                results = candidateResults,
                sentiment = sentimentSummary
            });
        }
    }

    public class VoteRequest
    {
        public string CandidateId { get; set; } = string.Empty;
    }
}
