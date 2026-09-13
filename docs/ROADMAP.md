# Roadmap — HarnasHub

Kolejność wdrażania od MVP do pełnej wersji. Każda faza powinna być używalna sama w sobie (drużyna dostaje wartość po każdej fazie, nie dopiero na końcu).

## Faza 0 — Fundament
- [ ] Scaffold solution .NET (Core / Application / Infrastructure / Api / Tests) + projekt React (Vite + TS + Tailwind)
- [ ] Docker Compose: PostgreSQL lokalnie
- [ ] Auth: rejestracja/zaproszenie do drużyny, logowanie (JWT), role Player/Coach/Manager
- [ ] Roster drużyny (lista graczy, role, podstawowy profil)
- [ ] CI: build + testy na GitHub Actions

## Faza 1 — Organizacja
- [ ] Dashboard (na razie: najbliższe wydarzenie + otwarte zadania, statyczny layout)
- [ ] Kalendarz wydarzeń (Match / Tournament / Training / PickupGame)
- [ ] Dostępność graczy per wydarzenie (klik: dostępny / niepewny / niedostępny) + widok zbiorczy dla coacha
- [ ] Zadania: coach/manager przydziela, zawodnik odhacza status

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
