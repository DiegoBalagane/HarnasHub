# Roadmap — HarnasHub

Kolejność wdrażania od MVP do pełnej wersji. Każda faza powinna być używalna sama w sobie (drużyna dostaje wartość po każdej fazie, nie dopiero na końcu).

## Faza 0 — Fundament
- [x] Scaffold solution .NET (Core / Application / Infrastructure / Api / Tests) + projekt React (Vite + TS + Tailwind)
- [x] Docker Compose: PostgreSQL lokalnie (port hosta 5433 — 5432 bywa zajęty przez lokalną instalację Postgresa)
- [x] Auth: logowanie przez **Discord OAuth2** (bez haseł, bez przechowywania danych logowania), poziomy uprawnień Guest/Player/Manager (nowe konta zawsze startują jako Guest, bez dostępu do danych drużyny; od Fazy 6 „Trener" to osobna flaga `IsCoach`, niezależna od poziomu uprawnień) — z weryfikacją członkostwa w serwerze Discord drużyny (`DiscordOAuth:RequiredGuildId`), przetestowane na prawdziwym koncie
- [x] Roster drużyny (lista graczy, role) + zmiana roli przez Managera (własnej roli zmienić nie można)
- [x] Rola w drużynie (IGL / Entry fragger / Support / AWPer / Lurker / Rifler) ustawiana przez Coacha/Managera + własny nick w grze, który każdy zmienia tylko sobie (wyświetlany zamiast nazwy z Discorda)
- [x] CI: build + testy na GitHub Actions

## Faza 1 — Organizacja
- [x] Dashboard (najbliższe wydarzenie + liczba otwartych zadań + sekcje „Dzisiaj"/„Jutro": kto gra, kto ma urlop, wydarzenie danego dnia)
- [x] Kalendarz wydarzeń (Match / Tournament / Training / PickupGame) — tworzenie: Coach/Manager
- [x] Dostępność graczy per wydarzenie (klik: dostępny / niepewny / niedostępny) + widok zbiorczy dla całej drużyny
- [x] Zadania: coach/manager przydziela, zawodnik odhacza status (widok tylko własnych zadań)

Uwaga: każdy z serwera Discord drużyny może się zalogować, ale dostaje konto jako **Gość** — widzi tylko ekran „Poczekaj na przydzielenie roli", a wszystkie grupy API z danymi drużyny odpowiadają mu 403. Dostęp otwiera dopiero Manager, nadając rolę w `/roster`. To zastępuje wcześniejsze podejście „każdy zalogowany od razu jest Playerem" i pełni funkcję kroku zaproszenia bez dodatkowej infrastruktury.

## Faza 2 — Wyniki i wiedza
- [x] Wyniki sparingów/meczów/turniejów + notatki pomeczowe — dodaje Coach/Manager, widzi cała drużyna
- [x] Demka jako link (nie plik) — bez własnego storage S3 na razie; wymagałoby prawdziwych danych do bucketa, którego nie mamy skonfigurowanego. Realny upload plików to osobny follow-up, gdy pojawi się konto na Cloudflare R2/podobne.
- [x] Baza granatów per mapa (smoke/flash/molotov/frag), osadzone wideo YouTube, opis pozycji — dodaje każdy zalogowany, usuwa autor wpisu lub Coach/Manager
- [x] Materiały treningowe (linki, kategorie) — dodaje Coach/Manager, widzi cała drużyna
- [x] Pozycje startowe na mapie — radar mapy z przeciąganymi pinezkami graczy per strona (CT/T), ustawia Coach/Manager, reszta drużyny tylko podgląd

## Faza 3 — Rozwój i analiza
- [x] Statystyki graczy (K/D, ADR, HS%, rating) wpisywane ręcznie per mecz — Coach/Manager dodaje, rozwijany panel na liście wyników; jeden wpis na gracza na mecz
- [x] Trend drużynowy w czasie (wygrane mecze, skuteczność %) — liczony bezpośrednio z `MatchResults` (bez osobnej encji punktów/ELO — patrz "Rozważane później"), wykres na `/stats` + kafelek na dashboardzie
- [x] Osobista historia statystyk gracza (średni rating, K/D/A/ADR/HS% per mecz)
- [x] Analizy przeciwników (notatki + link do materiału, filtr po nazwie) — dodaje Coach/Manager, widzi cała drużyna

## Faza 4 — Wygoda
- [x] PWA — instalowalna (prawdziwe ikony, manifest, service worker), działa offline dla statycznych widoków. Bez prawdziwych powiadomień push (VAPID + zgoda przeglądarki na urządzeniu) — świadomie odłożone do realnego wdrożenia, patrz `docs/DEPLOYMENT.md`
- [x] Webhook na Discorda — nowe wydarzenie, nowe zadanie, automatyczne przypomnienie przed startem wydarzenia (`EventReminderService`, jednorazowo per wydarzenie)
- [x] SignalR — live-update kalendarza/dostępności/zadań/wyników/granatów/pozycji na mapie/materiałów/przeciwników/rosteru bez odświeżania, przetestowane na dwóch kartach jednocześnie
- [x] Pojedynczy deploy (backend serwuje zbudowany frontend) + `Dockerfile`, zweryfikowany lokalnie end-to-end (build obrazu, kontener, rejestracja→JWT→chroniony endpoint, SPA fallback, manifest PWA)
- [x] Plan wdrożenia i testowania na produkcji — `docs/DEPLOYMENT.md`

## Faza 5 — Zgłoszenia zawodników (doszlifowanie)
- [x] Sesja odświeża się automatycznie (start apki + focus okna) — awans z Gościa działa bez ponownego logowania na innym urządzeniu; `Jwt:Secret` puste = backend nie startuje
- [x] Kalendarz: autosave statusu dnia (bez domyślnie zaznaczonej opcji), blokada edycji dni z przeszłości + zakładka „Historia", podświetlony własny wiersz, licznik dostępnych i wspólne okno godzinowe pod każdym dniem
- [x] Urlop można edytować w miejscu, nie tylko usunąć i dodać od nowa
- [x] Sekcja `/settings` (koło zębate): ustawienie/zmiana/**usunięcie** własnego nicku; nick własny wszędzie zamiast nazwy z Discorda (też w kalendarzu, RSVP eventu, pinach na mapie)
- [x] Skład: `RosterSlot` (Main/Bench/StandIn, main ograniczony do 5, StandIn poza kalendarzem, main sortowany nad ławką), przeciąganie kart między kolumnami, role dodatkowe w grze (multi-select obok roli głównej), własny kolor pinezki na radarze dla main składu, notatki „co robić" przy pozycjach na mapie

## Faza 6 — Porządki UX składu i kalendarza
- [x] Rola Trenera oddzielona od poziomu uprawnień: `User.IsCoach` (bool) niezależne od `AccessLevel` (Guest/Player/Manager) — Zawodnik lub Zarządca może być Trenerem naraz; JWT niesie oba jako osobne role-claimy, istniejące `RequireRole("Coach","Manager")` działają bez zmian
- [x] Skład: kolor pinezki wyniesiony z wiersza gracza do osobnego panelu nad tabelą (nie ściska już nicku do zera szerokości), jeden popover na rolę główną + role dodatkowe zamiast dwóch osobnych selectów, tooltipy z opisem poziomów uprawnień i roli Trenera, kolumna Main w `RosterBoard` wygasza się po osiągnięciu 5 graczy
- [x] Kalendarz: kreska między sekcjami Main/Ławka/Trener (Trener zawsze na dole, we własnej sekcji), gracze bez slotu i bez roli Trenera znikają z siatki, mocniej podświetlony własny wiersz, klik w dzień domyślnie ustawia „Cały dzień" (checkbox, odznaczenie pokazuje zakres godzin zamiast osobnego przycisku „częściowo dostępny")
- [x] Nowy typ wydarzenia „Sparing" (`EventType.Scrim`), osobny od „Meczu"
- [x] Radar mapy Cache dodany do `public/maps/` (pula map kompletna: Dust2/Mirage/Inferno/Nuke/Ancient/Anubis/Cache)

## Rozważane później
- Automatyczne parsowanie demek CS2 (statystyki bez ręcznego wpisywania)
- Integracja z FACEIT/Steam API
- Wewnętrzny ranking/ELO na bazie sparingów
- Prawdziwe powiadomienia push (VAPID + custom service worker) — po realnym wdrożeniu
- Upload plików (demek) do własnego storage (Cloudflare R2) zamiast linków
