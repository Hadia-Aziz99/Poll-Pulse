using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using PollpulseBackend.Models;

namespace PollpulseBackend.Services
{
    public class MongoDBService
    {
        private readonly IMongoDatabase _database;

        public MongoDBService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoDB") 
                ?? "mongodb://127.0.0.1:27017/au_cs_voice_portal";
            
            var mongoUrl = new MongoUrl(connectionString);
            var client = new MongoClient(mongoUrl);
            
            // Extract database name from connection string or fallback to default
            var databaseName = mongoUrl.DatabaseName ?? "au_cs_voice_portal";
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        public IMongoCollection<Admin> Admins => _database.GetCollection<Admin>("admins");
        public IMongoCollection<ClassSection> ClassSections => _database.GetCollection<ClassSection>("classsections");
        public IMongoCollection<Teacher> Teachers => _database.GetCollection<Teacher>("teachers");
        public IMongoCollection<Course> Courses => _database.GetCollection<Course>("courses");
        public IMongoCollection<Event> Events => _database.GetCollection<Event>("events");
        public IMongoCollection<Election> Elections => _database.GetCollection<Election>("elections");
        public IMongoCollection<Candidate> Candidates => _database.GetCollection<Candidate>("candidates");
        public IMongoCollection<Vote> Votes => _database.GetCollection<Vote>("votes");
        public IMongoCollection<Feedback> Feedbacks => _database.GetCollection<Feedback>("feedbacks");
public IMongoCollection<ContactMessage> ContactMessages => _database.GetCollection<ContactMessage>("contactmessages");
public IMongoCollection<AuditLog> AuditLogs => _database.GetCollection<AuditLog>("auditlogs");



        private async Task EnsureIndexesAsync()
        {
            // Helper to safely create an index, ignoring conflicts if the index already exists
            // with a different name (error codes 85 = IndexOptionsConflict, 86 = IndexKeySpecsConflict)
            static async Task SafeCreateIndex<T>(IMongoCollection<T> collection, CreateIndexModel<T> model)
            {
                try
                {
                    await collection.Indexes.CreateOneAsync(model);
                }
                catch (MongoCommandException ex) when (ex.Code == 85 || ex.Code == 86)
                {
                    // Index already exists with a different name — safe to ignore
                }
            }

            await SafeCreateIndex(Users, new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Email),
                new CreateIndexOptions { Unique = true, Name = "ux_users_email" }
            ));

            await SafeCreateIndex(Admins, new CreateIndexModel<Admin>(
                Builders<Admin>.IndexKeys.Ascending(a => a.Email),
                new CreateIndexOptions { Unique = true, Name = "ux_admins_email" }
            ));

            await SafeCreateIndex(Votes, new CreateIndexModel<Vote>(
                Builders<Vote>.IndexKeys.Ascending(v => v.Election).Ascending(v => v.User),
                new CreateIndexOptions { Unique = true, Name = "ux_votes_election_user" }
            ));

            await SafeCreateIndex(Elections, new CreateIndexModel<Election>(
                Builders<Election>.IndexKeys.Ascending(e => e.ClassKey).Ascending(e => e.Status),
                new CreateIndexOptions { Name = "ix_elections_class_status" }
            ));

