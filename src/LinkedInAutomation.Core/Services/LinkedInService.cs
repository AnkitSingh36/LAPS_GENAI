using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LinkedInAutomation.Core.Models;

namespace LinkedInAutomation.Core.Services
{
    public class LinkedInService : ILinkedInService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private string? _accessToken;
        private readonly HttpClient _httpClient;
        private readonly ILogger<LinkedInService> _logger;
        private string? _userUrn;

        public LinkedInService(HttpClient httpClient, string clientId, string clientSecret, ILogger<LinkedInService> logger)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _logger = logger;
            _httpClient = httpClient;

            // Set base URL and Authorization header
            _httpClient.BaseAddress = new Uri("https://api.linkedin.com/v2/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task Initialize()
        {
            try
            {
                _logger.LogInformation("Fetching LinkedIn profile data...");
                var profileResponse = await _httpClient.GetAsync("me");
                profileResponse.EnsureSuccessStatusCode();

                var profile = await profileResponse.Content.ReadFromJsonAsync<LinkedInProfile>();
                _userUrn = profile?.Id;

                _logger.LogInformation($"Successfully initialized LinkedInService for user: {_userUrn}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize LinkedInService.");
                throw;
            }
        }

        public async Task<bool> PostContent(string content)
        {
            try
            {
                if (string.IsNullOrEmpty(_userUrn))
                {
                    throw new Exception("User URN is not initialized.");
                }

                var payload = new
                {
                    author = $"urn:li:person:{_userUrn}",
                    lifecycleState = "PUBLISHED",
                    specificContent = new
                    {
                        com_linkedin_ugc_shareContent = new
                        {
                            shareCommentary = new { text = content },
                            shareMediaCategory = "NONE"
                        }
                    },
                    visibility = new
                    {
                        com_linkedin_ugc_MemberNetworkVisibility = "PUBLIC"
                    }
                };

                var response = await _httpClient.PostAsJsonAsync("ugcPosts", payload);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to post content. Status: {response.StatusCode}");
                    return false;
                }

                _logger.LogInformation("Content posted successfully on LinkedIn.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to post content.");
                return false;
            }
        }

        public async Task<bool> IsAuthenticated()
        {
            try
            {
                var response = await _httpClient.GetAsync("me");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication check failed.");
                return false;
            }
        }

        public async Task<string> GetUserProfile()
        {
            try
            {
                var response = await _httpClient.GetAsync("me");
                response.EnsureSuccessStatusCode();

                var profile = await response.Content.ReadFromJsonAsync<LinkedInProfile>();
                return $"{profile?.LocalizedFirstName} {profile?.LocalizedLastName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch user profile.");
                throw;
            }
        }

        public async Task<bool> AddHashtags(string[] hashtags)
        {
            if (hashtags.Length == 0) return true;

            var hashtagString = string.Join(" ", hashtags.Select(h => $"#{h}"));
            return await PostContent(hashtagString);
        }

        public Task<string> PreviewPost(string content)
        {
            return Task.FromResult(content);
        }

        public async Task<bool> SchedulePost(string content, DateTime scheduledTime)
        {
            if (scheduledTime <= DateTime.UtcNow)
                return await PostContent(content);

            throw new NotImplementedException("LinkedIn API does not support post scheduling.");
        }
    }
}
