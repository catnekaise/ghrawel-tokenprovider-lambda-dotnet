using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using Catnekaise.Ghrawel.TokenProvider.Models.GitHub;

namespace Catnekaise.Ghrawel.TokenProvider.Tests;

public class GitHubClientTests
{
    private static GitHubClient CreateClient(params TestResponse[] testResponses)
    {
        return new GitHubClient(
            new HttpClient(new Handler(testResponses)) { BaseAddress = new Uri("https://api.github.com") },
            "secret");
    }

    [Fact]
    public void Installation_Found()
    {
        var githubClient = CreateClient(InstallationsResponse());

        Assert.Equal(1234, githubClient.FindInstallationId(1, "catnekaise"));
    }

    [Fact]
    public void Installation_NotFound()
    {
        var githubClient = CreateClient(InstallationsResponse());

        Assert.Null(githubClient.FindInstallationId(1, "test"));
    }

    [Fact]
    public void Token_Received()
    {
        var githubClient = CreateClient(InstallationsResponse(),
            TokenResponse(1234));

        var token = githubClient.GetToken(1234, new() { ["contents"] = "read" }, null);

        Assert.NotNull(token);
    }

    [Fact]
    public void Token_Error()
    {
        var githubClient = CreateClient(InstallationsResponse(),
            TokenResponse(1234, HttpStatusCode.Unauthorized));

        Assert.Throws<TokenProviderException>(() => githubClient.GetToken(1234, new() { ["contents"] = "read" }, null));
    }

    public static TestResponse InstallationsResponse()
    {
        return req =>
        {
            if (req.Method != HttpMethod.Get || !req.RequestUri!.ToString().EndsWith("app/installations"))
            {
                return null;
            }

            var body = new List<Installation>
            {
                new Installation
                {
                    Id = 1234,
                    User = new User
                    {
                        Login = "catnekaise"
                    }
                }
            };

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
        };
    }

    public static TestResponse TokenResponse(int installationId, HttpStatusCode? httpStatusCode = default)
    {
        return req =>
        {
            if (req.Method != HttpMethod.Post ||
                !req.RequestUri!.ToString().EndsWith($"app/installations/{installationId}/access_tokens"))
            {
                return null;
            }

            object body;

            if (httpStatusCode == null)
            {
                body = new GitHubTokenResponse
                {
                    Token = "ghp_"
                };
            }
            else
            {
                body = new GitHubErrorResponse
                {
                    Message = "Error"
                };
            }

            return new HttpResponseMessage(httpStatusCode ?? HttpStatusCode.Created)
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
        };
    }
}

public delegate HttpResponseMessage? TestResponse(HttpRequestMessage request);

public class Handler : DelegatingHandler
{
    private readonly TestResponse[] _testResponses;

    public Handler(params TestResponse[] testResponses)
    {
        _testResponses = testResponses;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        foreach (var interceptor in _testResponses)
        {
            var response = interceptor.Invoke(request);

            if (response != null)
            {
                return response;
            }
        }

        throw new NotImplementedException();
    }
}