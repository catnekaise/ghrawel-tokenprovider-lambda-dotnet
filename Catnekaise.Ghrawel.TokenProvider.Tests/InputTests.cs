namespace Catnekaise.Ghrawel.TokenProvider.Tests;

public class InputTests
{
    [Theory]
    [InlineData("catnekaise", "catnekaise")]
    [InlineData("catne#kaise", null)]
    public void ReadOwner(string owner, string? passes)
    {
        Assert.Equal(passes, Input.ReadOwner(owner));
    }

    [Theory]
    [InlineData("DEFAULT", "AT_LEAST_ONE", "example-repo", true)]
    [InlineData("DEFAULT", "AT_LEAST_ONE", null, false)]
    [InlineData("DEFAULT", "AT_LEAST_ONE", "", false)]
    [InlineData("DEFAULT", "AT_LEAST_ONE", "example-repo,repo-2", false)]
    [InlineData("DEFAULT", "ALLOW_OWNER", "example-repo,repo-2", false)]
    [InlineData("DEFAULT", "AT_LEAST_ONE", "example#repo", false)]
    [InlineData("DYNAMIC_OWNER", "AT_LEAST_ONE", "example-repo", true)]
    [InlineData("DYNAMIC_OWNER", "AT_LEAST_ONE", "example-repo,repo-2", true)]
    [InlineData("DYNAMIC_OWNER", "AT_LEAST_ONE", null, false)]
    [InlineData("DYNAMIC_OWNER", "ALLOW_OWNER", null, true)]
    
    public void ReadRepositories(string endpointType, string repositorySelectionMode, string? repo, bool passes)
    {
        var (repositories, errorMessage) = Input.ReadRepositories(endpointType, repositorySelectionMode, repo);

        Assert.True(passes ? errorMessage == null : errorMessage != null);
    }
    
    [Theory]
    [InlineData("/", "/")]
    [InlineData("/catnekaise/github-apps", "/catnekaise/github-apps")]
    [InlineData("/catnekaise/github-apps/", null)]
    [InlineData("catne#kaise", null)]
    public void SecretPrefix(string prefix, string? passes)
    {
        Assert.Equal(passes, Input.ReadSecretsPrefix(prefix));
    }
    
    [Theory]
    [InlineData("PARAMETER_STORE", "PARAMETER_STORE")]
    [InlineData("SECRETS_MANAGER", "SECRETS_MANAGER")]
    [InlineData("PARAMS", null)]
    [InlineData("S3", null)]
    public void SecretsStorage(string storage, string? passes)
    {
        Assert.Equal(passes, Input.ReadSecretsStorage(storage));
    }
}