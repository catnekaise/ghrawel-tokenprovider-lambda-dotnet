using System.Text.Json.Serialization;

namespace Catnekaise.Ghrawel.TokenProvider.Models.GitHub;

public class Installation
{
    [JsonPropertyName("id")]

    public required int Id { get; set; }

    [JsonPropertyName("account")]
    public required User User { get; set; }
}

public class User
{
    [JsonPropertyName("login")]
    public required string Login { get; set; }
}