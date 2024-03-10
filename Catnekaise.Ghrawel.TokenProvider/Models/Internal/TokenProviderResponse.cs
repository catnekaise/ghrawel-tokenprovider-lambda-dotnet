using System.Text.Json.Serialization;

namespace Catnekaise.Ghrawel.TokenProvider.Models.Internal;

public class TokenProviderResponse
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }
}