            await SafeCreateIndex(Feedbacks, new CreateIndexModel<Feedback>(
                Builders<Feedback>.IndexKeys.Ascending(f => f.User).Ascending(f => f.Category).Descending(f => f.CreatedAt),
                new CreateIndexOptions { Name = "ix_feedback_user_category_created" }
            ));
            await SafeCreateIndex(ContactMessages, new CreateIndexModel<ContactMessage>(
            Builders<ContactMessage>.IndexKeys.Ascending(c => c.Email).Descending(c => c.CreatedAt),
            new CreateIndexOptions { Name = "ix_contact_email_created" }
            ));
        }

        public async Task SeedDatabaseAsync(SentimentService sentimentService)
        {
            await EnsureIndexesAsync();

            // Seed only if no Admins exist in the collection
            if (await Admins.CountDocumentsAsync(_ => true) > 0)
            {
                return;
            }

            // 1. Create Admins
            var adminId1 = ObjectId.GenerateNewId().ToString();
            var adminId2 = ObjectId.GenerateNewId().ToString();
            
            var adminsList = new List<Admin>
            {
                new Admin
                {
                    Id = adminId1,
                    Name = "AU CS Admin",
                    Email = "admin@aucs.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@12345"),
                    Role = "admin",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Admin
                {
                    Id = adminId2,
                    Name = "Department Coordinator",
                    Email = "coordinator@aucs.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@12345"),
                    Role = "admin",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            await Admins.InsertManyAsync(adminsList);

            // 2. Create Student Users
            var studentId1 = ObjectId.GenerateNewId().ToString();
            var studentId2 = ObjectId.GenerateNewId().ToString();
            var studentId3 = ObjectId.GenerateNewId().ToString();
            var studentId4 = ObjectId.GenerateNewId().ToString();

            var studentsList = new List<User>
            {
                new User
                {
                    Id = studentId1,
                    Name = "Demo BSCS Student",
                    Email = "2502077@students.au.edu.pk",
                    RollNo = "2502077",
                    Degree = "BSCS",
                    Year = 2,
                    Section = "A",
                    ClassKey = "BSCS-Y2-A",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                    Role = "user",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = studentId2,
                    Name = "Demo BSIT Student",
                    Email = "2502078@students.au.edu.pk",
                    RollNo = "2502078",
                    Degree = "BSIT",
                    Year = 1,
                    Section = "B",
                    ClassKey = "BSIT-Y1-B",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                    Role = "user",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = studentId3,
                    Name = "Sara Ahmed",
                    Email = "2502079@students.au.edu.pk",
                    RollNo = "2502079",
                    Degree = "BSCS",
                    Year = 2,
                    Section = "A",
                    ClassKey = "BSCS-Y2-A",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                    Role = "user",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = studentId4,
                    Name = "Usman Raza",
                    Email = "2502080@students.au.edu.pk",
                    RollNo = "2502080",
                    Degree = "BSIT",
                    Year = 2,
                    Section = "C",
                    ClassKey = "BSIT-Y2-C",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                    Role = "user",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            await Users.InsertManyAsync(studentsList);

            // 3. Create Class Sections
            var classDocs = new List<ClassSection>();
            foreach (var degree in new[] { "BSCS", "BSIT" })
            {
                for (int year = 1; year <= 6; year++)
                {
                    foreach (var section in new[] { "A", "B", "C" })
                    {
                        classDocs.Add(new ClassSection
                        {
                            Degree = degree,
                            Year = year,
                            Section = section,
                            ClassKey = $"{degree}-Y{year}-{section}",
                            Title = $"{degree} Year {year} Section {section}",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
            await ClassSections.InsertManyAsync(classDocs);

            // 4. Create Teachers
            var teacherId1 = ObjectId.GenerateNewId().ToString();
            var teacherId2 = ObjectId.GenerateNewId().ToString();
            var teacherId3 = ObjectId.GenerateNewId().ToString();
            var teacherId4 = ObjectId.GenerateNewId().ToString();

            var teachersList = new List<Teacher>
            {
                new Teacher { Id = teacherId1, Name = "Dr. Ayesha Khan", Email = "ayesha.khan@au.edu.pk", Department = "Computer Science", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Teacher { Id = teacherId2, Name = "Sir Ahmed Raza", Email = "ahmed.raza@au.edu.pk", Department = "Computer Science", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Teacher { Id = teacherId3, Name = "Ma’am Fatima Noor", Email = "fatima.noor@au.edu.pk", Department = "Computer Science", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Teacher { Id = teacherId4, Name = "Dr. Salman Iqbal", Email = "salman.iqbal@au.edu.pk", Department = "Computer Science", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            await Teachers.InsertManyAsync(teachersList);

            // 5. Create Courses
            var courseId1 = ObjectId.GenerateNewId().ToString();
            var courseId2 = ObjectId.GenerateNewId().ToString();
            var courseId3 = ObjectId.GenerateNewId().ToString();
            var courseId4 = ObjectId.GenerateNewId().ToString();
            var courseId5 = ObjectId.GenerateNewId().ToString();

            var coursesList = new List<Course>
            {
                new Course { Id = courseId1, Code = "CS101", Title = "Programming Fundamentals", Degree = "Both", Year = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Course { Id = courseId2, Code = "CS205", Title = "Database Systems", Degree = "Both", Year = 2, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Course { Id = courseId3, Code = "IT210", Title = "Web Technologies", Degree = "BSIT", Year = 2, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Course { Id = courseId4, Code = "CS310", Title = "Artificial Intelligence", Degree = "BSCS", Year = 3, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Course { Id = courseId5, Code = "CS220", Title = "Data Structures", Degree = "BSCS", Year = 2, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            await Courses.InsertManyAsync(coursesList);

            // 6. Create Events
            var eventId1 = ObjectId.GenerateNewId().ToString();
            var eventId2 = ObjectId.GenerateNewId().ToString();
            var eventId3 = ObjectId.GenerateNewId().ToString();
            var eventId4 = ObjectId.GenerateNewId().ToString();

            var eventsList = new List<Event>
            {
                new Event { Id = eventId1, Name = "CS Tech Seminar", EventDate = DateTime.UtcNow.AddDays(-5), Status = "completed", Description = "Department technology seminar.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Event { Id = eventId2, Name = "Sports Gala", EventDate = DateTime.UtcNow.AddDays(-10), Status = "completed", Description = "Annual sports event.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Event { Id = eventId3, Name = "Final Year Project Expo", EventDate = DateTime.UtcNow.AddDays(15), Status = "upcoming", Description = "Project exhibition.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Event { Id = eventId4, Name = "Web Technology Workshop", EventDate = DateTime.UtcNow.AddDays(-2), Status = "completed", Description = "Practical website development workshop.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            await Events.InsertManyAsync(eventsList);

            // 7. Create Elections
            var crElectionId = ObjectId.GenerateNewId().ToString();
            var grElectionId = ObjectId.GenerateNewId().ToString();
            var draftElectionId = ObjectId.GenerateNewId().ToString();

            var electionsList = new List<Election>
            {
                new Election
                {
                    Id = crElectionId,
                    Title = "BSCS Year 2 Section A CR Election",
                    Description = "Select the Class Representative for BSCS Year 2 Section A.",
                    ElectionType = "CR",
                    Degree = "BSCS",
                    Year = 2,
                    Section = "A",
                    ClassKey = "BSCS-Y2-A",
                    Status = "active",
                    StartAt = DateTime.UtcNow.AddDays(-1),
                    EndAt = DateTime.UtcNow.AddDays(2),
                    CreatedBy = adminId1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Election
                {
                    Id = grElectionId,
                    Title = "BSCS Year 2 Section A GR Election",
                    Description = "Select the GR for BSCS Year 2 Section A.",
                    ElectionType = "GR",
                    Degree = "BSCS",
                    Year = 2,
                    Section = "A",
                    ClassKey = "BSCS-Y2-A",
                    Status = "closed",
                    StartAt = DateTime.UtcNow.AddDays(-3),
                    EndAt = DateTime.UtcNow.AddDays(-1),
                    CreatedBy = adminId1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Election
                {
                    Id = draftElectionId,
                    Title = "BSIT Year 2 Section C CR Election",
                    Description = "Upcoming CR election for BSIT Year 2 Section C.",
                    ElectionType = "CR",
                    Degree = "BSIT",
                    Year = 2,
                    Section = "C",
                    ClassKey = "BSIT-Y2-C",
                    Status = "draft",
                    CreatedBy = adminId1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            await Elections.InsertManyAsync(electionsList);

            // 8. Create Candidates
            var candId1 = ObjectId.GenerateNewId().ToString();
            var candId2 = ObjectId.GenerateNewId().ToString();
            var candId3 = ObjectId.GenerateNewId().ToString();
            var candId4 = ObjectId.GenerateNewId().ToString();
            var candId5 = ObjectId.GenerateNewId().ToString();
            var candId6 = ObjectId.GenerateNewId().ToString();

            var candidatesList = new List<Candidate>
            {
                new Candidate { Id = candId1, Election = crElectionId, Name = "Ali Khan", RollNo = "2502011", Order = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Candidate { Id = candId2, Election = crElectionId, Name = "Hassan Malik", RollNo = "2502012", Order = 2, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Candidate { Id = candId3, Election = grElectionId, Name = "Ayesha Noor", RollNo = "2502021", Order = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Candidate { Id = candId4, Election = grElectionId, Name = "Hira Ali", RollNo = "2502022", Order = 2, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Candidate { Id = candId5, Election = draftElectionId, Name = "Zain Ali", RollNo = "2502088", Order = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Candidate { Id = candId6, Election = draftElectionId, Name = "Hania Malik", RollNo = "2502089", Order = 2, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            await Candidates.InsertManyAsync(candidatesList);

            // 9. Create Votes
            var votesList = new List<Vote>
            {
                new Vote
                {
                    Election = grElectionId,
                    Candidate = candId3,
                    User = studentId1,
                    ClassKey = "BSCS-Y2-A",
                    ElectionType = "GR",
                    IpAddress = "127.0.0.1",
                    UserAgent = "SeedAgent",
                    CreatedAt = DateTime.UtcNow.AddHours(-12),
                    UpdatedAt = DateTime.UtcNow.AddHours(-12)
                },
                new Vote
                {
                    Election = grElectionId,
                    Candidate = candId4,
                    User = studentId3,
                    ClassKey = "BSCS-Y2-A",
                    ElectionType = "GR",
                    IpAddress = "127.0.0.1",
                    UserAgent = "SeedAgent",
                    CreatedAt = DateTime.UtcNow.AddHours(-10),
                    UpdatedAt = DateTime.UtcNow.AddHours(-10)
                }
            };
            await Votes.InsertManyAsync(votesList);

            // Helper to generate Feedback document
            Feedback CreateFeedbackDoc(User student, string category, string targetName, string? teacherId, string? courseId, string? eventId, List<Rating> ratings, string comment, string suggestion)
            {
                var avg = Math.Round(ratings.Average(r => r.Value), 2);
                var sentimentDto = sentimentService.AnalyzeSentiment($"{comment} {suggestion}");

                return new Feedback
                {
                    User = student.Id ?? "",
                    Category = category,
                    TargetName = targetName,
                    Teacher = teacherId,
                    Course = courseId,
                    Event = eventId,
                    Ratings = ratings,
                    AverageRating = avg,
                    Comment = comment,
                    Suggestion = suggestion,
                    Sentiment = new SentimentResult
                    {
                        Score = sentimentDto.Score,
                        Comparative = sentimentDto.Comparative,
                        Label = sentimentDto.Label
                    },
                    Degree = student.Degree,
                    Year = student.Year,
                    Section = student.Section,
                    ClassKey = student.ClassKey,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                };
            }

            // 10. Create Feedbacks
            var feedbacksList = new List<Feedback>
            {
                CreateFeedbackDoc(
                    studentsList[0], "teacher", "Dr. Ayesha Khan", teacherId1, null, null,
                    new List<Rating> {
                        new Rating { Label = "Teaching Quality", Value = 5 },
                        new Rating { Label = "Communication", Value = 5 },
                        new Rating { Label = "Punctuality", Value = 4 },
                        new Rating { Label = "Course Coverage", Value = 5 },
                        new Rating { Label = "Behavior", Value = 5 }
                    },
                    "The teacher explains concepts clearly and is very helpful.",
                    "More lab examples would be useful."
                ),
                CreateFeedbackDoc(
                    studentsList[1], "course", "IT210 Web Technologies", null, courseId3, null,
                    new List<Rating> {
                        new Rating { Label = "Course Content", Value = 4 },
                        new Rating { Label = "Difficulty Management", Value = 3 },
                        new Rating { Label = "Practical Relevance", Value = 4 },
                        new Rating { Label = "Assessment Fairness", Value = 4 },
                        new Rating { Label = "Learning Value", Value = 4 }
                    },
                    "The course is useful but assignments are difficult.",
                    "Give more time for final project."
                ),
                CreateFeedbackDoc(
                    studentsList[0], "cafeteria", "Main Cafeteria Food Feedback", null, null, null,
                    new List<Rating> {
                        new Rating { Label = "Food Quality", Value = 2 },
                        new Rating { Label = "Cleanliness", Value = 2 },
                        new Rating { Label = "Price Fairness", Value = 3 },
                        new Rating { Label = "Menu Variety", Value = 2 },
                        new Rating { Label = "Overall Service", Value = 2 }
                    },
                    "Food quality is poor and the area is crowded during break.",
                    "Improve cleanliness and add more counters."
                ),
                CreateFeedbackDoc(
                    studentsList[2], "faculty", "Faculty Feedback", null, null, null,
                    new List<Rating> {
                        new Rating { Label = "Management", Value = 4 },
                        new Rating { Label = "Communication", Value = 4 },
                        new Rating { Label = "Support", Value = 3 },
                        new Rating { Label = "Responsiveness", Value = 4 },
                        new Rating { Label = "Overall Experience", Value = 4 }
                    },
                    "The department staff guides students well during registration.",
                    "Response time can be improved during peak days."
                ),
                CreateFeedbackDoc(
                    studentsList[3], "transport", "Transport Feedback", null, null, null,
                    new List<Rating> {
                        new Rating { Label = "Timing", Value = 4 },
                        new Rating { Label = "Cleanliness", Value = 3 },
                        new Rating { Label = "Safety", Value = 4 },
                        new Rating { Label = "Availability", Value = 4 },
                        new Rating { Label = "Overall Service", Value = 4 }
                    },
                    "Transport service is reliable most of the time.",
                    "Morning schedule should be communicated more clearly."
                ),
                CreateFeedbackDoc(
                    studentsList[1], "library", "Library Feedback", null, null, null,
                    new List<Rating> {
                        new Rating { Label = "Book Availability", Value = 4 },
                        new Rating { Label = "Study Environment", Value = 5 },
                        new Rating { Label = "Timing", Value = 5 },
                        new Rating { Label = "Staff Support", Value = 4 },
                        new Rating { Label = "Overall Service", Value = 4 }
                    },
                    "The library is quiet and suitable for study.",
                    "Add more recent programming books."
                ),
                CreateFeedbackDoc(
                    studentsList[2], "sports", "Sports Facilities Feedback", null, null, null,
                    new List<Rating> {
                        new Rating { Label = "Equipment", Value = 4 },
                        new Rating { Label = "Ground/Court Quality", Value = 3 },
                        new Rating { Label = "Availability", Value = 4 },
                        new Rating { Label = "Management", Value = 4 },
                        new Rating { Label = "Overall Facilities", Value = 4 }
                    },
                    "Sports activities are good and students enjoy them.",
                    "More equipment should be available for indoor games."
                ),
                CreateFeedbackDoc(
                    studentsList[3], "event", "Web Technology Workshop", null, null, eventId4,
                    new List<Rating> {
                        new Rating { Label = "Organization", Value = 5 },
                        new Rating { Label = "Management", Value = 5 },
                        new Rating { Label = "Usefulness", Value = 4 },
                        new Rating { Label = "Environment", Value = 4 },
                        new Rating { Label = "Overall Experience", Value = 5 }
                    },
                    "The workshop was very useful for our web technology project.",
                    "Arrange more hands-on sessions like this."
                )
            };
            await Feedbacks.InsertManyAsync(feedbacksList);

            // 11. Create Audit Logs
            var logsList = new List<AuditLog>
            {
                new AuditLog
                {
                    Actor = adminId1,
                    ActorModel = "Admin",
                    ActorRole = "admin",
                    Action = "SEED_DATABASE",
                    EntityType = "Database",
                    EntityId = "",
                    Details = "Created clean demo database with separate admins and student users via auto-seeding.",
                    IpAddress = "127.0.0.1",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new AuditLog
                {
                    Actor = studentId1,
                    ActorModel = "User",
                    ActorRole = "user",
                    Action = "DEMO_FEEDBACK",
                    EntityType = "Feedback",
                    EntityId = "",
                    Details = "Demo student feedback records inserted for testing.",
                    IpAddress = "127.0.0.1",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            await AuditLogs.InsertManyAsync(logsList);
        }
    }
}
