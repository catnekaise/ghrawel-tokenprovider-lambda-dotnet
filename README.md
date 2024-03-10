# ghrawel TokenProvider Lambda .NET
This application template is used by [catnekaise/ghrawel](https://github.com/catnekaise/ghrawel) and that repository also contains all documentation. For specifics about this application, read [here](https://github.com/catnekaise/ghrawel/blob/main/docs/token-provider/application.md).

## Logging
Proper logging will be added via AWS PowerTools once [chore: AOT support for Logging and Metrics #557](https://github.com/aws-powertools/powertools-lambda-dotnet/pull/557) has been merged.

## Performance
Init is less than 200ms in most memory configurations. A large amount of the time in the first request and an even larger amount of time in additional requests is spent on `rsa.ImportFromPem(privateKey)`. This time can be decreased by increasing function memory. For now, no option for not repeating importing of the private key has been created.

## Environment Variables

| Var             | Examples                           |
|-----------------|------------------------------------|
| SECRETS_STORAGE | PARAMETER_STORE or SECRETS_MANAGER |
| SECRETS_PREFIX  | /catnekaise/github-apps            |
| DEBUG_LOGGING   | true                               |