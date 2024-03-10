namespace Catnekaise.Ghrawel.TokenProvider.Models.Internal;

public class TokenRequest
{
    public required string Owner { get; init; }
    public string? Repo { get; init; }
}