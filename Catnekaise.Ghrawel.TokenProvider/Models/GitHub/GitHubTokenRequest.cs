using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Catnekaise.Ghrawel.TokenProvider.Models.GitHub;

public class GitHubTokenRequest
{
    [JsonPropertyName("repositories")]
    public List<string>? Repositories { get; set; }

    [JsonPropertyName("permissions")]
    public required Dictionary<string, string> Permissions { get; set; }

}