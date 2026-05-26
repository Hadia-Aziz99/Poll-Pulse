using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PollpulseBackend.Models
{
    [BsonIgnoreExtraElements]
    public class Rating
    {
        [BsonElement("label")]
        public string Label { get; set; } = string.Empty;

        [BsonElement("value")]
        public int Value { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class SentimentResult
    {
        [BsonElement("score")]
        public double Score { get; set; } = 0;

        [BsonElement("label")]
        public string Label { get; set; } = "Neutral"; // Positive, Neutral, Negative

        [BsonElement("comparative")]
        public double Comparative { get; set; } = 0;
    }

    [BsonIgnoreExtraElements]
    public class Feedback
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("user")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string User { get; set; } = string.Empty;

        [BsonElement("category")]
        public string Category { get; set; } = string.Empty; // faculty, teacher, course, transport, cafeteria, sports, library, event

        [BsonElement("targetName")]
        public string TargetName { get; set; } = string.Empty;

        [BsonElement("teacher")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Teacher { get; set; }

        [BsonElement("course")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Course { get; set; }

        [BsonElement("event")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Event { get; set; }

        [BsonElement("ratings")]
        public List<Rating> Ratings { get; set; } = new List<Rating>();

        [BsonElement("averageRating")]
        public double? AverageRating { get; set; }

        [BsonElement("comment")]
        public string Comment { get; set; } = string.Empty;

        [BsonElement("suggestion")]
        public string Suggestion { get; set; } = string.Empty;

        [BsonElement("sentiment")]
        public SentimentResult Sentiment { get; set; } = new SentimentResult();

        [BsonElement("degree")]
        public string Degree { get; set; } = string.Empty; // BSCS or BSIT

        [BsonElement("year")]
        public int Year { get; set; }

        [BsonElement("section")]
        public string Section { get; set; } = string.Empty; // A, B, C

        [BsonElement("classKey")]
        public string ClassKey { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
