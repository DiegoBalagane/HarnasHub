# HarnasHub - Developer Guidelines

## Language
- Reply to the user in POLISH (global rule). Code, identifiers, code comments and git commits in ENGLISH.
- Exception: root `README.md` is a recruiter-facing showcase and stays in ENGLISH. Everything under `docs/` stays in POLISH.

## Tech Stack
- Backend: ASP.NET Core (.NET 10), Vertical Slices, MediatR 12.5.0 (pinned, never bump), ErrorOr, FluentValidation, EF Core + PostgreSQL, SignalR.
- Frontend: React 19, TypeScript, Vite, TailwindCSS, TanStack Query, Zustand, React Router.
- Files (demos, training materials): S3-compatible storage — never in the database or the repo.
- Detailed conventions live in `backend/CLAUDE.md` and `frontend/CLAUDE.md` (auto-loaded when working there). Architecture: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md), priorities: [docs/ROADMAP.md](docs/ROADMAP.md) — read only when the task needs them.

## Working rules
- Generated dirs (`bin/`, `obj/`, `node_modules/`, `dist/`, `build/`, `.vs/`, `TestResults/`) are denied in `.claude/settings.json` — don't try to work around it unless the task is about the artifact itself.
- Run builds/tests/migrations/installs without asking; confirm only destructive operations (deleting files/dirs, DB reset, force push, hard reset).
- Keep files ~200 lines; split oversized files you are already editing (don't expand scope to unrelated files).
- One-sentence doc comment on every exported/public symbol.
- A change that affects architecture, API surface, setup or conventions updates `README.md`/`docs/` in the same change.
- New handler/endpoint/validator/non-trivial hook or component ships with tests in the same change.

## Git — HIGHEST PRIORITY
- Never `git commit`/`git push` unless explicitly asked in that message.
- No Claude/AI mention anywhere in history: no `Co-Authored-By` trailer, no AI author/committer identity, no AI footer in PRs. Attribution is disabled in settings; `SessionStart` hook sets the human identity and `.githooks`. Details and history: [.claude/GIT.md](.claude/GIT.md) (local, gitignored).
- After committing, verify with `git log -3 --format='%an <%ae>%n%B'`: author `Diego Bałagane <kacpermln@wp.pl>`, no AI lines.
- Conventional Commits (`feat:`, `fix:`, `docs:`, `refactor:`, `style:`, `chore:`, `test:`).
- Never push directly to `main`; never force-push/rewrite `main` without an explicit request in that message.
