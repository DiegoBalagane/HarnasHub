# Architektura — HarnasHub

## Przegląd

Monorepo: backend .NET oddzielony od frontendu React, komunikacja przez REST API + SignalR (live update dashboardu i dostępności).

```
HarnasHub/
├── backend/                    # ASP.NET Core Web API (.NET 10)
│   ├── src/
│   │   ├── HarnasHub.Core/            # encje, enumy, domain events — bez zewnętrznych zależności
│   │   ├── HarnasHub.Application/     # logika biznesowa jako Vertical Slices (Features/{Domain}/{Action}/), CQRS przez MediatR
│   │   ├── HarnasHub.Infrastructure/  # EF Core, PostgreSQL, storage plików, SignalR, Redis (cache)
│   │   └── HarnasHub.Api/             # Minimal API endpoints, auth (JWT), DI, konfiguracja
│   └── tests/
│       └── HarnasHub.Tests/          # mirror struktury src/
├── frontend/                   # React 18 + TypeScript + Vite
│   ├── src/
│   │   ├── features/            # dashboard, calendar, tasks, results, nades, stats, opponents...
│   │   ├── components/          # współdzielone komponenty UI (layout, common)
│   │   └── app/                 # routing, layout, providery
├── docs/
│   ├── ARCHITECTURE.md          # ten plik
│   ├── ROADMAP.md               # plan wdrażania
│   └── SETUP.md                 # (do uzupełnienia) instrukcja uruchomienia lokalnego
├── docker-compose.yml           # postgres + backend + frontend do lokalnego dev
└── .github/workflows/           # CI: build + testy
```

Warstwy zależą do wewnątrz: `Api`/`Infrastructure` → `Application` → `Core`. `Core` nigdy nie referencuje wyższych warstw; jeśli niższa warstwa potrzebuje czegoś z wyższej, definiuje interfejs u siebie, a implementacja żyje wyżej (inversion of control).

## Backend — .NET 10 / ASP.NET Core

- **Vertical Slices** zamiast klasycznych warstw Service/Repository — każdy use case (`Features/Calendar/CreateEvent/`, `Features/Tasks/AssignTask/`) ma własny `Command`/`Query`, `Validator` (FluentValidation), `Handler` (MediatR).
- **Wynik operacji**: `ErrorOr<T>` jako typ zwracany z handlerów zamiast rzucania wyjątków dla oczekiwanych błędów biznesowych (np. "termin już zajęty").
- **EF Core + PostgreSQL** — kluczowe relacje:
  - `User` → `TeamMember` (rola: Player / Coach / Manager)
  - `Event` (Match / Tournament / Training / PickupGame) → `Availability` (per user, per event)
  - `Event` → `Result` → `DemoFile` (referencja do pliku w storage, nie w bazie)
  - `Task` → przypisany `User`, status
  - `Map` → `NadeEntry` (typ granatu, opis pozycji, link YouTube, screenshot)
  - `MatchStat` (per gracz, per mecz) → agregacja do trendu w czasie
- **Auth**: JWT + refresh tokeny, role-based authorization (Player/Coach/Manager/Admin).
- **SignalR** — hub do live update dostępności i dashboardu bez odświeżania strony.
- **Storage plików**: demka CS2 bywają duże (100–300 MB) — nie trzymać w repo/DB. Lokalnie wolumin Docker, docelowo S3-compatible (Cloudflare R2 — darmowy egress).

## Frontend — React + TypeScript

- **Vite** + **TailwindCSS**, komponenty w stylu shadcn/ui.
- **React Query** do stanu serwera (cache, refetch, dedupe requestów), **Zustand** do stanu UI/lokalnego.
- **vite-plugin-pwa** — instalacja na telefonie (ikonka na ekranie głównym), cache dla widoków statycznych (np. baza granatów) działający offline.
- Struktura per-feature: `features/<name>/{components,hooks,stores}`, żeby nie mieszać widoków domenowych ze współdzielonym UI.
- Kalendarz dostępności jako siatka tydzień × zawodnik z kolorami (dostępny / niepewny / niedostępny).

## Sugerowany hosting (tani/darmowy start)

| Element | Propozycja |
|---|---|
| Baza danych | Neon / Supabase (PostgreSQL managed, darmowy tier) |
| Backend | Railway / Render (kontener .NET) |
| Frontend | Vercel / Netlify |
| Pliki (demka, materiały) | Cloudflare R2 |

## Rozważane później (nie w MVP)

- Parsowanie demek CS2 (np. biblioteka `DemoFile` dla C#) do automatycznego wyciągania statystyk zamiast ręcznego wpisywania.
- Integracja z FACEIT/Steam API do importu statystyk gracza.
- Webhook do Discorda przy nowym zadaniu/wydarzeniu.
- Wewnętrzny ranking/ELO na bazie wyników sparingów.
