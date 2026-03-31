# AGENTS.md

Instructions for AI coding agents (Codex and similar) working in this repository.

## Scope

- This file applies to the entire repository.
- Prefer minimal, targeted changes over broad refactors.
- Preserve existing conventions and naming patterns in touched files.

## Project Overview

- Frontend: React + TypeScript + Vite (`frontend/`)
- Backend: ASP.NET Core + EF Core (`backend/src/`)
- Deployment: Helm chart (`helm/kapitelshelf/`)
- Docs: `docs/`

## Repo Map

- Backend API: `backend/src/KapitelShelf.Api`
- Backend tests: `backend/src/KapitelShelf.Api.Tests`
- DB migrations project: `backend/src/KapitelShelf.Data.Migrations`
- Frontend source: `frontend/src`
- Generated frontend API client: `frontend/src/lib/api/KapitelShelf.Api`

## Working Rules for Codex

- Read before editing: inspect adjacent files and follow local patterns.
- Keep diffs small and avoid unrelated cleanup.
- Do not rename/move files unless required by the task.
- Do not manually edit generated API client files in `frontend/src/lib/api/KapitelShelf.Api` unless the task explicitly asks for it.
- If backend API contracts change, regenerate swagger + client using the command below.

## Build, Test, and Validation

Run the smallest relevant checks first, then broader checks if needed.

### Frontend (`frontend/`)

- Install deps: `npm install`
- Lint: `npm run lint`
- Build: `npm run build`
- Dev server: `npm run dev`

### Backend (`backend/src/`)

- Build: `dotnet build`
- Run API: `dotnet watch run --project KapitelShelf.Api`
- Run tests: `dotnet test`

### Root (`/`)

- Dev bootstrap build: `npm run build:dev`
- Generate swagger + TS client: `npm run gen:swagger`
- Run frontend + backend for dev:
  - `npm run dev:run:frontend`
  - `npm run dev:run:backend:api`

### Helm

- Build chart assets: `npm run build:helm`

## API and Schema Changes

When changing DTOs/controllers/endpoints in `KapitelShelf.Api`:

1. Build backend first (`dotnet build` in `backend/src`).
2. Regenerate swagger/client (`npm run gen:swagger` at repo root).
3. Verify frontend still builds (`npm run build` in `frontend`).

## Database Changes

- Add migration from `backend/src` with:
  - `dotnet ef migrations add <MigrationName> --project ./KapitelShelf.Data.Migrations --startup-project ./KapitelShelf.Data.Migrations --context KapitelShelfDBContext`
- Ensure migrations are committed with the related model changes.

## Documentation Expectations

- Update docs when behavior, setup, or public API usage changes, and prefer updating existing docs under `docs/` instead of creating duplicates.
- Follow the current docs structure: `docs/README.md` is the docs entry point, `docs/references.md` is the feature-reference hub, and `docs/references/*.md` is the canonical home for user-facing feature guides.
- Prefer end-user, task-based guidance unless the task explicitly asks for developer or internal docs. Document relevant features, workflows, constraints, limitations, and outcomes rather than every visible UI detail.
- Prefer updating or linking to one canonical guide instead of duplicating the same workflow across multiple pages. When adding a major new user-facing guide, also check whether `README.md`, `docs/README.md`, `docs/references.md`, and `docs/quickstart.md` need discoverability links.
- Verify doc claims against the current implementation and do not document broader support than the code actually provides.
- Only add images when they are genuinely needed. Reuse existing assets in `docs/.attachments/` when available; otherwise insert `TODO: IMAGE - <description>` immediately after the step or subsection it should illustrate.
- Markdown blockquotes with `>` are allowed and recommended for callouts and quotes, for example `> ℹ️ ...`, `> ⚠️ ...`, or a plain `> ...`.
- When editing docs, do a lightweight validation pass: verify relative links, image paths or `TODO: IMAGE - ...` placeholders, feature claims against the implementation, and avoid unrelated cleanup.

## Commit and PR Conventions

- Use Conventional Commits style when asked to create commit messages:
  - `<type>(<scope>): <description>`
- Keep PR scope focused on one logical change.

## Safety

- Avoid destructive git commands (`reset --hard`, force checkout) unless explicitly requested.
- If the working tree contains unrelated edits, do not revert them.
- If a command fails, report the failure and the next best action.
