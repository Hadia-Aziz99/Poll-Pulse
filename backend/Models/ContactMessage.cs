using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PollpulseBackend.Models
{
    [BsonIgnoreExtraElements]
    public class ContactMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("rating")]
        public int Rating { get; set; }

        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;

        [BsonElement("ipAddress")]
        public string IpAddress { get; set; } = string.Empty;

        [BsonElement("userAgent")]
        public string UserAgent { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}