# Development Guide

This guide outlines the contributor workflow for KapitelShelf. It complements the project-wide [CONTRIBUTING.md](../CONTRIBUTING.md) file with the conventions the team relies on day-to-day.

## Contribution Workflow

1. **Work from `main`.** Keep your local copy up to date before starting new work.
2. **Create a feature branch.** Use a descriptive branch name that reflects the change you are making.
3. **Open a Pull Request (PR).** Every feature, enhancement or bugfix must go through a PR targeting `main`.
4. **Squash commits before merge.** Each PR should merge as a single, squashed commit to keep history clean and predictable.

### Pull Request Checklist

Follow the Conventional Commits specification for every PR title. This enables our automated tooling to generate changelogs and version bumps accurately.

- Format: `<type>(<scope>): <description>`
  - Examples: `feat: add dark mode toggle`, `fix(frontend): handle empty search result`
- Breaking changes must include a `!` after the type: `feat!: remove legacy API`
- Reference: [https://www.conventionalcommits.org/en/v1.0.0/](https://www.conventionalcommits.org/en/v1.0.0/)

A well-structured PR description should explain **what** changed, **why** it was necessary, and any testing that validates the behaviour. Follow the PR description template.

## Release Management

KapitelShelf has one version for the frontend, API, Android app, and Helm chart. Every release rebuilds all artifacts, including components without source changes. Release notes live in the root [CHANGELOG.md](../CHANGELOG.md).

### 1. Create a Release Branch

```bash
git checkout main
git pull
git fetch origin --tags
git checkout -b release/vX.Y.Z
```

### 2. Prepare the Release

With a clean working tree and full Git history, run:

```bash
npm ci
npm run release -- --dry-run
npm run release
```

The equivalent VS Code task is **Release: Bump KapitelShelf**. `commit-and-tag-version` chooses the next version using the existing Conventional Commit rules. No version is forced by the release configuration. To deliberately override its recommendation, use the tool's `--release-as` option, for example `npm run release -- --release-as minor`.

`npm run release` invokes `commit-and-tag-version` directly. Its root configuration synchronizes the frontend, API, and chart versions, updates the chart's `frontend.image.tag` and `api.image.tag` defaults and documentation, generates `CHANGELOG.md`, and creates one `chore(release): vX.Y.Z` commit. Local tagging is disabled so publishing can tag the reviewed commit after merging.

The postchangelog hook runs `npm run gen:helm:docs` and includes the generated Helm README in the release commit. This requires a running Docker engine. Edit [the Helm README template](./helm/README.md.gotmpl) for prose changes; do not edit the generated README directly.

Review the generated changelog before opening the PR. The tool warns about unclassified commit messages; add any missing notable changes manually and commit the reviewed notes.

### 3. Open a Pull Request

Push the release branch and open a PR against `main`. Keep one release PR in flight at a time. If application changes merge while it is under review, update the release branch and reviewed changelog to include them before merging.

### 4. Merge and Publish

Merge the PR using the repository's normal merge strategy. **Publish KapitelShelf** checks out the merged commit, validates the release, and builds everything.

### Failed Releases

Publishing to GitHub, both registries, and Pages is not atomic. A failure can leave a draft GitHub release or some versioned artifacts published. The completed GitHub release is withheld until all required publishing steps succeed.

Retry **Publish KapitelShelf** with its manual `ref` input set to the **original release commit SHA shown in the failed run**, or rerun the failed workflow. The retry reuses the draft and version; it may rebuild artifacts but does not create another release. A tag or draft targeting a different commit is rejected. A completed release is skipped, and older releases cannot advance container aliases past a newer published release.
