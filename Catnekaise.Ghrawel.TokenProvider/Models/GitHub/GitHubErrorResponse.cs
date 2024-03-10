using System.Text.Json.Serialization;

namespace Catnekaise.Ghrawel.TokenProvider.Models.GitHub;

public class GitHubErrorResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}