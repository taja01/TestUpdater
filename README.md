# TestUpdater

A production-ready .NET 10 application that automatically synchronizes test case definitions from Gherkin feature files and TypeScript test files to Azure DevOps Test Cases.

## Overview

TestUpdater parses test specifications from multiple sources (Reqnroll/Gherkin `.feature` files, TypeScript `.spec.ts` files) and automatically updates corresponding Azure DevOps Test Cases with structured test steps, maintaining traceability between your test automation and test management system.

## Key Features

- ✅ **Multi-Format Support** - Parse Reqnroll/Gherkin and TypeScript test files
- 🔒 **Secure Configuration** - User secrets and environment variable support for sensitive data
- 🔄 **Resilient HTTP Communication** - Automatic retry with exponential backoff and timeout policies
- 📊 **Structured Logging** - Serilog with file and console outputs
- ✔️ **Configuration Validation** - Startup validation with detailed error messages
- 🏥 **Health Checks** - Azure DevOps connectivity monitoring
- ⚡ **Async/Await Throughout** - Non-blocking I/O operations
- 🎯 **SOLID Principles** - Clean architecture with dependency injection

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Azure DevOps account with a Personal Access Token (PAT)
- Access to Azure DevOps project with test case management enabled

## Installation

1. **Clone the repository**

git clone https://github.com/taja01/TestUpdater.git cd TestUpdater

2. **Restore dependencies**

cd TestUpdaterTool dotnet restore

3. **Set up User Secrets**
   
   Navigate to the `TestRunner` project folder:
```
cd TestRunner 
dotnet user-secrets init dotnet user-secrets set "AzureOptions:PersonalAccessToken" "your-azure-devops-pat-here"
```

To create a PAT, visit: `https://dev.azure.com/{YourOrganization}/_usersSettings/tokens`

Required scopes:
- **Work Items**: Read & Write

## Configuration

### appsettings.json
```
{
  "TestRunnerOptions": {
    "TestFilesPath": "example/reqnroll",      // Path to your test files
    "UseRelativePath": true                   // true = relative to executable, false = absolute path
  },
  "AzureOptions": {
    "Organization": "YourOrganization",       // Azure DevOps organization name
    "Project": "YourProject",                 // Azure DevOps project name
    "ApiVersion": "7.1-preview.3"             // API version (default works for most cases)
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",                     // Log level: Debug, Information, Warning, Error
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### Environment Variables (Alternative to User Secrets)

For CI/CD or containerized environments:

**Linux/macOS:**

export AZUREOPTIONS__PERSONALACCESSTOKEN="your-pat-here"
export TESTRUNNEROPTIONS__TESTFILESPATH="/path/to/tests"

**Windows PowerShell:**

$env:AZUREOPTIONS__PERSONALACCESSTOKEN = "your-pat-here" 
$env:TESTRUNNEROPTIONS__TESTFILESPATH = "C:\path\to\tests"

**Docker:**

ENV AZUREOPTIONS__PERSONALACCESSTOKEN="your-pat-here"

## Usage

### Running the Application

cd TestRunner dotnet run

### Test File Format

#### Reqnroll/Gherkin (.feature)
```
Feature: User Authentication

