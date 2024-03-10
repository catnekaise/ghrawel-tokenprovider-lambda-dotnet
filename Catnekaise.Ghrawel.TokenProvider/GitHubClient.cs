using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Catnekaise.Ghrawel.TokenProvider.Models.GitHub;

namespace Catnekaise.Ghrawel.TokenProvider;

public class GitHubClient
{
    private readonly HttpClient _client;
    private readonly string _appJwt;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        TypeInfoResolver = GitHubJsonSerializerContext.Default,
    };

    private static readonly Dictionary<(int appId, string owner), int> KnownInstallations = new();

    public GitHubClient(HttpClient client, string appJwt)
    {
        _client = client;
        _appJwt = appJwt;
    }

    [SuppressMessage("Trimming", "IL2026")]
    [SuppressMessage("AOT", "IL3050")]
    private T? Send<T>(HttpMethod method, string path, object? body = default) where T : class
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add("Authorization", $"Bearer {_appJwt}");
        request.Headers.Add("Accept", "application/vnd.github.v3+json");
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        request.Headers.Add("User-Agent", "CatnekaiseGhrawel");

        if (body != null)
        {
            var bodyStr = JsonSerializer.Serialize(body, SerializerOptions);
            request.Content = new StringContent(bodyStr, Encoding.UTF8, "application/json");
        }

        var response = _client.Send(request);
        var responseBody = response.Content.ReadAsStringAsync().Result;

        if (!response.IsSuccessStatusCode)
        {
            var err = JsonSerializer.Deserialize<GitHubErrorResponse>(responseBody, SerializerOptions);
            throw new TokenProviderException(500, "Error", $"StatusCode: {response.StatusCode:D}, Message: {err?.Message}");
        }

        return JsonSerializer.Deserialize<T>(responseBody, SerializerOptions);
    }
    
    public int? FindInstallationId(int appId, string owner)
    {
        var ok = KnownInstallations.TryGetValue((appId, owner), out var knownInstallationId);

        if (ok)
        {
            return knownInstallationId;
        }

        var installations = Send<List<Installation>>(HttpMethod.Get, "app/installations");

        var installationId = installations?.SingleOrDefault(x => x.User.Login == owner)?.Id;

        if (installationId != null)
        {
            KnownInstallations.Add((appId, owner), installationId.Value);
        }

        return installationId;
    }
    
    public string? GetToken(int installationId, Dictionary<string, string> permissions,
        List<string>? repo)
    {
        var request = new GitHubTokenRequest
        {
            Repositories = repo,
            Permissions = permissions
        };

        var installations = Send<GitHubTokenResponse>(HttpMethod.Post,
            $"app/installations/{installationId}/access_tokens", request);

        return installations?.Token;
    }
}

[JsonSerializable(typeof(List<Installation>))]
[JsonSerializable(typeof(GitHubTokenResponse))]
[JsonSerializable(typeof(GitHubTokenRequest))]
[JsonSerializable(typeof(GitHubErrorResponse))]
public partial class GitHubJsonSerializerContext : JsonSerializerContext;