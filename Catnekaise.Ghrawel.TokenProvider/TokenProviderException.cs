using System;

namespace Catnekaise.Ghrawel.TokenProvider;

public class TokenProviderException : Exception
{
    public static string Format(int statusCode, string message, string? errorDetails)
    {
        return $"{{\"patternSelection\":\"CK_ERR_{statusCode}\",\"message\":\"{message}\",\"errorDetails\":\"{errorDetails}\"}}";
    }

    public TokenProviderException(int statusCode, string message, string? errorDetails = default) : base(Format(statusCode, message, errorDetails))
    {
    }

    public TokenProviderException(int statusCode, string message, Exception? exception) : base(
        Format(statusCode, message, exception?.Message), exception)
    {
    }
}