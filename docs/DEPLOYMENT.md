# Wdrożenie — HarnasHub

## Model: jeden deploy, jeden URL

Backend (ASP.NET Core) serwuje zbudowany frontend jako pliki statyczne (`wwwroot/`, patrz `Program.cs`) — nie ma osobnego hostingu frontendu. Dla drużyny to jest optymalne: gracz dostaje **jeden link** (np. `https://harnashub.up.railway.app`), otwiera go na telefonie, loguje się i instaluje jako PWA. Zero CORS-owych niespodzianek, jeden certyfikat HTTPS, jedna rzecz do zapamiętania.

Cały ten model jest zbudowany i **przetestowany lokalnie** — `Dockerfile` w root repo buduje frontend (Node) i backend (.NET) w jednym obrazie; kontener uruchomiony lokalnie poprawnie serwował SPA, API i manifest PWA z jednego portu. Logowanie to **Discord OAuth2** (zero haseł) — przetestowane end-to-end na prawdziwym koncie Discord: redirect → zgoda na Discordzie → callback → JWT → dashboard.

## Hosting — rekomendacja: Railway

| Element | Wybór | Dlaczego |
|---|---|---|
| Backend + frontend | Railway (deploy z `Dockerfile`) | Jedna platforma, jeden branch do deployu, automatyczny HTTPS i domena `*.up.railway.app`, prosty custom domain później |
| Baza danych | Railway Postgres (plugin) | Ten sam projekt/dashboard, `DATABASE_URL` wstrzykiwany automatycznie, nie trzeba osobnego konta na Neon/Supabase |
| Pliki (demka, materiały) | Na razie linki zewnętrzne (Drive itp.) — patrz `docs/ARCHITECTURE.md` | Nie wymaga dodatkowej infrastruktury na start |

Render jest równoważną alternatywą (też wspiera deploy z Dockerfile + managed Postgres) — wybór między nimi to głównie kwestia darmowego limitu/UI, mechanika ta sama.

## Krok po kroku (Railway)

1. Railway → **New Project** → **Deploy from GitHub repo** → wskaż `HarnasHub`. Railway wykryje `Dockerfile` w root i zbuduje z niego.
2. **Add a plugin → PostgreSQL** w tym samym projekcie. Railway sam ustawi zmienną `DATABASE_URL`.
3. W ustawieniach serwisu backendu dodaj zmienne środowiskowe (Railway zamienia `:` na `__` w nazwach zmiennych .NET):

   | Zmienna | Wartość |
   |---|---|
   | `ConnectionStrings__Database` | zbuduj z danych Postgresa Railway: `Host=...;Port=...;Database=...;Username=...;Password=...` (Railway pokazuje te dane w zakładce Postgresa) |
   | `Jwt__Secret` | **nowy, losowy sekret min. 32 znaki** — nigdy nie ten z `appsettings.Development.json` |
   | `Jwt__Issuer` | `HarnasHub` |
   | `Jwt__Audience` | `HarnasHub.Client` |
   | `Jwt__ExpiryMinutes` | `43200` (30 dni — tyle samo co lokalnie; krótsza wartość wymusza ponowne logowanie po każdym wygaśnięciu tokenu, co bez odświeżania sesji łatwo pomylić z problemem przy deployu) |
   | `Discord__WebhookUrl` | URL webhooka z Discorda (Ustawienia kanału → Integracje → Webhooks → New Webhook → Copy URL) — powiadomienia o nowych wydarzeniach/zadaniach |
   | `DiscordOAuth__ClientId` | Client ID aplikacji Discord (Developer Portal → OAuth2) |
   | `DiscordOAuth__ClientSecret` | Client Secret tej samej aplikacji — **traktuj jak hasło** |
   | `DiscordOAuth__RedirectUri` | `https://<twoja-domena-railway>/api/auth/discord/callback` — musi być **dokładnie** taki sam jak Redirect URI dodany w Discord Developer Portal |
   | `DiscordOAuth__FrontendCallbackUrl` | zostaw puste — w tym modelu frontend i backend są na jednym originie |
   | `DiscordOAuth__RequiredGuildId` | ID Waszego serwera Discord (patrz niżej, jak go znaleźć) — **bez tego każdy z Discorda mógłby się zalogować** |
   | `Reminders__LookaheadMinutes` | `60` |
   | `Reminders__CheckIntervalSeconds` | `60` |
   | `ASPNETCORE_ENVIRONMENT` | `Production` |

   `Cors__AllowedOrigins` **nie jest potrzebne** w tym modelu — frontend i backend są na tym samym originie.

