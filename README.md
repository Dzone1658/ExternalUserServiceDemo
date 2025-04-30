# ExternalUserServiceDemo

A .NET 8 solution demonstrating integration with a public API (https://reqres.in/) using HttpClient, caching, retry policies (Polly), and dependency injection.

## 📦 Projects
- `ExternalUserService`: Class library for API client logic.
- `ConsoleAppDemo`: Console UI using the library.
- `ExternalUserService.Tests`: xUnit tests for the client.

## 🚀 Getting Started
```bash
dotnet restore
dotnet build
dotnet run --project ConsoleAppDemo
```

## 🧪 Run Tests
```bash
dotnet test ExternalUserService.Tests
```

## ✅ Features
- API client using `HttpClientFactory`
- Polly retry policy
- In-memory caching
- Configuration via `appsettings.json`
- Unit testing with Moq and xUnit
- Works with VS Code (see .vscode folder)
