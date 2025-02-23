using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Net.Http.Json;
using LinkedInAutomation.Core.Models;


namespace LinkedInAutomation.Core.Services
{
    public class LinkedInService : ILinkedInService
    {
        private readonly string? _clientId;
        private readonly string? _clientSecret;
        private readonly string? _accessToken;
        private readonly HttpClient _httpClient;
        private readonly ILogger<LinkedInService> _logger;
        private string? _userUrn;

        public LinkedInService( string clienId, string clientSecret, string accessToken, ILogger<LinkedInService> logger)
        {
            _clientId = clienId;
            _clientSecret = clientSecret;
            _accessToken = accessToken;
            _logger = logger;
            _httpClient = new HttpClient { BaseAddress = new Uri("https://api.linkedin.com/v2/") };
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task<bool> AddHashtags(string[] hashtags)
        {
            if (!hashtags.Any()) return true;

            var hashtagString = string.Join(" ", hashtags.Select(h => $"#{h}"));
            return await PostContent(hashtagString);
        }

        public async Task<string> GetUserProfile()
        {
            var response = await _httpClient.GetAsync("me");
            response.EnsureSuccessStatusCode();

            var profile = await response.Content.ReadFromJsonAsync<LinkedInProfile>();
           
            return $"{profile?.LocalizedFirstName} {profile?.LocalizedLastName}"; 
        }

        public async Task Initialize()
        {
            try
            {
                _logger.LogInformation("Requesting LinkedIn profile data");
                var profileResponse = await _httpClient.GetAsync("me");
                profileResponse.EnsureSuccessStatusCode();

                var profile = await profileResponse.Content.ReadFromJsonAsync<LinkedInProfile>();
                _userUrn = profile?.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initial LinkedIN Service");
            }
        }

        public async Task<bool> IsAuthenticated()
        {
            try
            {
                var response = await _httpClient.GetAsync("me");
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> PostContent(string content)
        {
            try
            {
                var payload = new
                {
                    author = $"urn:li:person{_userUrn}",
                    lifecycleState = "PUBLISHED",
                    specificContent = new
                    {
                        com_linkedin_ugc_shareContent = new
                        {
                            shareCommentary = new
                            {
                                text = content
                            },
                            shareMediaCategory = "None"
                        }
                    },
                    visibility = new
                    {
                        com_linkedin_ugc_MemberNetworkVisibility = "PUBLIC"
                    }
                };
                
            var response = await _httpClient.PostAsJsonAsync(
                    "ugcPosts",
                    payload
                );

            return response.IsSuccessStatusCode;
            

        }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Filed to post content");
            }
            return false;
        }

        public Task<string> PreviewPost(string content)
        {
            return Task.FromResult(content);
        }

        public async Task<bool> SchedulePost(string content, DateTime scheduledGTime)
        {
            if (scheduledGTime <= DateTime.UtcNow)
                return await PostContent(content);
            throw new NotImplementedException("Post scheduling is not supported by LinkedIn API");
        }
    }
}
