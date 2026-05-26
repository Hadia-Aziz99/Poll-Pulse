using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PollpulseBackend.Models;
using PollpulseBackend.Services;

namespace PollpulseBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;
        private readonly TokenService _tokenService;

        public AuthController(MongoDBService mongoDBService, TokenService tokenService)
        {
            _mongoDBService = mongoDBService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Degree) || 
                request.Year < 1 || request.Year > 6 || string.IsNullOrWhiteSpace(request.Section))
            {
                return BadRequest(new { msg = "Please provide all required fields." });
            }

            var email = request.Email.Trim().ToLower();
            var emailRegex = new Regex(@"^\d{7}@students\.au\.edu\.pk$", RegexOptions.IgnoreCase);
            if (!emailRegex.IsMatch(email))
            {
                return BadRequest(new { msg = "Student email must follow the pattern 2502077@students.au.edu.pk." });
            }

            var rollNoMatch = Regex.Match(email, @"^(\d{7})@");
            var rollNo = rollNoMatch.Success ? rollNoMatch.Groups[1].Value : "";

            var existingUser = await _mongoDBService.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return Conflict(new { msg = "This university email is already registered." });
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var classKey = $"{request.Degree}-Y{request.Year}-{request.Section}";

            var user = new User
            {
                Name = request.Name.Trim(),
                Email = email,
                RollNo = rollNo,
                Degree = request.Degree,
                Year = request.Year,
                Section = request.Section,
                ClassKey = classKey,
                PasswordHash = passwordHash,
                Role = "user",
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _mongoDBService.Users.InsertOneAsync(user);

            // Log action
            var log = new AuditLog
            {
                Actor = user.Id,
                ActorModel = "User",
                ActorRole = "user",
                Action = "REGISTER",
                EntityType = "User",
                EntityId = user.Id ?? "",
                Details = $"Student registered for {user.ClassKey}.",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _mongoDBService.AuditLogs.InsertOneAsync(log);

            var token = _tokenService.GenerateUserToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email,
                    rollNo = user.RollNo,
                    degree = user.Degree,
                    year = user.Year,
                    section = user.Section,
                    classKey = user.ClassKey,
                    role = user.Role
                }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { msg = "Email and Password are required." });
            }

            var email = request.Email.Trim().ToLower();
            var user = await _mongoDBService.Users.Find(u => u.Email == email).FirstOrDefaultAsync();

            if (user == null || user.Status != "active" || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { msg = "Invalid email or password." });
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _mongoDBService.Users.ReplaceOneAsync(u => u.Id == user.Id, user);

            var token = _tokenService.GenerateUserToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email,
                    rollNo = user.RollNo,
                    degree = user.Degree,
                    year = user.Year,
                    section = user.Section,
                    classKey = user.ClassKey,
                    role = user.Role
                }
            });
        }

        [HttpPost("admin/login")]
        public async Task<IActionResult> AdminLogin([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { msg = "Email and Password are required." });
            }

            var email = request.Email.Trim().ToLower();
            var admin = await _mongoDBService.Admins.Find(a => a.Email == email).FirstOrDefaultAsync();

            if (admin == null || admin.Status != "active" || !BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
            {
                return Unauthorized(new { msg = "Invalid email or password." });
            }

            admin.LastLoginAt = DateTime.UtcNow;
            await _mongoDBService.Admins.ReplaceOneAsync(a => a.Id == admin.Id, admin);

            var token = _tokenService.GenerateAdminToken(admin);

            return Ok(new
            {
                token,
                user = new
                {
                    id = admin.Id,
                    name = admin.Name,
                    email = admin.Email,
                    role = admin.Role
                }
            });
        }
    }

    public class RegisterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Section { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