4. Deploy. Migracje EF Core aplikują się **automatycznie przy starcie** (patrz `Program.cs` — `dbContext.Database.MigrateAsync()`), więc nie trzeba ręcznie odpalać `dotnet ef database update` na produkcji.
5. Railway nada domenę `https://<nazwa>.up.railway.app` — to jest link, który dostają gracze. Custom domena (np. `hub.harnasiesport.pl`) — do dodania później w ustawieniach serwisu, gdy będzie potrzebna.

## Discord OAuth — dokończenie konfiguracji dla produkcji

Aplikację Discord (Client ID/Secret) już masz założoną z testów lokalnych — trzeba tylko:

1. **Discord Developer Portal → Twoja aplikacja → OAuth2 → Redirects** → dodaj **drugi** redirect (obok tego z `localhost`, nie zamiast):
   ```
   https://<twoja-domena-railway>/api/auth/discord/callback
   ```
2. **Znajdź ID swojego serwera Discord** (potrzebne do `DiscordOAuth__RequiredGuildId`): w aplikacji Discord włącz Tryb Dewelopera (Ustawienia użytkownika → Zaawansowane → Tryb dewelopera), potem kliknij prawym na nazwę serwera drużyny na liście serwerów → **Kopiuj identyfikator serwera**.
3. Bez `RequiredGuildId` appka **działa, ale pozwala zalogować się dowolnemu kontu Discord** (jako Gość, bez dostępu do danych) — dla appki wewnętrznej drużyny to ustaw od razu.

## Jak Ty (i gracze) to przetestujecie po wdrożeniu

1. Otwórz link Railway na telefonie (Chrome na Androidzie / Safari na iOS).
2. Kliknij **„Zaloguj się przez Discord"** — logujesz się kontem Discord, nie tworzysz nowego hasła. Musisz być na serwerze Discord drużyny (patrz `RequiredGuildId` wyżej), inaczej dostaniesz błąd logowania.
3. **Instalacja jako PWA**:
   - Android/Chrome: powinno pojawić się „Dodaj do ekranu głównego" (albo ikonka instalacji w pasku adresu) — zaakceptuj, ikonka HarnasHub wyląduje na ekranie głównym.
   - iOS/Safari: Udostępnij → „Dodaj do ekranu początkowego".