@TC123456 
Scenario: Successful login with valid credentials 
Given the user is on the login page 
When the user enters valid username "testuser" 
And the user enters valid password "password123" 
And the user clicks the login button 
Then the user should be redirected to the dashboard 
And the user should see a welcome message
```

**Tag Format:** `@TC{TestCaseId}` - Links the scenario to Azure DevOps Test Case ID

#### TypeScript (.system.spec.ts)
```
test({ testCaseId: 123456, title: 'User can login successfully' }, async ({ page }) => { // Test implementation });
```

### Output

The application will:
1. Parse all test files matching the configured pattern
2. Validate test case structure and IDs
3. Update Azure DevOps Test Cases with structured steps
4. Log success/failure metrics

**Console Output:**

[12:34:56 INF] Starting application... 
[12:34:56 INF] Runner is starting the service. 
[12:34:56 INF] Processing file: example/reqnroll/login.feature
[12:34:57 INF] Successfully updated test case 'Successful login' (ID: 123456)
[12:34:57 INF] Runner completed processing. Success: 15, Failed: 0

## Project Structure



## Architecture

### Design Patterns

- **Dependency Injection**: All services registered in IoC container
- **Strategy Pattern**: Pluggable parsers via `ITestCaseParser`
- **Repository Pattern**: `IFileHandler` abstracts file system
- **Options Pattern**: Strongly-typed configuration with validation

### Key Components

| Component | Responsibility | Lifetime |
|-----------|---------------|----------|
| `Runner` | Orchestrates the update process | Scoped (HostedService) |
| `TestProcessor` | Processes test files using parsers | Transient |
| `TestCaseValidator` | Validates parsed test cases | Transient |
| `AzureDevOpsService` | Communicates with Azure DevOps API | Transient (HttpClient) |
| `FileHandler` | Async file I/O operations | Transient |
| `ReqnRollParser` | Parses Gherkin feature files | Singleton |

### Resilience Policies

- **Retry Policy**: 3 attempts with exponential backoff (2s, 4s, 8s)
- **Timeout Policy**: 30 seconds per HTTP request
- **Cancellation Support**: Graceful shutdown via CancellationToken

## Development

### Building
```
dotnet build
```
### Running Tests

```
dotnet test
```

### Adding a New Parser

1. Implement `ITestCaseParser`:
```
public class MyCustomParser : ITestCaseParser { public string FilePattern => "*.custom";

public Task<List<ParsedTest>> ParseFileAsync(string filePath, CancellationToken cancellationToken)
{
    // Parse logic here
}
```

2. Register in `Program.cs`:
```
services.AddSingleton<ITestCaseParser, MyCustomParser>();
```

### Logging

Logs are written to:
- **Console**: All log levels with timestamps
- **File**: `logs/log-{Date}.txt` (rolling daily)

Adjust log levels in `appsettings.json`:
```
{ 
  "Serilog": { 
    "MinimumLevel": { 
        "Default": "Information"  // Debug | Information | Warning | Error 
        } 
    } 
}
```

## CI/CD Integration

### GitHub Actions Example
```
name: Update Test Cases

on: push: branches: [main] paths: - 'tests/**/*.feature'

jobs: update: runs-on: ubuntu-latest steps: - uses: actions/checkout@v4

  - name: Setup .NET
    uses: actions/setup-dotnet@v4
    with:
      dotnet-version: '10.0.x'
  
  - name: Run TestUpdater
    env:
      AZUREOPTIONS__PERSONALACCESSTOKEN: ${{ secrets.AZURE_PAT }}
    run: |
      cd TestUpdaterTool/TestRunner
      dotnet run

```
### Azure Pipelines Example
```
trigger: branches: include: - main paths: include: - tests/**/*.feature
pool: vmImage: 'ubuntu-latest'
variables:
•	group: azure-devops-secrets  # Variable group containing PAT
steps:
•	task: UseDotNet@2 inputs: version: '10.0.x'
•	script: | cd TestUpdaterTool/TestRunner dotnet run env: AZUREOPTIONS__PERSONALACCESSTOKEN: $(AzurePAT)
```

## Troubleshooting

### Common Issues

**Issue: "Azure Personal Access Token not configured"**
- **Solution**: Ensure user secrets are set or environment variable is configured

dotnet user-secrets set "AzureOptions:PersonalAccessToken" "your-pat"

**Issue: "Failed to update Test Case ID {id}. Status: 401"**
- **Solution**: PAT is invalid or expired. Generate a new PAT with correct scopes

**Issue: "Failed to update Test Case ID {id}. Status: 404"**
- **Solution**: Test case doesn't exist in Azure DevOps or wrong Organization/Project

**Issue: "No files found to process"**
- **Solution**: Check `TestFilesPath` in `appsettings.json` and parser `FilePattern`

### Debug Mode

Enable detailed logging:

```
{ "Serilog": { "MinimumLevel": { "Default": "Debug" } } }
```
## Security Best Practices

- ✅ **Never commit** `appsettings.json` with PAT values
- ✅ **Use User Secrets** for local development
- ✅ **Use Key Vault** or secret managers in production
- ✅ **Rotate PATs** regularly (recommended: every 90 days)
- ✅ **Limit PAT scope** to minimum required permissions (Work Items: Read & Write)
- ✅ Add `secrets.json` to `.gitignore`

## Performance

- **Async I/O**: Non-blocking file and HTTP operations
- **Streaming**: Files processed one at a time (low memory footprint)
- **Connection Pooling**: HttpClient factory pattern
- **Cancellation**: Graceful shutdown on CTRL+C

**Typical Performance:**
- ~50-100 test cases/minute (network dependent)
- Memory usage: ~50-100 MB
- CPU usage: Low (I/O bound)

## Roadmap

- [ ] Support for additional test frameworks (JUnit, NUnit, xUnit)
- [ ] Bulk update optimization
- [ ] Dry-run mode
- [ ] Detailed diff reporting
- [ ] Webhook integration for automatic updates

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For issues and questions:
- Open an [Issue](https://github.com/taja01/TestUpdater/issues)
- Check existing [Discussions](https://github.com/taja01/TestUpdater/discussions)

## Acknowledgments

Built with:
- [Serilog](https://serilog.net/) - Structured logging
- [Polly](https://github.com/App-vNext/Polly) - Resilience policies
- [Gherkin](https://github.com/cucumber/gherkin) - Gherkin parser
- [Newtonsoft.Json](https://www.newtonsoft.com/json) - JSON serialization

---

**Made with ❤️ for test automation engineers**