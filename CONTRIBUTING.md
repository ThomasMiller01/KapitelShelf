# Contributing to KapitelShelf

Thank you for your interest in improving KapitelShelf! This guide explains how to propose changes, report issues, and collaborate with the community.

## Code of Conduct

By participating in this project you agree to uphold the [Contributor Covenant Code of Conduct](https://www.contributor-covenant.org/version/2/1/code_of_conduct/). Be respectful, inclusive, and considerate in all interactions.

## Ways to Contribute

- **Report bugs** using the [issue tracker](https://github.com/ThomasMiller01/KapitelShelf/issues). Include reproduction steps, logs, screenshots, and environment details when possible.
- **Suggest enhancements** by opening a feature request issue. Describe the motivation, proposed solution, and alternatives considered.
- **Submit code or documentation** improvements through pull requests (PRs). See the workflow below.
- **Triage issues** by reproducing reports, confirming fixes, or labelling tickets.
- **Improve documentation** in the [`docs/`](./docs) directory to help new users and contributors.

## Development Environment

KapitelShelf combines a .NET backend, a browser-based frontend, Helm charts, and supporting automation. Follow the setup, dependency, and workflow guidance documented in the [Development Guide](./docs/development.md) to prepare your environment and discover the available build and run scripts.

## Workflow for Changes

1. **Discuss first.** For large work items, open an issue or comment on an existing ticket to align on scope.
2. **Create a feature branch** from `main`. Keep branches focused on a single change.
3. **Write tests and docs** alongside code updates. Update existing documentation when behaviour changes.
4. **Run checks locally.** At minimum, build the affected components and run relevant test suites or linters.
5. **Follow the PR description template** and provide context, screenshots, and testing results. Explain *what* changed and *why*. The template asks for a description, motivation, type-of-change checkbox, testing and documentation confirmation, related issues, and any additional notes.
6. **Keep commits atomic** and use Conventional Commit messages. Squash commits before merge unless you are preparing a release branch.
7. **Request review** from maintainers. Address feedback promptly and courteously.

## Testing Expectations

- Backend changes should compile with `dotnet build` and include or update automated tests when appropriate.
- Frontend changes should build successfully and include any manual verification relevant to the change.
- Helm and infrastructure updates should be validated with `helm template`.
- Documentation-only updates do not require automated tests but should be proofread for clarity and correctness.

Document the commands you ran in the PR description so reviewers can reproduce your results.

## Pull Request Guidelines

- Target the `main` branch unless maintainers direct otherwise.
- Title your PR using the Conventional Commits format: `<type>(<scope>): <description>`.
- Link related issues with `Fixes #<issue-number>` or `Closes #<issue-number>` in the PR body when applicable.
- Include screenshots or screen recordings for UI changes. If you cannot provide them, explain why.
- Update translations, schemas, and generated files when behaviour changes.
- After approval, maintainers will merge using squash merges for feature branches. Release branches follow the merge strategy documented in the [Development Guide](./docs/development.md#release-management).

## Documentation Style

- Place new guides or references under the most relevant directory inside [`docs/`](./docs).
- Keep the tone friendly, concise, and task-oriented.
- When adding screenshots, store them alongside related content under `docs/.attachments/`.
- Avoid duplicating information; link to existing guides where possible.

## Community Support

Questions that are not bug reports can be raised in the [GitHub Discussions](https://github.com/ThomasMiller01/KapitelShelf/discussions/categories/general) forum. You can also join issues tagged with `good first issue` to get started.

We appreciate your contributions and look forward to building KapitelShelf together!
