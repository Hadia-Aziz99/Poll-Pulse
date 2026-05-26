using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PollpulseBackend.Models
{
    [BsonIgnoreExtraElements]
    public class Vote
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("election")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Election { get; set; } = string.Empty;

        [BsonElement("candidate")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Candidate { get; set; } = string.Empty;

        [BsonElement("user")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string User { get; set; } = string.Empty;

        [BsonElement("classKey")]
        public string ClassKey { get; set; } = string.Empty;

        [BsonElement("electionType")]
        public string ElectionType { get; set; } = string.Empty; // CR or GR

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
