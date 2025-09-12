# Branch Protection Configuration Guide

## Overview
This repository includes GitHub Actions CI/CD workflows that enforce code quality and testing before allowing merges to the main branch.

## Required Branch Protection Settings

To ensure branches cannot be merged directly to main without approval, configure the following branch protection rules in your GitHub repository:

### Steps to Configure Branch Protection:

1. **Navigate to Repository Settings**
   - Go to your repository on GitHub
   - Click on "Settings" tab
   - Select "Branches" from the left sidebar

2. **Add Branch Protection Rule for `main`**
   - Click "Add rule"
   - Enter `main` as the branch name pattern
   - Configure the following settings:

### Required Settings:

#### ✅ Restrict pushes that create files larger than a specified limit
- Check: "Restrict pushes that create files larger than 100 MB"

#### ✅ Require a pull request before merging
- Check: "Require a pull request before merging"
- Set: "Required number of approvals before merging: 1"
- Check: "Dismiss stale reviews when new commits are pushed"
- Check: "Require review from CODEOWNERS" (if you have a CODEOWNERS file)

#### ✅ Require status checks to pass before merging
- Check: "Require status checks to pass before merging"
- Check: "Require branches to be up to date before merging"
- Add required status checks:
  - `build` (from CI workflow)
  - `code-quality` (from CI workflow)

#### ✅ Require conversation resolution before merging
- Check: "Require conversation resolution before merging"

#### ✅ Require signed commits
- Check: "Require signed commits" (optional but recommended)

#### ✅ Require linear history
- Check: "Require linear history" (optional, prevents merge commits)

#### ✅ Include administrators
- Check: "Include administrators" (applies rules to repository admins)

### Additional Recommended Settings:

#### Allow force pushes
- Uncheck: "Allow force pushes" (recommended for main branch)

#### Allow deletions
- Uncheck: "Allow deletions" (prevents accidental deletion of main branch)

## Workflow Enforcement

The CI workflow (`ci.yml`) automatically:
- ✅ Runs on all pull requests to `main` and `develop`
- ✅ Uses .NET 8.0 for consistent builds
- ✅ Builds the solution/projects
- ✅ Runs tests (when test projects exist)
- ✅ Performs code quality checks
- ✅ Blocks merging if any step fails

## Branch Strategy

### Recommended Git Flow:
1. **main** - Production-ready code only
2. **develop** - Integration branch for features
3. **feature/** - Feature development branches
4. **hotfix/** - Emergency fixes for production

### Workflow:
1. Create feature branches from `develop`
2. Open pull requests to merge feature branches into `develop`
3. Open pull requests to merge `develop` into `main` for releases
4. All pull requests require CI checks to pass and approval before merging

## Status Checks

The following status checks must pass before merging to main:
- **build**: Ensures code compiles successfully
- **code-quality**: Runs additional code analysis

These checks are automatically triggered by the GitHub Actions workflow and will appear as required status checks on pull requests when properly configured.