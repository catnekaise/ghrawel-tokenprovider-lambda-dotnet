using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Catnekaise.Ghrawel.TokenProvider;

public static partial class Input
{
    [GeneratedRegex("^/$|^/[a-zA-Z][a-zA-Z0-9/-]+[a-z]$")]
    private static partial Regex PrefixRegEx();
    
    [GeneratedRegex("^[a-zA-Z][a-zA-Z0-9-]+$")]
    private static partial Regex OwnerRegex();
    
    [GeneratedRegex("^[a-zA-Z][a-zA-Z0-9_.-]+$")]
    private static partial Regex RepoRegex();

    public static string? ReadSecretsPrefix(string? value)
    {

        return PrefixRegEx().IsMatch(value ?? "") ? value : null;
    }

    public static string? ReadSecretsStorage(string? value)
    {
        return value switch
        {
            "PARAMETER_STORE" => value,
            "SECRETS_MANAGER" => value,
            _ => null
        };
    }

    public static string? ReadOwner(string owner)
    {
        return OwnerRegex().IsMatch(owner) ? owner : null;
    }

    public static (List<string>? repositories, string? errorMessage) ReadRepositories(string endpointType,
        string repositorySelectionMode, string? repo)
    {
        var repositories = (repo ?? "").Split(",").Where(x => x != "").ToList();
        var ownerEndpoint = endpointType is "STATIC_OWNER" or "DYNAMIC_OWNER";

        var count = repositories.Count(x => x != "");

        var errorMessage = (repositorySelectionMode, ownerEndpoint, count) switch
        {
            { repositorySelectionMode: "AT_LEAST_ONE", ownerEndpoint: false, count: 0 } => "At least one repository has to be specified",
            { repositorySelectionMode: "AT_LEAST_ONE", ownerEndpoint: false, count: > 1 } => "Exactly one repository must be specified",
            { repositorySelectionMode: "ALLOW_OWNER", ownerEndpoint: false } => "Error",
            { repositorySelectionMode: "AT_LEAST_ONE", ownerEndpoint: true, count: 0 } => "At least one repository has to be specified",
            _ => null
        };

        if (errorMessage != null)
        {
            return (null, errorMessage);
        }

        if (repositories.Any(x => !RepoRegex().IsMatch(x)))
        {
            return (null, "One or more repositories provided has an invalid name");
        }

        return (repositories.Count > 0 ? repositories : null, null);
    }


}