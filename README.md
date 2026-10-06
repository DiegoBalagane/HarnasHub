# HarnasHub

**A team management platform for HArnasiESport (CS2)** — one place for scheduling, coaching tasks, match results, and team analytics, replacing scattered Discord/Messenger threads.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=black)
![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-EF%20Core-4169E1?logo=postgresql&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-green)

## Overview

HarnasHub is a full-stack web app built for a competitive Counter-Strike 2 team to run its day-to-day operations: who's available for practice, what the coach expects from each player, how the last scrim went, and whether the team is actually improving over time.

It doubles as a hands-on playground for practicing AI-assisted software development — the entire codebase is built and iterated on with [Claude Code](https://claude.com/claude-code), used as a way to sharpen real engineering workflows (architecture decisions, code review, refactoring) rather than just prompting for snippets.

## Features

- **Sign in with Discord** — OAuth2, no local passwords; the team already lives on Discord.
- **Team dashboard** — upcoming events, open tasks, and recent results at a glance.
- **Calendar & availability** — matches, tournaments, trainings, and pickup games; players mark their availability and see the roster's at a glance.
- **Coach-assigned tasks** — coach/manager assigns action items to players, with status tracking.
- **Match results & demos** — scrim/match/tournament results, grouped by tournament or league season, with post-match notes and a demo link. Attach the `.dem` itself and the score and map are read straight out of it, round by round, so halftime and overtime side swaps are handled on their own.
- **Per-map nade library** — organized smoke/flash/molotov lineups with embedded video clips, plus an optional pin on the map radar for each lineup.
- **Map starting positions** — a per-map radar board where the coach drags each player's pin onto their CT/T spot, and the team reads the setup at a glance.
- **Training materials** — a categorized library of learning resources.
- **Opponent scouting** — a profile per opponent that merges scouting notes, the head-to-head record (overall and per map), match history and upcoming games; the next match gets a "prepare" banner on the dashboard.
- **Map pool** — every map with the coach's status (comfort pick / playable / learning / ban), the team's record and win rate on it, recent form, and a shortcut to that map's tactics; filterable by scrims, league or tournament games.
- **Veto assistant** — suggests picks and bans against a given opponent from the map pool, the team's overall and head-to-head record, tactic coverage and the opponent's recorded veto habits; every score comes with plain-language reasons. The actual veto is recorded per match event.
- **FACEIT opponent report** — paste a FACEIT team link, match room link or nickname list and get a "them vs us" page: TL;DR insights, a per-map matrix (their team games and win rate vs ours, smoothed advantage with a confidence level), the opponent's likely bans/picks, a simulated BO1/BO3 veto with reasons for every step, players to watch, recent form and individual form (per-player ELO, per-map K/D/ADR/WR split team vs solo, form arrows, and "map comfort" from solo games blended into the predictions with shrinkage). Our players are matched automatically by SteamID64; data is cached and refreshed in the background before scheduled matches.
- **Opponent demo tendencies** — upload several of the opponent's demos at once (or pull their latest FACEIT team games automatically when a Downloads API token is configured); the opponent's team is recognised by SteamID64, and every map gets a "how they play" card: T-side site and timing split, entry arrows and standard grenade clusters, CT default setups ("2A-1M-2B"), stacks, AWP spots, early aggression, retake vs save, key players — each with sample size, confidence and anti-strat suggestions. Grenade clusters can be saved as an analysis board or added to the nade library as "theirs". Any round of an analysed opponent demo can be replayed in 2D.
- **2D round replay & tactic matching** — every round of a match with a demo replays on the radar (per-second positions smoothly interpolated on a canvas, kill feed, grenades with their lifetimes, bomb, 1x/2x/4x, keyboard shortcuts) and can be snapshotted into an analysis board in one click. Our rounds are automatically matched to the Playbook tactics (player positions and grenade landings from the opening seconds vs tactic points), so each round shows the tactic it was played with and every tactic shows its real win rate ("5/8 rounds").
- **Game plan per event** — the coach writes the plan for a match or training and attaches tactics and analysis boards from the libraries; players open everything from one place (the event, or the dashboard's next-match banner) with deep links straight into each tactic or board.
- **Player & team stats** — individual performance (K/D, ADR, HS%, rating), a win-rate trend chart over time, and one-click stat import straight from a CS2 demo file.
- **Discord notifications** — new events/tasks and pre-event reminders posted to a team webhook.
- **Live updates** — SignalR pushes changes (availability, tasks, results, ...) to every connected client, no polling.
- **Background jobs with live progress** — demo parsing, FACEIT syncs and demo downloads run on an in-process job queue (`System.Threading.Channels` + a hosted worker with bounded concurrency); the API answers `202 { jobId }` and the UI shows a progress bar driven by SignalR (bytes-read progress for demos), so nobody waits on a blocking request. A FACEIT demo's original file name (`1-<uuid>-1-2.dem`) is recognised and pre-fills the opponent, map, date and category.
- **Installable PWA** — add-to-homescreen on mobile, no app store needed.

## Tech stack

| Layer | Stack |
|---|---|
| Backend | ASP.NET Core (.NET 10), Vertical Slice Architecture, MediatR 12 (CQRS), ErrorOr, FluentValidation, DemoFile.Net (CS2 demo parsing) |
| Database | PostgreSQL (EF Core 10) |
| Frontend | React 19, TypeScript, Vite, TailwindCSS 4, TanStack Query, Zustand, React Router, dnd-kit |
| Real-time | SignalR |
| PWA | vite-plugin-pwa — installable, offline-capable for static views |
| Deployment | Single Docker image (backend serves the built frontend) |

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the full layer breakdown and design decisions, [docs/ROADMAP.md](docs/ROADMAP.md) for the delivery plan, and [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md) for how it ships to production.

## Getting started

Requirements: .NET 10 SDK, Node 20+, Docker.

```bash
# database + local S3-compatible storage (SeaweedFS on :9000, bucket "harnashub" created on start) for demo uploads
docker compose up -d

# backend
cd backend
dotnet restore
# optional: FACEIT Data API v4 server key for the opponent report (the app runs without it)
dotnet user-secrets set "Faceit:ApiKey" "<your-key>" --project src/HarnasHub.Api
# optional: FACEIT Downloads API token (granted on application) for automatic opponent demo downloads
dotnet user-secrets set "Faceit:DownloadsApiToken" "<your-token>" --project src/HarnasHub.Api
dotnet run --project src/HarnasHub.Api

# frontend
cd frontend
npm install
npm run dev

# frontend tests (Vitest + Testing Library, no network or backend needed)
npm test
```

Or build and run the whole app as a single container (see [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md)):

```bash
docker build -t harnashub .
docker run -p 8080:8080 \
  -e ConnectionStrings__Database="..." \
  -e Jwt__Secret="..." \
  -e DiscordOAuth__ClientId="..." \
  -e DiscordOAuth__ClientSecret="..." \
  -e DiscordOAuth__RedirectUri="http://localhost:8080/api/auth/discord/callback" \
  harnashub
```

## Status

All 10 delivery phases are built and tested — auth/roster, calendar & availability, results/nade library/map positions, stats & analysis, PWA/live updates, coach-role and calendar UX polish, tactics library, task-linked materials, roster member removal, and grenade map pins/richer video embeds/tournament-league grouping/automatic demo stat import. See [docs/ROADMAP.md](docs/ROADMAP.md) for the detailed breakdown and what's planned next.
