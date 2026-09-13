# Roadmap — HarnasHub

Kolejność wdrażania od MVP do pełnej wersji. Każda faza powinna być używalna sama w sobie (drużyna dostaje wartość po każdej fazie, nie dopiero na końcu).

## Faza 0 — Fundament
- [x] Scaffold solution .NET (Core / Application / Infrastructure / Api / Tests) + projekt React (Vite + TS + Tailwind)
- [x] Docker Compose: PostgreSQL lokalnie (port hosta 5433 — 5432 bywa zajęty przez lokalną instalację Postgresa)
- [x] Auth: rejestracja, logowanie (JWT), role Player/Coach/Manager (nowe konta zawsze startują jako Player)
- [x] Roster drużyny (lista graczy, role) + zmiana roli przez Managera (własnej roli zmienić nie można)
- [x] CI: build + testy na GitHub Actions

## Faza 1 — Organizacja
- [x] Dashboard (najbliższe wydarzenie + liczba otwartych zadań)
- [x] Kalendarz wydarzeń (Match / Tournament / Training / PickupGame) — tworzenie: Coach/Manager
- [x] Dostępność graczy per wydarzenie (klik: dostępny / niepewny / niedostępny) + widok zbiorczy dla całej drużyny
- [x] Zadania: coach/manager przydziela, zawodnik odhacza status (widok tylko własnych zadań)

Uwaga: rejestracja jest wciąż otwarta (każdy może sobie założyć konto jako Player) — prawdziwe zaproszenia e-mailem to osobna, większa funkcja, poza zakresem MVP.

## Faza 2 — Wyniki i wiedza
- [x] Wyniki sparingów/meczów/turniejów + notatki pomeczowe — dodaje Coach/Manager, widzi cała drużyna
- [x] Demka jako link (nie plik) — bez własnego storage S3 na razie; wymagałoby prawdziwych danych do bucketa, którego nie mamy skonfigurowanego. Realny upload plików to osobny follow-up, gdy pojawi się konto na Cloudflare R2/podobne.
- [x] Baza granatów per mapa (smoke/flash/molotov/frag), osadzone wideo YouTube, opis pozycji — dodaje każdy zalogowany, usuwa autor wpisu lub Coach/Manager
- [x] Materiały treningowe (linki, kategorie) — dodaje Coach/Manager, widzi cała drużyna

## Faza 3 — Rozwój i analiza
- [x] Statystyki graczy (K/D, ADR, HS%, rating) wpisywane ręcznie per mecz — Coach/Manager dodaje, rozwijany panel na liście wyników; jeden wpis na gracza na mecz
- [x] Trend drużynowy w czasie (wygrane mecze, skuteczność %) — liczony bezpośrednio z `MatchResults` (bez osobnej encji punktów/ELO — patrz "Rozważane później"), wykres na `/stats` + kafelek na dashboardzie
- [x] Osobista historia statystyk gracza (średni rating, K/D/A/ADR/HS% per mecz)
- [x] Analizy przeciwników (notatki + link do materiału, filtr po nazwie) — dodaje Coach/Manager, widzi cała drużyna

## Faza 4 — Wygoda
- [ ] PWA — instalacja na telefonie, powiadomienia push
- [ ] Webhook powiadomień na Discorda (nowe zadanie, nowe wydarzenie, przypomnienie)
- [ ] SignalR — live update dashboardu/dostępności bez odświeżania

## Rozważane później
- Automatyczne parsowanie demek CS2 (statystyki bez ręcznego wpisywania)
- Integracja z FACEIT/Steam API
- Wewnętrzny ranking/ELO na bazie sparingów
