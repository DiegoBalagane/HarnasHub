# Architektura — HarnasHub

## Przegląd

Monorepo: backend .NET oddzielony od frontendu React, komunikacja przez REST API + SignalR (live update dashboardu i dostępności).

```
HarnasHub/
├── backend/                    # ASP.NET Core Web API (.NET 10)
│   ├── src/
│   │   ├── HarnasHub.Core/            # encje, enumy, opcje konfiguracji — bez zewnętrznych zależności
│   │   ├── HarnasHub.Application/     # logika biznesowa jako Vertical Slices (Features/{Domain}/{Action}/), CQRS przez MediatR
│   │   ├── HarnasHub.Infrastructure/  # EF Core, PostgreSQL, powiadomienia (Discord, SignalR), EventReminderService
│   │   └── HarnasHub.Api/             # Minimal API endpoints, auth (JWT), SignalR Hub, DI, konfiguracja, serwowanie wwwroot/
│   └── tests/
│       └── HarnasHub.Tests/          # mirror struktury src/
├── frontend/                   # React 18 + TypeScript + Vite
│   ├── src/
│   │   ├── features/            # dashboard, calendar, availability, tasks, results, nades, map-strategy, stats, opponents...
│   │   ├── components/          # współdzielone komponenty UI (layout, common)
│   │   └── app/                 # routing, layout, providery, RealtimeSync (SignalR)
├── docs/
│   ├── ARCHITECTURE.md          # ten plik
│   ├── ROADMAP.md               # plan wdrażania
│   └── DEPLOYMENT.md            # jak i gdzie wdrożyć, jak przetestować po wdrożeniu
├── Dockerfile                   # multi-stage build: frontend (Node) + backend (.NET) → jeden obraz
├── docker-compose.yml           # postgres do lokalnego dev
└── .github/workflows/           # CI: build + testy
```

Warstwy zależą do wewnątrz: `Api`/`Infrastructure` → `Application` → `Core`. `Core` nigdy nie referencuje wyższych warstw; jeśli niższa warstwa potrzebuje czegoś z wyższej, definiuje interfejs u siebie, a implementacja żyje wyżej (inversion of control).

## Backend — .NET 10 / ASP.NET Core

- **Vertical Slices** zamiast klasycznych warstw Service/Repository — każdy use case (`Features/Calendar/CreateEvent/`, `Features/Tasks/AssignTask/`) ma własny `Command`/`Query`, `Validator` (FluentValidation), `Handler` (MediatR, przypięty na `12.5.0` — wersje 13+ wymagają płatnej licencji, patrz [CLAUDE.md](../CLAUDE.md)).
- **Wynik operacji**: `ErrorOr<T>` jako typ zwracany z handlerów zamiast rzucania wyjątków dla oczekiwanych błędów biznesowych (np. "termin już zajęty", "nie możesz zmienić własnej roli").
- **EF Core + PostgreSQL** — encje (`HarnasHub.Core/Entities`):
  - `User` (`DiscordId`, `AvatarUrl`, rola: Guest / Player / Coach / Manager, zmieniana tylko przez Managera, nie na sobie samym) — bez hasła, tożsamość wyłącznie z Discorda. `Role` leci do bazy jako string (`HasConversion<string>`), więc dorzucenie nowej wartości enuma nie wymaga migracji
  - `User.TeamRole` (IGL / EntryFragger / Support / AWPer / Lurker / Rifler, nullable) — rola **w grze**, osobna od `Role` (poziom uprawnień); ustawia ją Coach lub Manager (`PATCH /api/roster/{userId}/team-role`)
  - `User.InGameNickname` (nullable, 2–32 znaki) — własny nick gracza, edytowany wyłącznie przez samego zainteresowanego (`PATCH /api/roster/me/nickname`, bez parametru `userId` w ścieżce, żeby nie dało się podmienić celu). Nie ma go w JWT, więc frontend dociąga go z listy składu i trzyma w `useAuthStore`; w UI nick ma pierwszeństwo przed `DisplayName` z Discorda
  - `Event` (Match / Tournament / Training / PickupGame) → `Availability` (per user, per event, unikalny indeks na parze)
  - `PlayerAvailabilityDay` — dostępność dzienna niezależna od wydarzeń (Available / PartiallyAvailable z zakresem godzin / Off), unikalny indeks `(UserId, Date)`
  - `Vacation` — zakres dni wolnych gracza; w widoku tygodniowym nadpisuje dostępność dzienną na `Off` z flagą `IsVacation`
  - Logikę "efektywnego statusu dnia" (urlop > deklaracja > `NotSet`) trzyma jedna klasa `Features/Availability/Shared/EffectiveAvailabilityCalculator` — używa jej zarówno widok tygodniowy, jak i dashboard (dziś/jutro), żeby nie rozjechały się interpretacje urlopu
  - `TaskItem` → przypisany do `User`, status Todo/Done
  - `MatchResult` — wynik + opcjonalny link do demki (`DemoUrl`, zwykły string, nie plik)
  - `NadeEntry` — per mapa (`MapName` jako enum: Dust2 / Mirage / Inferno / Nuke / Ancient / Anubis — aktualna pula 2026, w bazie trzymany jako string przez `HasConversion<string>`), typ granatu, link YouTube. Rotacja puli map = nowa wartość enuma + migracja normalizująca stare wpisy (wzór: `ConvertNadeMapNameToEnum`)
  - `MapPositionAssignment` — pozycja startowa jednego gracza na danej mapie i stronie (`MapSide`: CT / T, w bazie jako string). `X`/`Y` to **ułamki [0,1] względem radaru**, nie piksele — dzięki temu pinezki są niezależne od rozmiaru obrazu i skalują się z kontenerem. Unikalny indeks `(MapName, Side, UserId)` = jeden gracz ma jedno miejsce na mapie/stronie, a slice `Features/MapStrategy/SetPlayerPosition` robi po tym kluczu upsert (wzór jak `SetAvailability`)
  - `TrainingMaterial` — link + kategoria