4. Otwórz appkę z ikonki — powinna wystartować na pełnym ekranie, bez paska adresu przeglądarki.
5. Ty jako pierwszy zalogowany użytkownik musisz **ręcznie zmienić sobie poziom uprawnień na Manager w bazie danych** (Railway → Postgres → Query, albo `psql`), bo każde nowe konto startuje jako Gość (widzi tylko ekran „Poczekaj na przydzielenie roli"), a uprawnienia może zmieniać tylko Manager (i nie może zmienić własnych):
   ```sql
   UPDATE "Users" SET "AccessLevel" = 'Manager' WHERE "DiscordId" = 'twoje_discord_id';
   ```
   (Discord ID znajdziesz tak samo jak ID serwera w kroku wyżej — prawym klikiem na swój nick zamiast na serwer). Wyloguj się i zaloguj ponownie, żeby dostać token z nową rolą. Od tego momentu zarządzasz rolami reszty drużyny z poziomu UI (`/roster`).
6. Sprawdź **live-update**: otwórz appkę na dwóch urządzeniach (albo telefon + laptop) zalogowaną jako różni gracze, zmień dostępność na jednym — drugie powinno zaktualizować się samo, bez odświeżania.
7. Sprawdź **Discorda**: dodaj wydarzenie albo zadanie — wiadomość powinna przyjść na skonfigurowany kanał w ciągu kilku sekund.

## Środowisko testowe (staging) — osobne od produkcji

Zanim coś z brancha `develop` wyląduje na `main` (czyli na produkcji, którą widzą gracze), warto przetestować to na osobnym, izolowanym środowisku. Railway ma to wbudowane jako **Environments** w ramach tego samego projektu — nie trzeba zakładać drugiego projektu ani konta.

### Jednorazowa konfiguracja

1. W projekcie Railway (dashboard projektu) → dropdown środowiska obok nazwy projektu → **New Environment** → nazwij `develop`. Nowe środowisko startuje **puste** — serwisy się nie kopiują automatycznie.
2. W pustym środowisku `develop` kliknij **„+ New"** → **GitHub Repository** → wskaż repo `HarnasHub`. Railway zdeployuje domyślny branch (`main`) — od razu potem wejdź w ten serwis → **Settings** → sekcja **Source** → zmień **Branch** na `develop`. Od tej pory każdy push/merge do `develop` odpala deploy tylko tutaj, produkcji nie rusza.
3. Tam samo „+ New" → **Database** → **PostgreSQL** — osobna, pusta baza tylko dla stagingu. **Nigdy nie współdziel bazy z produkcją.**
4. W serwisie backendu → **Settings** → **Networking** → **Generate Domain** — Railway nada osobny publiczny URL (np. `harnashub-develop-xxxx.up.railway.app`), inny niż produkcyjny.
5. W serwisie backendu → zakładka **Variables** → **Raw Editor** (wklejanie wielu zmiennych naraz) — te same klucze co w tabeli produkcyjnej wyżej, ale z dwiema różnicami:
   - `ConnectionStrings__Database` odwołuje się do **stagingowego** Postgresa przez składnię referencji Railway (samo podstawia wartość, nie trzeba kopiować ręcznie):
     ```
     Host=${{<nazwa-serwisu-postgres>.PGHOST}};Port=${{<nazwa-serwisu-postgres>.PGPORT}};Database=${{<nazwa-serwisu-postgres>.PGDATABASE}};Username=${{<nazwa-serwisu-postgres>.PGUSER}};Password=${{<nazwa-serwisu-postgres>.PGPASSWORD}}
     ```
   - `Jwt__Secret` — **inny, osobny sekret niż produkcyjny** (żeby token ze stagingu nie działał na produkcji i odwrotnie), np. `openssl rand -base64 32` lokalnie.
   - `DiscordOAuth__RedirectUri` wskazuje na domenę z kroku 4, nie na produkcyjną.
   - `ASPNETCORE_ENVIRONMENT` = `Staging` (zamiast `Production`) — czysto informacyjne, ułatwia odróżnienie w logach.
   - Resztę (`DiscordOAuth__ClientId`/`ClientSecret`/`RequiredGuildId`, `Discord__WebhookUrl`) można zostawić takie same jak na produkcji, **ale rozważ osobny kanał/webhook na Discordzie dla powiadomień testowych** — inaczej każdy test zaśmieca prawdziwy kanał drużyny. Osobna aplikacja Discord (inny Client ID/Secret) też jest opcją, jeśli wolisz pełną izolację.
6. **Discord Developer Portal → Twoja aplikacja → OAuth2 → Redirects** → dodaj **trzeci** redirect (obok `localhost` i produkcji):
   ```
   https://<domena-stagingu>/api/auth/discord/callback
   ```
7. Deploy. Migracje aplikują się automatycznie na starcie, tak samo jak na produkcji — nowa baza dostaje pełen, aktualny schemat.
8. Pierwsze logowanie na stagingu ląduje jako Gość (nowa, pusta baza) — trzeba ręcznie nadać sobie Managera w **stagingowym** Postgresie, dokładnie tak samo jak w kroku 5 sekcji „Jak Ty (i gracze) to przetestujecie" wyżej, ale w nowej bazie.

### Workflow docelowy

Feature branch → merge do `develop` → auto-deploy na staging → testujesz sam na domenie stagingu → jak OK, merge `develop` → `main` → auto-deploy na produkcję.

### Koszt

Drugi komplet serwisów (backend + Postgres) działający równolegle liczy się do tego samego miesięcznego limitu Hobby planu Railway — realnie ~2× koszt przy stałym działaniu obu środowisk. Jeśli to problem, serwis stagingu można ręcznie zatrzymywać między sesjami testowymi (Railway → serwis → „Sleep"/pauza), zamiast trzymać go włączonego cały czas.

## Czego świadomie nie ma (i dlaczego)

- **Prawdziwe powiadomienia push** (natywny prompt o zgodę na telefonie) — wymagają VAPID + custom service workera; odłożone do momentu, gdy appka będzie realnie wdrożona i będzie na czym testować prawdziwe zezwolenia przeglądarki na urządzeniu.
- **Upload plików (demek) do własnego storage** — na razie `DemoUrl` to zwykły link (np. do Google Drive). Realny upload do S3-compatible bucketa (Cloudflare R2) to sensowny następny krok, gdy pojawi się potrzeba i konto na taki bucket.
