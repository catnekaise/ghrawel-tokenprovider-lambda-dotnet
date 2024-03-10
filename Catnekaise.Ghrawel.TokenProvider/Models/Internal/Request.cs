using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace Catnekaise.Ghrawel.TokenProvider.Models.Internal;

public class Request : APIGatewayProxyRequest
{
    [JsonPropertyName("tokenRequest")]
    public required TokenRequest TokenRequest { get; init; }
    
    [JsonPropertyName("tokenContext")]
    public required TokenContext TokenContext { get; init; }
}

