# Event-Management-System

A .NET 8.0 based Event Management System with CI/CD pipeline integration.

## CI/CD Pipeline

This repository includes a comprehensive GitHub Actions CI/CD pipeline that ensures code quality and prevents direct merges to the main branch without proper review.

### Workflow Triggers

The CI pipeline runs on:
- **Pull Requests** to `main` and `develop` branches
- **Pushes** to `develop`, `feature/*`, and `hotfix/*` branches

### Required Status Checks

Before merging to main, the following checks must pass:
- `build` - Successful compilation
- `code-quality` - Code analysis validation


##  Development Workflow

1. Create feature branches from `develop`
2. Implement changes and ensure CI passes
3. Open pull request with proper description
4. Get approval from code reviewers
5. Merge after all checks pass

##  Getting Started

This repository is ready for .NET 8.0 development. When you add .NET projects:

1. The CI pipeline will automatically detect and build them
2. Test projects will be discovered and executed
3. Code quality checks will run on your codebase

### Prerequisites

- .NET 8.0 SDK
- Git
- GitHub account with appropriate permissions

### Local Development

```bash
# Clone the repository
git clone https://github.com/Promise30/Event-Management-System.git

# Navigate to project directory
cd Event-Management-System

# When .NET projects are added, you can:
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```


- [Branch Protection Setup](.github/BRANCH_PROTECTION.md)
- [Pull Request Template](.github/pull_request_template.md)
- [CI/CD Workflow](.github/workflows/ci.yml)
