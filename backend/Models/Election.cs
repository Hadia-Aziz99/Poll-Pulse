using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PollpulseBackend.Models
{
    [BsonIgnoreExtraElements]
    public class Election
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("electionType")]
        public string ElectionType { get; set; } = string.Empty; // CR or GR

        [BsonElement("degree")]
        public string Degree { get; set; } = string.Empty; // BSCS or BSIT

        [BsonElement("year")]
        public int Year { get; set; }

        [BsonElement("section")]
        public string Section { get; set; } = string.Empty; // A, B, C

        [BsonElement("classKey")]
        public string ClassKey { get; set; } = string.Empty;

        [BsonElement("status")]
        public string Status { get; set; } = "draft"; // draft, active, closed

        [BsonElement("startAt")]
        public DateTime? StartAt { get; set; }

        [BsonElement("endAt")]
        public DateTime? EndAt { get; set; }

        [BsonElement("createdBy")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatedBy { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsOpen
        {
            get
            {
                var now = DateTime.UtcNow;
                var startsOk = !StartAt.HasValue || StartAt.Value <= now;
                var endsOk = !EndAt.HasValue || EndAt.Value >= now;
                return Status == "active" && startsOk && endsOk;
            }
        }
    }
}
