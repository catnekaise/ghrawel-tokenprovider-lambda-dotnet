using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Catnekaise.Ghrawel.TokenProvider.Models.Internal;

public class TokenContext
{
    [JsonPropertyName("providerName")]
    public required string ProviderName { get; init; }

    [JsonPropertyName("permissions")]
    public required Dictionary<string, string> Permissions { get; init; }

    [JsonPropertyName("app")]
    public required App App { get; init; }

    [JsonPropertyName("endpoint")]
    public required Endpoint Endpoint { get; init; }

    [JsonPropertyName("targetRule")]
    public required TargetRule TargetRule { get; init; }
}

public class App
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }
}

public class Endpoint
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }
}

public class TargetRule
{
    [JsonPropertyName("repositorySelectionMode")]
    public required string RepositorySelectionMode { get; init; }
}