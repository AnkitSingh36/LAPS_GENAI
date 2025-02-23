using System.Text.Json.Serialization;
using System.Net.Http.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace LinkedInAutomation.Core.Models
{
    public class LinkedInProfile
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("localizedFirstName")]
        public string? LocalizedFirstName { get; set; }

        [JsonPropertyName("localizedLastName")]
        public string? LocalizedLastName { get; set; }

        [JsonPropertyName("profilePicture")]
        public ProfilePicture? ProfilePicture { get; set; }

        [JsonPropertyName("vanityName")]
        public string? VanityName { get; set; }

        [JsonPropertyName("headline")]
        public string? Headline { get; set; }
    }

    public class ProfilePicture
    {
        [JsonPropertyName("displayImage")]
        public string? DisplayImage { get; set; }
    }
}