- **Autoryzacja**: logowanie wyłącznie przez **Discord OAuth2** (`/api/auth/discord/login` → redirect do Discorda → `/api/auth/discord/callback` wymienia kod na profil przez `IDiscordOAuthClient`, znajduje-lub-tworzy `User` po `DiscordId`, wystawia JWT i przekierowuje do frontendu z tokenem we fragmencie URL `#token=`). Świadoma decyzja: zero haseł, zero przechowywanych danych logowania — tożsamość i zgoda żyją tylko po stronie Discorda. Scope `identify guilds` + `DiscordOAuth:RequiredGuildId` — jeśli ustawiony, logowanie sprawdza (`GET /users/@me/guilds`) czy dana osoba faktycznie jest na serwerze Discord drużyny i odrzuca każdego innego; bez tego dowolne konto Discord mogłoby wejść do środka. **Każde nowe konto startuje jako `Guest`** — jest zalogowane, ale nie widzi żadnych danych drużyny, dopóki Manager nie nada mu roli przez ekran Skład (`PATCH /api/roster/{userId}/role`). Ponieważ nikt nie może zmienić własnej roli, pierwszy Manager musi zostać ustawiony ręcznie w bazie (patrz [DEPLOYMENT.md](DEPLOYMENT.md)).
  - Egzekwowanie: polityka `AuthorizationPolicies.TeamMember` (`RequireRole("Player", "Coach", "Manager")`, rejestrowana w `Program.cs`) wisi na **każdej** grupie `MapGroup(...)` z danymi drużyny oraz na `TeamHub` — Gość dostaje 403, nie 401. Dodatkowe `RequireRole("Coach", "Manager")` / `RequireRole("Manager")` na pojedynczych endpointach zostają bez zmian (są podzbiorem `TeamMember`); tam gdzie trzeba sprawdzić "właściciel zasobu LUB Coach/Manager" (np. usuwanie granatu), logika jedzie w handlerze przez `ICurrentUserService.Role`.
  - Frontend lustrzanie: `ProtectedRoute` dla roli `Guest` renderuje `PendingAccessPage` zamiast żądanej strony, a `Layout` chowa całą nawigację (zostaje avatar, komunikat "konto oczekuje na przydzielenie roli" i Wyloguj).
  - **Długość sesji**: JWT ważny `Jwt:ExpiryMinutes` = **43200 minut (30 dni)**. Świadomie bez refresh tokenów — to wewnętrzne narzędzie jednej drużyny, a długi token wymienia dodatkową infrastrukturę na brak logowania co godzinę. Po faktycznym wygaśnięciu `apiClient` łapie 401, czyści sesję (`useAuthStore.clearSession()`) i przerzuca na `/login`.
