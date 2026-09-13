# Roadmap — HarnasHub

Kolejność wdrażania od MVP do pełnej wersji. Każda faza powinna być używalna sama w sobie (drużyna dostaje wartość po każdej fazie, nie dopiero na końcu).

## Faza 0 — Fundament
- [x] Scaffold solution .NET (Core / Application / Infrastructure / Api / Tests) + projekt React (Vite + TS + Tailwind)
- [x] Docker Compose: PostgreSQL lokalnie (port hosta 5433 — 5432 bywa zajęty przez lokalną instalację Postgresa)
- [x] Auth: rejestracja, logowanie (JWT), role Player/Coach/Manager (nowe konta zawsze startują jako Player)
- [x] Roster drużyny (lista graczy, role) + zmiana roli przez Managera (własnej roli zmienić nie można)
- [x] CI: build + testy na GitHub Actions
- [ ] Zaproszenia do drużyny / promowanie na Coach/Manager (na razie rola zmieniana tylko ręcznie w bazie)

## Faza 1 — Organizacja
- [x] Dashboard (najbliższe wydarzenie + liczba otwartych zadań)
- [x] Kalendarz wydarzeń (Match / Tournament / Training / PickupGame) — tworzenie: Coach/Manager
- [x] Dostępność graczy per wydarzenie (klik: dostępny / niepewny / niedostępny) + widok zbiorczy dla całej drużyny
- [x] Zadania: coach/manager przydziela, zawodnik odhacza status (widok tylko własnych zadań)

Uwaga: rejestracja jest wciąż otwarta (każdy może sobie założyć konto jako Player) — prawdziwe zaproszenia e-mailem to osobna, większa funkcja, poza zakresem MVP.

## Faza 2 — Wyniki i wiedza
- [ ] Wyniki sparingów/meczów/turniejów + notatki pomeczowe
- [ ] Upload/link do demki (storage S3-compatible, nie baza danych)
- [ ] Baza granatów per mapa (smoke/flash/molotov), osadzone wideo YouTube, opis pozycji
- [ ] Materiały treningowe (linki/pliki, kategorie)

## Faza 3 — Rozwój i analiza
- [ ] Statystyki graczy (K/D, ADR, HS%, rating) wpisywane ręcznie per mecz
- [ ] Trend drużynowy w czasie (wygrane mecze, punkty/ranking) — wykresy
- [ ] Analizy przeciwników (notatki + materiały przed konkretnym meczem)

## Faza 4 — Wygoda
- [ ] PWA — instalacja na telefonie, powiadomienia push
- [ ] Webhook powiadomień na Discorda (nowe zadanie, nowe wydarzenie, przypomnienie)
- [ ] SignalR — live update dashboardu/dostępności bez odświeżania

## Rozważane później
- Automatyczne parsowanie demek CS2 (statystyki bez ręcznego wpisywania)
- Integracja z FACEIT/Steam API
- Wewnętrzny ranking/ELO na bazie sparingów
