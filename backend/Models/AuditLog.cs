using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PollpulseBackend.Models
{
    [BsonIgnoreExtraElements]
    public class AuditLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("actor")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Actor { get; set; }

        [BsonElement("actorModel")]
        public string? ActorModel { get; set; } // User or Admin

        [BsonElement("actorRole")]
        public string ActorRole { get; set; } = "system"; // user, admin, system

        [BsonElement("action")]
        public string Action { get; set; } = string.Empty;

        [BsonElement("entityType")]
        public string EntityType { get; set; } = string.Empty;

        [BsonElement("entityId")]
        public string EntityId { get; set; } = string.Empty;

        [BsonElement("details")]
        public string Details { get; set; } = string.Empty;

        [BsonElement("ipAddress")]
        public string IpAddress { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
