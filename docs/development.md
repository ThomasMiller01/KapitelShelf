# Development Guide

This guide outlines the contributor workflow for KapitelShelf. It complements the project-wide [CONTRIBUTING.md](../CONTRIBUTING.md) file with the conventions the team relies on day-to-day.

## Contribution Workflow

1. **Work from `main`.** Keep your local copy up to date before starting new work.
2. **Create a feature branch.** Use a descriptive branch name that reflects the change you are making.
3. **Open a Pull Request (PR).** Every feature, enhancement, or bugfix must go through a PR targeting `main`.
4. **Squash commits before merge.** Each PR should merge as a single, squashed commit to keep history clean and predictable.

### Pull Request Checklist

Follow the Conventional Commits specification for every PR title. This enables our automated tooling to generate changelogs and version bumps accurately.

- Format: `<type>(<scope>): <description>`
  - Examples: `feat: add dark mode toggle`, `fix(frontend): handle empty search result`
- Breaking changes must include a `!` after the type: `feat!: remove legacy API`
- Reference: [https://www.conventionalcommits.org/en/v1.0.0/](https://www.conventionalcommits.org/en/v1.0.0/)

A well-structured PR description should explain **what** changed, **why** it was necessary, and any testing that validates the behaviour. Follow the PR description template.

## Release Management

When publishing a new version of any component (e.g. Helm chart, API client, or frontend package), use the dedicated release workflow outlined below.

### 1. Create a Release Branch

```bash
git checkout main
git pull
git checkout -b release/vX.Y.Z
```

### 2. Run the "Release: Bump X" Task in VS Code

From the Command Palette select **Tasks: Run Task** and choose the appropriate bump task (e.g. `Release: Bump Helm`). This script will:

- Update the version in the relevant manifest (`Chart.yaml`, `package.json`, or `KapitelShelf.X.csproj`).
- Commit the change as `chore(release): X.Y.Z`.
- Create the matching tag (e.g. `helm@X.Y.Z`).

### 3. Open a Pull Request

Submit the branch `release/vX.Y.Z` against `main`. **Do not squash-merge** this PR; the release commit and tag must remain intact on `main`.

### 4. Complete the Merge with a Merge Commit

Use the regular merge-commit strategy. This keeps the original tag pointed at the published commit so `commit-and-tag-version` can compute the next changelog correctly.

After merging, CI picks up the new tag and publishes the release artefacts automatically.