- **Storage plików**: demka CS2 bywają duże (100–300 MB) — świadomie NIE wdrożono własnego uploadu do S3-compatible storage w tej fazie (brak realnych danych dostępowych do bucketa do przetestowania). `MatchResult.DemoUrl` to zwykły link do zewnętrznie hostowanego pliku (Drive, itp.); prawdziwy upload z presigned URL to follow-up, gdy pojawi się konto np. na Cloudflare R2.
- **Live-update**: `IRealtimeNotifier` (Application) → `SignalRRealtimeNotifier` (Api, `IHubContext<TeamHub>`) — handlery po zapisie wołają `NotifyAsync("<topic>")`, `TeamHub` broadcastuje do wszystkich połączonych klientów; frontend mapuje topic na klucz TanStack Query i robi `invalidateQueries` zamiast odpytywać w pętli. JWT do huba idzie przez query string (`?access_token=`), bo przeglądarkowe transporty SignalR nie potrafią ustawić nagłówka `Authorization`.
- **Discord**: `IDiscordNotifier` (Application) → `DiscordWebhookNotifier` (Infrastructure, zwykły `HttpClient` POST na webhook URL z `appsettings`). Pusty URL = cichy no-op, nic się nie wysyła — bezpieczny domyślny stan bez konfiguracji.
- **Przypomnienia**: `EventReminderService` (Infrastructure, `BackgroundService`) co `Reminders:CheckIntervalSeconds` sprawdza wydarzenia startujące w oknie `Reminders:LookaheadMinutes`, wysyła jedno przypomnienie na Discorda i oznacza `Event.ReminderSentAtUtc`, żeby nie wysłać drugi raz.
- **Wdrożenie**: `Program.cs` na starcie sam aplikuje migracje (`Database.MigrateAsync()`) i serwuje `wwwroot/` jako SPA fallback — jeden kontener = cały produkt. Szczegóły i checklista testowa: [DEPLOYMENT.md](DEPLOYMENT.md).

## Frontend — React + TypeScript

- **Vite** + **TailwindCSS**, komponenty w stylu shadcn/ui.
- **React Query** do stanu serwera (cache, refetch, dedupe requestów), **Zustand** do stanu UI/lokalnego.
- **vite-plugin-pwa** — instalacja na telefonie (prawdziwe ikony w `public/`, wygenerowane bez zewnętrznych narzędzi), cache dla widoków statycznych działający offline. Manifest i service worker działają tylko w buildzie produkcyjnym (`vite build`/`preview`), nie w `vite dev` — tak działa plugin domyślnie.
- **`@microsoft/signalr`** — `RealtimeSync` (w `App.tsx`) trzyma jedno połączenie do `/hubs/team` dopóki użytkownik jest zalogowany; odbiera topic i wywołuje `queryClient.invalidateQueries`.
- Struktura per-feature: `features/<name>/{components,hooks,stores}`, żeby nie mieszać widoków domenowych ze współdzielonym UI.
- Kalendarz dostępności jako siatka tydzień × zawodnik z kolorami (dostępny / częściowo dostępny z zakresem godzin / off / urlop) — `features/availability/` na stronie `/calendar`: nawigacja po tygodniach, edycja wyłącznie własnego wiersza, pod każdą kolumną dnia lista nieobecnych, nad kolumną znacznik wydarzenia z `useUpcomingEvents()`. Urlopy (`VacationForm`/`VacationList`) nadpisują status dnia po stronie backendu. Live-update przez topic `availability-week` → invalidacja klucza `['availability','week']`.
- Pozycje startowe (`features/map-strategy/` na stronie `/map-strategy`) — radar wybranej mapy (`public/maps/<mapa>.webp`, nazwa pliku = lowercase wartość enuma `MapName`) w kontenerze `position: relative`, na nim pinezki graczy pozycjonowane procentowo z `X`/`Y`. Coach/Manager przeciąga pinezkę (pointer events z `setPointerCapture`, zapis **raz**, na `pointerup` — nie na każdym `pointermove`), zwykły gracz widzi tylko tooltip z nickiem i rolą w grze. Live-update przez topic `map-strategy`.
- Dashboard (`features/dashboard/`) nad trzema kafelkami (najbliższe wydarzenie / otwarte zadania / skuteczność) pokazuje sekcje „Dzisiaj" i „Jutro" — `DailyStatusCard` renderuje wydarzenie danego dnia i listę graczy podzieloną na grających i nieobecnych, reużywając `DayStatusBadge` z `features/availability`. Dane idą jednym zapytaniem `GET /api/dashboard` (pola `today`/`tomorrow`).

## Hosting

Jeden deploy, jeden URL — backend serwuje zbudowany frontend. Pełen plan, konfiguracja i checklista testowa: [DEPLOYMENT.md](DEPLOYMENT.md).

## Rozważane później (nie w MVP)

- Parsowanie demek CS2 (np. biblioteka `DemoFile` dla C#) do automatycznego wyciągania statystyk zamiast ręcznego wpisywania.
- Integracja z FACEIT/Steam API do importu statystyk gracza.
- Wewnętrzny ranking/ELO na bazie wyników sparingów.
- Prawdziwe powiadomienia push (VAPID + custom service worker) — po realnym wdrożeniu, gdy jest na czym testować zgodę przeglądarki.
- Upload plików (demek) do własnego storage (Cloudflare R2) zamiast linków zewnętrznych.
