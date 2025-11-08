# PokedexApp

A .NET 8 Web API application for Pokémon information.

![CI](https://github.com/ZarakiKanzaki/PokedexApp/workflows/CI/badge.svg)
[![codecov](https://codecov.io/gh/ZarakiKanzaki/PokedexApp/branch/master/graph/badge.svg)](https://codecov.io/gh/ZarakiKanzaki/PokedexApp)

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Building the Project

```bash
dotnet restore
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Running with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

## CI/CD

This project uses GitHub Actions for continuous integration. The workflow:

1. Builds the solution
2. Runs all tests
3. Collects code coverage using coverlet
4. Uploads coverage reports to Codecov

## Setting Up Codecov

To enable code coverage reporting:

1. Go to [codecov.io](https://codecov.io) and sign in with your GitHub account
2. Add your repository to Codecov
3. Copy the repository token from Codecov dashboard
4. Go to your GitHub repository settings → Secrets and variables → Actions
5. Add a new secret named `CODECOV_TOKEN` with the token value
6. The CI workflow will automatically upload coverage reports on each push/PR

## Project Structure

- `PokedexApp.BasicInfo/` - Core domain logic and queries
- `PokedexApp.WebApi/` - Web API controllers and middleware
- `PokedexAppTest.BasicInfo/` - Unit tests for core logic
- `PokedexAppTest.WebApi/` - Unit and integration tests for Web API

## Code Coverage

Code coverage is collected using coverlet and uploaded to Codecov. The coverage report includes:

- Line coverage
- Branch coverage
- Method coverage

Coverage reports are also available as artifacts in GitHub Actions runs.