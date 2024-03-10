using System;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Catnekaise.Ghrawel.TokenProvider.Models.Internal;

namespace Catnekaise.Ghrawel.TokenProvider;

[JsonSerializable(typeof(TokenProviderResponse))]
[JsonSerializable(typeof(Request))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext;

public static class Function
{
    private static readonly HttpClient Client = new HttpClient
    {
        BaseAddress = new Uri("https://api.github.com")
    };

    private static string? _secretsStorage;
    private static string? _secretsPrefix;

    private static async Task Main()
    {
        _secretsStorage = Input.ReadSecretsStorage(Environment.GetEnvironmentVariable("SECRETS_STORAGE"));
        _secretsPrefix = Input.ReadSecretsPrefix(Environment.GetEnvironmentVariable("SECRETS_PREFIX"));

        var handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler,
                new SourceGeneratorLambdaJsonSerializer<LambdaFunctionJsonSerializerContext>())
            .Build()
            .RunAsync();
    }

    public static async Task<TokenProviderResponse> FunctionHandler(Request input, ILambdaContext context)
    {
        context.Logger.LogInformation("Starting");

        if (_secretsStorage == null)
        {
            context.Logger.LogError("Invalid Secrets Storage");
            throw new TokenProviderException(500, "Error");
        }

        if (_secretsPrefix == null)
        {
            context.Logger.LogError("Invalid Secrets Prefix");
            throw new TokenProviderException(500, "Error");
        }

        var owner = Input.ReadOwner(input.TokenRequest.Owner);

        if (owner == null)
        {
            context.Logger.LogWarning("Invalid Owner specified by user");
            throw new TokenProviderException(400, "Invalid owner specified");
        }

        var (repositories, repositoryErrorMessage) = Input.ReadRepositories(
            input.TokenContext.Endpoint.Type,
            input.TokenContext.TargetRule.RepositorySelectionMode,
            input.TokenRequest.Repo
        );

        if (repositoryErrorMessage != null)
        {
            context.Logger.LogWarning(repositoryErrorMessage);
            throw new TokenProviderException(400, repositoryErrorMessage);
        }

        var privateKey = PrivateKey.GetPrivateKey(_secretsStorage, _secretsPrefix, input.TokenContext.App.Name);

        if (privateKey == null)
        {
            context.Logger.LogError("Could not resolve private key for unknown reasons.");
            throw new TokenProviderException(500, "Error");
        }

        var appJwt = PrivateKey.CreateJwt(
            privateKey, input.TokenContext.App.Id);

        if (appJwt == null)
        {
            context.Logger.LogError("Could not create GitHub App JWT");
            throw new TokenProviderException(500, "Error");
        }

        var githubClient = new GitHubClient(Client, appJwt);

        var installationId = githubClient.FindInstallationId(input.TokenContext.App.Id, input.TokenRequest.Owner);

        if (installationId == null)
        {
            context.Logger.LogError($"Could not find installation id for owner {owner}");
            throw new TokenProviderException(500, "Error");
        }

        var token = githubClient.GetToken(installationId.Value, input.TokenContext.Permissions, repositories);

        if (token == null)
        {
            context.Logger.LogError("Could not create token");
            throw new TokenProviderException(500, "Error");
        }

        return new TokenProviderResponse
        {
            Token = token
        };
    }
}