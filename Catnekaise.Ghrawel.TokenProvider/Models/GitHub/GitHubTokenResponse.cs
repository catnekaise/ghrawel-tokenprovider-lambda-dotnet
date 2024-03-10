using System.Text.Json.Serialization;

namespace Catnekaise.Ghrawel.TokenProvider.Models.GitHub;

public class GitHubTokenResponse
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }
}