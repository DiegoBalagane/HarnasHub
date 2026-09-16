# HarnasHub

**A team management platform for HArnasiESport (CS2)** — one place for scheduling, coaching tasks, match results, and team analytics, replacing scattered Discord/Messenger threads.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-18-61DAFB?logo=react&logoColor=black)
![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-EF%20Core-4169E1?logo=postgresql&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-green)

## Overview

HarnasHub is a full-stack web app built for a competitive Counter-Strike 2 team to run its day-to-day operations: who's available for practice, what the coach expects from each player, how the last scrim went, and whether the team is actually improving over time.

## Features

- **Sign in with Discord** — OAuth2, no local passwords; the team already lives on Discord.
- **Team dashboard** — upcoming events, open tasks, and recent results at a glance.
- **Calendar & availability** — matches, tournaments, trainings, and pickup games; players mark their availability and see the roster's at a glance.
- **Coach-assigned tasks** — coach/manager assigns action items to players, with status tracking.
- **Match results & demos** — scrim/match/tournament results with post-match notes and a demo link.
- **Per-map nade library** — organized smoke/flash/molotov lineups with embedded YouTube clips and position notes.
- **Map starting positions** — a per-map radar board where the coach drags each player's pin onto their CT/T spot, and the team reads the setup at a glance.
- **Training materials** — a categorized library of learning resources.
- **Opponent scouting** — notes and materials prepared ahead of a specific match.
- **Player & team stats** — individual performance (K/D, ADR, HS%, rating) and a win-rate trend chart over time.
- **Discord notifications** — new events/tasks and pre-event reminders posted to a team webhook.
- **Live updates** — SignalR pushes changes (availability, tasks, results, ...) to every connected client, no polling.
- **Installable PWA** — add-to-homescreen on mobile, no app store needed.

## Tech stack

| Layer | Stack |
|---|---|
| Backend | ASP.NET Core (.NET 10), Vertical Slice Architecture, MediatR (CQRS), EF Core |
| Database | PostgreSQL |
| Frontend | React 18, TypeScript, Vite, TailwindCSS, TanStack Query, Zustand |
| Real-time | SignalR |
| Deployment | Single Docker image (backend serves the built frontend) |

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the full layer breakdown and design decisions, [docs/ROADMAP.md](docs/ROADMAP.md) for the delivery plan, and [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md) for how it ships to production.

## Getting started

Requirements: .NET 10 SDK, Node 20+, Docker.

```bash
# database
docker compose up -d postgres

# backend
cd backend
dotnet restore
dotnet run --project src/HarnasHub.Api

# frontend
cd frontend
npm install
npm run dev
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

All four MVP phases are built and tested — see [docs/ROADMAP.md](docs/ROADMAP.md) for the detailed breakdown.
