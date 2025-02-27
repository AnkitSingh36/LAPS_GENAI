using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace LinkedInAutomation.Core.Services
{
    public class HuggingFaceGenerator : IAIContentGenerator
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiToken;
        private const string API_URL = "https://api-inference.huggingface.co/models/tiiuae/falcon-7b-instruct";

        public HuggingFaceGenerator(HttpClient httpClient, string apiToken)
        {
            _apiToken = apiToken;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}");
        }

        public async Task<string> GenerateContentAsync(string topic)
        {
            var prompt = $@"Write a professional, engaging LinkedIn post about **{topic}**, with a strong hook, key insights, bullet points, emojis, hashtags, and a CTA—under 3000 characters.";

            var payload = new { inputs = prompt }; 
            var response = await _httpClient.PostAsJsonAsync(API_URL, payload);

            if (!response.IsSuccessStatusCode)
            {
                return $"API Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
            }

            var result = await response.Content.ReadAsStringAsync();

            try
            {
                var deserializedData = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(result);

                if (deserializedData != null && deserializedData.Count > 0 && deserializedData[0].ContainsKey("generated_text"))
                {
                    var generatedText = deserializedData[0]["generated_text"];
                    return generatedText.Replace(prompt, "").Trim();
                }
            }
            catch (JsonException)
            {
                return $"Failed to parse API response: {result}";
            }

            return "No content generated.";
        }
    }
}
