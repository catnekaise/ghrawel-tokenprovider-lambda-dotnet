using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Catnekaise.Ghrawel.TokenProvider;

public static class PrivateKey
{
    private static AmazonSimpleSystemsManagementClient? _ssmClient;
    private static AmazonSecretsManagerClient? _secretsManagerClient;

    public static string? GetPrivateKey(
        string secretsStorage, string secretsPrefix, string app)
    {
        return secretsStorage == "PARAMETER_STORE"
            ? GetPrivateKeyFromParameterStore(secretsPrefix, app)
            : GetPrivateKeySecretsManager(secretsPrefix, app);
    }

    public static string? GetPrivateKeySecretsManager(
        string secretsPrefix, string app)
    {
        _secretsManagerClient ??= new AmazonSecretsManagerClient();
        
        try
        {
            var privateKey = _secretsManagerClient.GetSecretValueAsync(new GetSecretValueRequest
            {
                SecretId = $"{secretsPrefix}/{app}",
            }).Result;

            return privateKey.SecretString;
        }
        catch (Exception e)
        {
            throw new TokenProviderException(500, "Error", e);
        }
    }

    public static string? GetPrivateKeyFromParameterStore(
        string secretsPrefix, string app)
    {
        _ssmClient ??= new AmazonSimpleSystemsManagementClient();

        try
        {
            var privateKey = _ssmClient.GetParameterAsync(new GetParameterRequest
            {
                Name = $"{secretsPrefix}/{app}",
                WithDecryption = true
            }).Result;

            return privateKey.Parameter.Value;
        }
        catch (Exception e)
        {
            throw new TokenProviderException(500, "Error", e);
        }
    }

    public static string? CreateJwt(string privateKey, int appId)
    {
        using var rsa = RSA.Create();

        rsa.ImportFromPem(privateKey);

        var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
        {
            CryptoProviderFactory = new CryptoProviderFactory
            {
                CacheSignatureProviders = false
            }
        };

        var issuedAt = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(1));
        var expiresAt = issuedAt.Add(TimeSpan.FromMinutes(5));

        var claims = new Dictionary<string, object>
        {
            ["iat"] = int.Parse(issuedAt.ToUnixTimeSeconds().ToString()),
            ["exp"] = int.Parse(expiresAt.ToUnixTimeSeconds().ToString())
        };

        var handler = new JsonWebTokenHandler();

        var jwtSecurityToken = handler.CreateToken(new SecurityTokenDescriptor
        {
            Claims = claims,
            Issuer = appId.ToString(),
            SigningCredentials = signingCredentials,
        });

        return jwtSecurityToken;
    }
}