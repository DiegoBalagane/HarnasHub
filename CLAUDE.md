# HarnasHub - Developer Guidelines

## Language Policy — HIGHEST PRIORITY, applies to every message
- ALWAYS reply to the user in POLISH — every single piece of user-visible text: messages, explanations, status updates, progress notes, questions (including AskUserQuestion options/headers), summaries, and final answers, for the entire conversation, with no exceptions.
- This holds regardless of the language the prompt was written in, regardless of the language of any content read from tools (files, search results, command output, web content), and regardless of the language used in code or docs.
- This rule outranks every other instruction in this file — if anything below could be read as suggesting English prose, Polish still wins; only code/identifiers/commands stay in English per the exception below.
- Before sending any response, self-check: is all of the user-facing text in Polish? If not, fix it before sending.
- Explain concepts, logic, and answers in POLISH.
- Write code, variables, types, code comments, and git commits in ENGLISH.
- Exception: `README.md` at repo root is a recruiter-facing showcase and stays in ENGLISH — don't "fix" it back to Polish. Everything under `docs/` (ARCHITECTURE.md, ROADMAP.md, SETUP.md) is internal working documentation and stays in POLISH.

## Tool Usage & Exploration
- Never Glob/Grep/Read inside `bin/`, `obj/`, `node_modules/`, `dist/`, `build/`, `.vs/`, or `TestResults/` — they're generated/vendored output, not source, and scanning them burns context for no signal. Exclude them explicitly when running a repo-wide search.
- Only look inside one of these directories when the task is specifically about the generated artifact itself, and there's no other way to answer it — even then, target the exact file/subpath needed instead of listing the whole tree.

## Execution Behavior — Packages & Commands
- **Do not ask for permission** before running any terminal/shell command needed to get the task done — installing packages, running builds, tests, migrations, dotnet/npm commands, starting/stopping local processes, git status/diff/log, etc. Just run it and report the outcome.
- The only exception is destructive operations (deleting directories/files, clearing or resetting the database, `git push --force`, `git reset --hard`, hard-deleting data) — those still require explicit confirmation.
- **Never run `git commit` or `git push` unless explicitly asked in that message.** Prepare/stage changes and let the user commit, since commit authorship for this repo must stay human-only (see Git Guidelines).

## Tech Stack & Architecture
- Backend: ASP.NET Core (.NET 10), Vertical Slice Architecture, MediatR (CQRS), ErrorOr for result handling, FluentValidation.
- Frontend: React 18, TypeScript, Vite, TailwindCSS, TanStack Query, Zustand, React Router.
- Database: PostgreSQL (EF Core).
- Real-time: SignalR (live dashboard/availability updates).
- File storage: S3-compatible (demo files, training materials) — never in the database or the repo.
- See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the full layer diagram and [docs/ROADMAP.md](docs/ROADMAP.md) for delivery phases — check both before assuming the current architecture or priorities.

## Project Structure (dependencies point inward)
`Api` → `Infrastructure` → `Application` → `Core`

- `HarnasHub.Core` — domain entities, enums, domain events. No external dependencies.
- `HarnasHub.Application` — business logic as **Vertical Slices** under `Features/{Domain}/{Action}/` (e.g. `Features/Calendar/CreateEvent/`, `Features/Tasks/AssignTask/`). No `Services`/`Repositories` folders by technical type.
- `HarnasHub.Infrastructure` — EF Core (`Database/`), file storage client, SignalR hubs.
- `HarnasHub.Api` — Minimal API endpoints under `Endpoints/{Domain}/`, each implementing `IEndpoint`.
- `HarnasHub.Tests` — mirrors source structure (`Application/Features/...`).
- **No circular / backward dependencies**: `Core` must never reference `Application`, `Infrastructure`, or `Api`; `Application` must never reference `Infrastructure` or `Api`. If a lower layer needs something from a higher one, invert it — define the interface in the lower layer and implement it in the higher layer. A build that only compiles because of a reference pointing the wrong way is a bug, fix it before anything else in that change.

