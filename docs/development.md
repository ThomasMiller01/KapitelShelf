# Development Guide

> More coming soon

## Pull Requests

When contributing to KapitelShelf, all feature and bugfix changes must be submitted via Pull Requests (PRs) against the `main` branch. To keep our history clean and consistent:

- **Squash your commits** into a single commit per PR. This ensures a linear history and makes changelog generation straightforward.
- **Follow the Conventional Commits specification** for the name of the PR:
  - PR names must follow the format: `<type>(<scope>): <description>`
  - Example: `feat: add dark mode toggle` or `fix: handle null response from /books`
  - Read more: https://www.conventionalcommits.org/en/v1.0.0/

A properly formatted, squashed commit allows our release tooling to automatically generate accurate changelogs and version bumps.

## Publishing a new Release

When publishing a new version of any component (e.g. the Helm chart, the API client, or frontend packages), please follow these steps:

### 1. **Create a release branch** off `main`:

```bash
git checkout main
git pull
git checkout -b release/vX.Y.Z
```

### 2. **Run the `Release: Bump X` task** in VSCode (Command Palette ▶️ `Tasks: Run Task` ▶️ select `Release: Bump Helm`, etc.).

This will:

- Update the version in the relevant manifest (`Chart.yaml`, `package.json` or `KapitelShelf.X.csproj`)
- Commit the bump under `chore(release): X.Y.Z` and tag it (e.g. `helm@X.Y.Z`)

### 3. **Open a Pull Request** from `release/vX.Y.Z` into `main`.

**Do not squash** this PR when merging. Squash merges rewrite the release-branch commit into a new commit on main, which causes the original tag to point at an unreachable commit. That breaks our `commit-and-tag-version` changelog generation.

### 4. **Merge with normal (merge-commit)** so the bump commit and its tag remain on `main`'s history

Once merged, CI will pick up the new tag and publish the release artifacts. This workflow preserves tags on their exact bump commits and ensures `commit-and-tag-version` can accurately compute the next changelog.
