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
   | `Jwt__ExpiryMinutes` | `60` |
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
3. Bez `RequiredGuildId` appka **działa, ale wpuszcza dowolne konto Discord** jako Playera — dla appki wewnętrznej drużyny to ustaw od razu.

## Jak Ty (i gracze) to przetestujecie po wdrożeniu

1. Otwórz link Railway na telefonie (Chrome na Androidzie / Safari na iOS).
2. Kliknij **„Zaloguj się przez Discord"** — logujesz się kontem Discord, nie tworzysz nowego hasła. Musisz być na serwerze Discord drużyny (patrz `RequiredGuildId` wyżej), inaczej dostaniesz błąd logowania.
3. **Instalacja jako PWA**:
   - Android/Chrome: powinno pojawić się „Dodaj do ekranu głównego" (albo ikonka instalacji w pasku adresu) — zaakceptuj, ikonka HarnasHub wyląduje na ekranie głównym.
   - iOS/Safari: Udostępnij → „Dodaj do ekranu początkowego".
4. Otwórz appkę z ikonki — powinna wystartować na pełnym ekranie, bez paska adresu przeglądarki.
5. Ty jako pierwszy zalogowany użytkownik musisz **ręcznie zmienić sobie rolę na Manager w bazie danych** (Railway → Postgres → Query, albo `psql`), bo pierwsze konto zawsze startuje jako Player, a rolę może zmieniać tylko Manager (i nie może zmienić własnej):
   ```sql
   UPDATE "Users" SET "Role" = 'Manager' WHERE "DiscordId" = 'twoje_discord_id';
   ```
   (Discord ID znajdziesz tak samo jak ID serwera w kroku wyżej — prawym klikiem na swój nick zamiast na serwer). Wyloguj się i zaloguj ponownie, żeby dostać token z nową rolą. Od tego momentu zarządzasz rolami reszty drużyny z poziomu UI (`/roster`).
6. Sprawdź **live-update**: otwórz appkę na dwóch urządzeniach (albo telefon + laptop) zalogowaną jako różni gracze, zmień dostępność na jednym — drugie powinno zaktualizować się samo, bez odświeżania.
7. Sprawdź **Discorda**: dodaj wydarzenie albo zadanie — wiadomość powinna przyjść na skonfigurowany kanał w ciągu kilku sekund.

## Czego świadomie nie ma (i dlaczego)

- **Prawdziwe powiadomienia push** (natywny prompt o zgodę na telefonie) — wymagają VAPID + custom service workera; odłożone do momentu, gdy appka będzie realnie wdrożona i będzie na czym testować prawdziwe zezwolenia przeglądarki na urządzeniu.
- **Upload plików (demek) do własnego storage** — na razie `DemoUrl` to zwykły link (np. do Google Drive). Realny upload do S3-compatible bucketa (Cloudflare R2) to sensowny następny krok, gdy pojawi się potrzeba i konto na taki bucket.
