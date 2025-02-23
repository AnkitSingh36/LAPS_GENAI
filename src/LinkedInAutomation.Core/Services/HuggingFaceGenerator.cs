using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http.Json;

namespace LinkedInAutomation.Core.Services
{
    public class HuggingFaceGenerator : IAIContentGenerator
    {
        private readonly HttpClient _client;
        private readonly string _apiToken;
        private const string API_URL = "https://api-inference.huggingface.co/models/mistralai/Mistral-7B-Instruct-v0.1";

        public HuggingFaceGenerator(string apiToken)
        {
            _apiToken = apiToken;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}");

        }

        public async Task<string> GenerateContentAsync(string topic)
        {
            var prompt = $@"Create a professional LinkedIn Post about {topic}.
                            The Post should be engaging, informative, and include relevant hashtag.
                            Keep it under 3000 charachter.";

            var payload = new { input = prompt };
            var response = await _client.PostAsJsonAsync(API_URL, payload);
            var result = await response.Content.ReadAsStringAsync();

            var deserializedData = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(result);

            if (deserializedData == null || deserializedData.Count == 0 || !deserializedData[0].ContainsKey("generated_text"))
            {
                return string.Empty; 
            }

            return deserializedData[0]["generated_text"];
        }
    }
}
