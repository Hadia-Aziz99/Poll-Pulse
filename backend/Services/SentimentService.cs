using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PollpulseBackend.Models;

namespace PollpulseBackend.Services
{
    public class SentimentResultDto
    {
        public double Score { get; set; }
        public double Comparative { get; set; }
        public string Label { get; set; } = "Neutral";
    }

    public class SentimentService
    {
        private readonly List<string> _positiveWords = new List<string> 
        { 
            "good", "great", "excellent", "helpful", "best", "clean", "useful", "supportive", 
            "amazing", "satisfied", "clear", "friendly", "improved", "comfortable", "organized" 
        };

        private readonly List<string> _negativeWords = new List<string> 
        { 
            "bad", "poor", "worst", "late", "dirty", "unfair", "difficult", "rude", 
            "boring", "slow", "issue", "problem", "expensive", "crowded", "weak" 
        };

        public SentimentResultDto AnalyzeSentiment(string text)
        {
            var input = (text ?? "").Trim();
            var lower = input.ToLower();
            
            double score = 0;
            
            foreach (var word in _positiveWords)
            {
                if (lower.Contains(word)) score += 1;
            }
            
            foreach (var word in _negativeWords)
            {
                if (lower.Contains(word)) score -= 1;
            }

            var wordsCount = Regex.Split(lower, @"\s+").Where(w => !string.IsNullOrEmpty(w)).Count();
            double comparative = wordsCount > 0 ? score / wordsCount : 0;

            string label = "Neutral";
            if (score > 0) label = "Positive";
            else if (score < 0) label = "Negative";

            return new SentimentResultDto
            {
                Score = score,
                Comparative = comparative,
                Label = label
            };
        }

        public SentimentPercentages CalculatePercentages(List<SentimentResult> sentiments)
        {
            int total = sentiments.Count;
            int positiveCount = 0;
            int neutralCount = 0;
            int negativeCount = 0;

            foreach (var s in sentiments)
            {
                if (s.Label == "Positive") positiveCount++;
                else if (s.Label == "Negative") negativeCount++;
                else neutralCount++;
            }

            int denominator = total > 0 ? total : 1;

            return new SentimentPercentages
            {
                Total = total,
                Positive = (int)Math.Round((double)positiveCount / denominator * 100),
                Neutral = (int)Math.Round((double)neutralCount / denominator * 100),
                Negative = (int)Math.Round((double)negativeCount / denominator * 100)
            };
        }
    }

    public class SentimentPercentages
    {
        public int Total { get; set; }
        public int Positive { get; set; }
        public int Neutral { get; set; }
        public int Negative { get; set; }
    }
}
