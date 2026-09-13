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

- **Vertical Slices** zamiast klasycznych warstw Service/Repository — każdy use case (`Features/Calendar/CreateEvent/`, `Features/Tasks/AssignTask/`) ma własny `Command`/`Query`, `Validator` (FluentValidation), `Handler` (MediatR, przypięty na `12.5.0` — wersje 13+ wymagają płatnej licencji, patrz [CLAUDE.md](../CLAUDE.md)).
- **Wynik operacji**: `ErrorOr<T>` jako typ zwracany z handlerów zamiast rzucania wyjątków dla oczekiwanych błędów biznesowych (np. "termin już zajęty", "nie możesz zmienić własnej roli").
- **EF Core + PostgreSQL** — encje (`HarnasHub.Core/Entities`):
  - `User` (rola: Player / Coach / Manager, zmieniana tylko przez Managera, nie na sobie samym)
  - `Event` (Match / Tournament / Training / PickupGame) → `Availability` (per user, per event, unikalny indeks na parze)
  - `TaskItem` → przypisany do `User`, status Todo/Done
  - `MatchResult` — wynik + opcjonalny link do demki (`DemoUrl`, zwykły string, nie plik)
  - `NadeEntry` — per mapa (`MapName` jako string, nie enum — pula map w CS2 się rotuje), typ granatu, link YouTube
  - `TrainingMaterial` — link + kategoria
- **Autoryzacja**: JWT (bez refresh tokenów na razie), `RequireRole` na poziomie endpointu dla akcji Coach/Manager-only; tam gdzie trzeba sprawdzić "właściciel zasobu LUB Coach/Manager" (np. usuwanie granatu), logika jedzie w handlerze przez `ICurrentUserService.Role`.
- **Storage plików**: demka CS2 bywają duże (100–300 MB) — świadomie NIE wdrożono własnego uploadu do S3-compatible storage w tej fazie (brak realnych danych dostępowych do bucketa do przetestowania). `MatchResult.DemoUrl` to zwykły link do zewnętrznie hostowanego pliku (Drive, itp.); prawdziwy upload z presigned URL to follow-up, gdy pojawi się konto np. na Cloudflare R2.

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
