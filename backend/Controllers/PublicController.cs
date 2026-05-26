using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PollpulseBackend.Services;

namespace PollpulseBackend.Controllers
{
    [ApiController]
    [Route("api/public")]
    public class PublicController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;
        private readonly SentimentService _sentimentService;
        private readonly HttpClient _httpClient;

        public PublicController(MongoDBService mongoDBService, SentimentService sentimentService)
        {
            _mongoDBService = mongoDBService;
            _sentimentService = sentimentService;
            _httpClient = new HttpClient();
        }

        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics()
        {
            var activeElections = await _mongoDBService.Elections.CountDocumentsAsync(e => e.Status == "active");
            var closedElections = await _mongoDBService.Elections.CountDocumentsAsync(e => e.Status == "closed");
            var allFeedbacks = await _mongoDBService.Feedbacks.Find(_ => true).ToListAsync();
            
            var sentimentSummary = _sentimentService.CalculatePercentages(
                allFeedbacks.Select(f => f.Sentiment).ToList()
            );

            return Ok(new
            {
                activeElections,
                closedElections,
                totalFeedback = allFeedbacks.Count,
                sentiment = sentimentSummary
            });
        }

        [HttpGet("weather")]
        public async Task<IActionResult> GetWeather([FromQuery] double? lat, [FromQuery] double? lon)
        {
            if (!lat.HasValue || !lon.HasValue)
            {
                return BadRequest(new { error = "Latitude and longitude are required." });
            }

            try
            {
                var forecastUrl = $"https://api.open-meteo.com/v1/forecast?latitude={lat.Value}&longitude={lon.Value}&current=temperature_2m,weather_code&timezone=auto";
                var geoUrl = $"https://geocoding-api.open-meteo.com/v1/reverse?latitude={lat.Value}&longitude={lon.Value}&count=1&language=en&format=json";

                var forecastTask = _httpClient.GetStringAsync(forecastUrl);
                var geoTask = _httpClient.GetStringAsync(geoUrl);

                await Task.WhenAll(forecastTask, geoTask);

                using var forecastDoc = JsonDocument.Parse(forecastTask.Result);
                using var geoDoc = JsonDocument.Parse(geoTask.Result);

                string city = "Your City";
                if (geoDoc.RootElement.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array && results.GetArrayLength() > 0)
                {
                    var firstResult = results[0];
                    if (firstResult.TryGetProperty("name", out var nameProp))
                    {
                        city = nameProp.GetString() ?? "Your City";
                    }
                    else if (firstResult.TryGetProperty("admin1", out var admin1Prop))
                    {
                        city = admin1Prop.GetString() ?? "Your City";
                    }
                }

                double? temperature = null;
                if (forecastDoc.RootElement.TryGetProperty("current", out var currentObj) && currentObj.TryGetProperty("temperature_2m", out var tempProp))
                {
                    temperature = tempProp.GetDouble();
                }

                string unit = "°C";
                if (forecastDoc.RootElement.TryGetProperty("current_units", out var unitsObj) && unitsObj.TryGetProperty("temperature_2m", out var unitProp))
                {
                    unit = unitProp.GetString() ?? "°C";
                }

                int? weatherCode = null;
                if (forecastDoc.RootElement.TryGetProperty("current", out var currentObj2) && currentObj2.TryGetProperty("weather_code", out var codeProp))
                {
                    weatherCode = codeProp.GetInt32();
                }

                return Ok(new
                {
                    city,
                    temperature,
                    unit,
                    weatherCode
                });
            }
            catch (Exception)
            {
                return Ok(new { city = "Your City", temperature = (double?)null, unit = "°C", error = "Weather unavailable" });
            }
        }
    }
}