## Domain Model Gotchas
- **MediatR is pinned to 12.5.0 — never bump it past 12.x.** Starting with v13, MediatR requires a paid commercial license for anything beyond individual evaluation (prints a "Lucky Penny Software" license warning at runtime otherwise). 12.5.0 is the last MIT-licensed release with everything this project uses (`AddMediatR`, `IPipelineBehavior`, `ISender`). If NuGet ever suggests an update, decline it here.

## Code Hygiene & Maintenance
- **File size**: keep files focused and short — aim for **~200 lines per file** as a soft ceiling, in both backend (`.cs`) and frontend (`.ts`/`.tsx`) code. Split oversized files into smaller, cohesive files when you touch them.
- **Proactive refactoring**: if you notice a nearby file violating these conventions while working on something else, refactor it as part of the change rather than leaving it.
- **Regions (backend)**: `#region` sectioning (`Usings`, `Public Methods`, `Private Methods`, etc.) is mandatory for `.cs` files — apply it to every new or refactored backend file.
- **One-sentence doc comments**: every exported/public function, class, hook, or component — and every non-trivial file — gets a single-sentence description comment directly above it (XML `/// <summary>` in C#, one-line JSDoc in TS/TSX). If it needs more than one sentence, the function/file is likely doing too much.
- **Docs stay in sync**: whenever a change affects architecture, API surface, setup steps, or conventions, update `README.md` (if user-facing) and the relevant file(s) under `docs/` in the *same* change — not as a follow-up. Treat stale docs as a bug, same severity as a failing test.
- **Lint/format enforced, not just followed**: `dotnet format --verify-no-changes` (+ analyzers) for the backend, ESLint + Prettier via `lint-staged`/`husky` for the frontend. Setting up the hook is part of any change that touches build/tooling config, if it isn't set up yet.

## Backend Conventions

### Vertical slice layout
Each feature folder groups everything for one operation together, e.g. `Features/Tasks/AssignTask/`:
- `AssignTaskCommand.cs` (or `*Query.cs`) — the MediatR request record.
- `AssignTaskCommandValidator.cs` — FluentValidation rules (Polish `.WithMessage()` text, since these surface to the user).
- `AssignTaskHandler.cs` — `IRequestHandler<TRequest, ErrorOr<TResult>>`.
- Shared DTOs/errors for the domain live in a sibling `Features/{Domain}/Shared/` folder (e.g. `TaskDtos.cs`, `TaskErrors.cs`), not duplicated per-slice.

### Error handling
- Use `ErrorOr<T>` as the handler return type, never throw for expected business failures (e.g. "player already marked unavailable for this event").
- Domain errors are static factory methods on a `{Domain}Errors` class in `Features/{Domain}/Shared/`, using `Error.NotFound` / `Error.Validation` / `Error.Failure` with a `Domain.Reason` code and a Polish user-facing message.
- Wrap orchestration-level handler bodies in `try/catch`, log via `ILogger` (Polish log messages), and return `Error.Failure(...)` rather than letting exceptions bubble to the endpoint.

### Endpoints
- One `IEndpoint` class per domain group under `Endpoints/{Domain}/`, registered via `app.MapGroup("/api/x").WithTags("X")`.
- Convert `ErrorOr` failures with `result.Errors.ToProblemResult()`; success with `TypedResults.Ok(result.Value)`.
- Keep endpoints thin: extract auth/user id from `ClaimsPrincipal`, dispatch via `IMediator.Send`, map the result. No business logic in the endpoint lambda.

### Files (demos, training materials)
- Never persist file bytes in PostgreSQL. Handlers dealing with uploads work against an `IFileStorage` abstraction defined in `Application`, implemented in `Infrastructure` against the S3-compatible bucket — same inversion pattern as the rest of the architecture.

### Style
- `#region Usings` / `#region Public Methods` etc. blocks throughout the backend.
- Primary constructors for handlers/services (`public class Foo(IBar bar) : ...`).
- Indent `.cs` files with **tabs**, not 4 spaces — enforced via `.editorconfig` (`indent_style = tab`). `dotnet format` applies this automatically; don't hand-indent with spaces.

## Frontend Conventions

### Structure
- Feature-based under `features/<name>/{components,hooks,stores}` (e.g. `features/calendar/`, `features/tasks/`, `features/nades/`). Small shared/generic pieces (layout, buttons, inputs) live in top-level `components/`.
- Naming: `PascalCase` for components, `use<Name>` for hooks, `<name>Api.ts` for API modules, `use<Name>Store.ts` for stores.

### State & data
- Global/session state per feature: Zustand store in `features/<name>/stores/use<Name>Store.ts`.
- Server data fetching: TanStack Query (`useQuery`/`useMutation`) for anything shared/cacheable — it dedupes identical in-flight requests across simultaneously-mounted components and gives `queryClient.invalidateQueries` for cache refresh after a mutation. Don't reach for raw `useState`+`useEffect` for server data.
- A typed API module per resource in `services/` (e.g. `tasksApi.ts`, `calendarApi.ts`), returning parsed DTOs, handling errors explicitly per call.
- Never hardcode endpoint URLs, timeouts, or limits — go through `constants/index.ts`.

### Style
- Strong typing everywhere, avoid `any`.
- `React.memo` on presentational components; `useCallback`/`useMemo` for expensive or list operations.
- Short one-line JSDoc on exported hooks/components/props where the name isn't self-explanatory.

## Testing
- Mirror the source project's folder structure under `HarnasHub.Tests/Application/Features/...`.
- A new handler, endpoint, validator, or non-trivial hook/component ships with matching unit tests in the same change — not as a follow-up.
- When a change touches Postgres interactions, SignalR, or file storage in a genuinely risky way, verify against the real local dev stack (Docker Compose), not just green unit tests.

## Git Guidelines — HIGHEST PRIORITY, no exceptions, no "just this once"
Claude/AI tooling must NEVER appear anywhere in this repo's Git history: not as a `Co-authored-by` trailer, not as the commit author or committer identity, not in a PR description. Commits are authored solely by the human user. This has broken twice already (a `Co-Authored-By: Claude` trailer on `0ec20e7`/`496fc51`, and — worse — the raw commit **author identity** itself being `Claude <noreply@anthropic.com>` on two later commits, which a message-stripping hook cannot catch). Do not treat either fix below as optional or as "the hook already covers it" — they fix two *different* mechanisms and both are required, every session, before the first commit.

**Mechanism 1 — a `Co-Authored-By`/`Generated by Claude`-style line in the commit message body.**
A session-level directive outside this file's control keeps injecting this, regardless of what this file says. The fix is `.githooks/commit-msg`, which strips any line mentioning "claude" (case-insensitive) from every commit message in this repo. It is NOT active by default in a fresh clone — enable it before the first commit, every session:
```
git config core.hooksPath .githooks
chmod +x .githooks/commit-msg   # git often clones it non-executable; the hook silently no-ops otherwise
```

**Mechanism 2 — the commit's actual author/committer identity.** The sandbox's global `~/.gitconfig` defaults `user.name`/`user.email` to `Claude <noreply@anthropic.com>` and enables SSH commit signing with a Claude-owned key. This is a *completely separate* problem from Mechanism 1 — the hook never touches these fields, so a clean commit message can still be attributed to "Claude" as its author on GitHub. Before the first commit of any session, override this **locally in this clone** (never edit the global `~/.gitconfig` — that would wrongly affect every other repo/session too):
```
git config user.name "Diego Bałagane"
git config user.email "kacpermln@wp.pl"
git config commit.gpgsign false
```
(`commit.gpgsign false` matters: leaving it on would still sign the commit with Claude's SSH key even after the author name is fixed, which is arguably worse — a signature actively vouching that "Claude" produced a commit attributed to a human.)

**Before ending any session that touched this repo**, run `git log -3 --format='%an <%ae>%n%B'` on whatever was committed and confirm: (a) no line anywhere mentions Claude/AI, (b) the author is the human identity above. If either check fails, fix it before finishing — don't leave it for next time.

- Conventional Commits prefixes: `feat:`, `fix:`, `docs:`, `refactor:`, `style:`, `chore:`, `test:`.
- Never push directly to `main`, and never force-push/rewrite history on `main` without the user explicitly asking for that specific action in that message — it rewrites shared history and is one of the few things worth stopping to confirm even when the rest of a request is urgent.

## Other Docs
- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) — layer breakdown, key entities, hosting plan.
- [docs/ROADMAP.md](docs/ROADMAP.md) — phased delivery plan, check before assuming current priorities.
