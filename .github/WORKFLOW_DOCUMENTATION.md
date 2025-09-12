# CI/CD Workflow Documentation

## Overview

The Event Management System uses GitHub Actions for continuous integration and deployment. This document explains how the CI/CD pipeline works and how to use it effectively.

## Workflow Files

### `.github/workflows/ci.yml`
Main CI/CD pipeline that handles:
- Building .NET 8.0 projects
- Running tests
- Code quality analysis
- Artifact management

## Workflow Jobs

### 1. Build Job
**Purpose**: Compile and test the codebase
**Runs on**: `ubuntu-latest`

**Steps**:
1. **Checkout**: Downloads repository code
2. **Setup .NET 8.0**: Installs .NET 8.0 SDK
3. **Display Version**: Shows .NET version for verification
4. **Restore Dependencies**: Downloads NuGet packages
5. **Build Solution**: Compiles all projects
6. **Run Tests**: Executes unit/integration tests
7. **Upload Results**: Saves test artifacts

### 2. Code Quality Job
**Purpose**: Perform additional code analysis
**Runs on**: `ubuntu-latest`
**Depends on**: Build job success

**Steps**:
1. **Checkout**: Downloads repository code
2. **Setup .NET 8.0**: Installs .NET 8.0 SDK
3. **Restore Dependencies**: Downloads NuGet packages
4. **Code Analysis**: Runs static analysis tools

## Trigger Conditions

### Pull Requests
Workflow runs when PRs are opened/updated targeting:
- `main` branch
- `develop` branch

### Push Events
Workflow runs on pushes to:
- `develop` branch
- `feature/*` branches
- `hotfix/*` branches

## Branch Protection Integration

The CI workflow integrates with GitHub branch protection rules:

1. **Required Status Checks**: Both `build` and `code-quality` jobs must pass
2. **Up-to-date Requirement**: Branches must be current with target
3. **Review Requirement**: Manual approval needed before merge

## Smart Project Detection

The workflow intelligently handles different repository states:

### No .NET Projects
- Workflow runs successfully
- Reports "No projects found" 
- Ready for future project addition

### Solution Files (*.sln)
- Uses solution for restore/build/test
- Builds entire solution at once

### Individual Projects (*.csproj)
- Discovers project files automatically
- Builds each project separately

### Test Projects
Automatically detects and runs tests from projects matching:
- `*Test*.csproj`
- `*.Test.csproj`
- `*.Tests.csproj`

## Artifact Management

### Test Results
- **Format**: TRX files
- **Location**: `TestResults/` directory
- **Retention**: Available for download after workflow completion
- **Access**: Via GitHub Actions UI

## Environment Requirements

### .NET Version
- **Required**: .NET 8.0.x
- **Installation**: Automatic via `actions/setup-dotnet@v4`
- **Verification**: Version displayed in workflow logs

### Dependencies
- **NuGet Packages**: Restored automatically
- **Cache**: Not implemented (can be added for performance)

## Error Handling

### Build Failures
- Workflow fails if compilation errors occur
- Prevents merge until issues resolved
- Error details available in workflow logs

### Test Failures
- Workflow fails if any tests fail
- Test results uploaded for analysis
- Details available in TRX artifacts

### Missing Projects
- Workflow succeeds with informational messages
- Ready for project addition
- No false failures on empty repositories

## Best Practices

### For Developers
1. **Test Locally**: Run `dotnet build` and `dotnet test` before pushing
2. **Small Commits**: Keep changes focused and reviewable
3. **Clear Messages**: Use descriptive commit messages
4. **Branch Naming**: Follow `feature/`, `hotfix/`, `bugfix/` conventions

### For Reviewers
1. **Check CI Status**: Ensure all checks pass
2. **Review Changes**: Look at code changes thoroughly
3. **Test Coverage**: Verify new code includes tests
4. **Documentation**: Ensure docs are updated if needed

### For Maintainers
1. **Status Checks**: Configure required checks in repository settings
2. **Review Requirements**: Set minimum number of reviewers
3. **Protection Rules**: Enable branch protection on main/develop
4. **Team Access**: Configure appropriate permissions

## Troubleshooting

### Common Issues

#### "No projects found"
- **Cause**: No .NET projects in repository yet
- **Solution**: Normal behavior, add projects when ready

#### Build failures
- **Cause**: Compilation errors in code
- **Solution**: Fix errors locally, then push

#### Test failures
- **Cause**: Unit tests failing
- **Solution**: Review test results in artifacts, fix failing tests

#### Permission errors
- **Cause**: Insufficient GitHub permissions
- **Solution**: Check repository access and team memberships

### Getting Help

1. **Workflow Logs**: Check detailed logs in GitHub Actions tab
2. **Test Artifacts**: Download TRX files for test details
3. **Documentation**: Review this guide and branch protection docs
4. **Team Support**: Contact repository maintainers

## Future Enhancements

Potential improvements to consider:

### Performance
- NuGet package caching
- Build output caching
- Parallel job execution

### Quality
- Code coverage reporting
- Static analysis tools (SonarQube)
- Security scanning

### Deployment
- Staging environment deployment
- Production deployment automation
- Database migration handling

### Notifications
- Slack/Teams integration
- Email notifications
- Custom status badges