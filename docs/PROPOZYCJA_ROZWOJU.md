# Propozycja rozwoju HarnasHub — UI, demki, analiza rywala

Dokument roboczy z propozycją kolejnych etapów. Każdy etap da się wdrożyć i oddać osobno; testy jednostkowe powstają razem z kodem, a pełny build, testy i weryfikacja w przeglądarce (Docker + Chrome) — **raz, po wszystkich etapach**.

## 0. Założenia

- Trener i IGL mają dostawać **gotowe wnioski** („co grają, gdzie mamy przewagę, co banować, czego się spodziewać”), a nie surowe dane do przeglądania mecz po meczu.
- Pliki `.dem` dalej **nie są przechowywane** — po parsowaniu zostaje znormalizowana oś czasu (JSON.gz w S3) + agregaty w PostgreSQL.
- Wszystkie wnioski generowane deterministycznie (reguły + progi, testowalne jednostkowo). Każdy wniosek pokazuje dowód (liczby, próbka) i poziom pewności.
- Istniejące moduły są rozszerzane, nie duplikowane: `VetoScoring`, `GetOpponentProfile`, `Tactics`, `Nades`, `AnalysisBoards`, `GamePlan`.

## 1. Stan obecny (skrót)

| Obszar | Co jest | Ograniczenie |
|---|---|---|
| Parser demek | Suma per gracz (K/D/A, ADR, HS, entry, KAST, multi-kille, utility dmg, flash assisty), zwycięzca rundy + strony, pozycje śmierci | Po parsowaniu zostają tylko sumy — brak osi rund, ekonomii, granatów, pozycji w czasie |
| Przeciwnicy | Bilans H2H, bilans per mapa, notatki, nadchodzące mecze, sugestia veta (`VetoScoring`) | Wiedza tylko z meczów **przeciw nam** — nic o tym, co grają z innymi |
| UI | 14 pozycji menu w 3 grupach | Treść w wąskiej kolumnie (~30% szerokości desktopu), formularze dodawania na stałe na górze stron, nakładające się moduły map |

## 2. Etap 1 — porządki w UI

**Nawigacja (z 14 do 5 pozycji):**

| Nowe menu | Zawiera | Skąd |
|---|---|---|
| Dashboard | widok zależny od roli | bez zmian |
| Kalendarz | wydarzenia na siatce tygodnia, warstwa dostępności, urlopy | scala „Kalendarz” (dostępność) i „Wydarzenia” |
| Mecze | Wyniki → strona meczu, Przeciwnicy → raport rywala, Rozwój | grupa „Mecze” |
| Playbook | wybór mapy → zakładki: Pozycje / Granaty / Taktyki / Tablice / Materiały / Statystyki mapy | 5 osobnych stron strategii + „Mapy” |
| Drużyna | Skład, Zadania, Frekwencja | „Zespół” bez kalendarza |

**Layout i interakcje:**
- Kontener `max-w-6xl` zamiast wąskiej kolumny; radary w pełnym rozmiarze.
- Formularze „Dodaj…” za przyciskiem (modal / panel boczny) — lista jest pierwsza.
- Puste stany z instrukcją, co zrobić („Brak statystyk — zaimportuj demkę w Wynikach”).
- Strona meczu `/results/:id` zamiast rozwijanego wiersza: wynik, tabela graczy, mapa śmierci, (od etapu 3) oś rund.
- Dashboard: trener/IGL — najbliższy mecz + TL;DR raportu rywala, ostatnie wnioski z demki, frekwencja; gracz — moje zadania, dostępność, forma.
- Do sprawdzenia: przy przeglądzie zawiesił się renderer na `/nades` (zrzut ekranu timeout) — profilowanie widoku mapy granatów.

**Rozmiar:** M. **Ryzyko:** niskie (tylko frontend + przekierowania starych ścieżek).

## 3. Wspólny fundament parsera (część etapu 2)

`DemoParseSession` (331 linii) dzielimy na kolektory uruchamiane w **jednym przebiegu** po pliku:

```
IDemoCollector { void Subscribe(CsDemoParser demo); void Contribute(DemoTimelineBuilder builder); }

StatsCollector          — obecne sumy per gracz (bez zmian w wynikach)
RoundCollector          — numer rundy, freeze end, zwycięzca, powód końca, plant/defuse + strefa
GrenadeCollector        — rzut (pozycja, rzucający, strona, czas) + lądowanie/detonacja per granat
EconomyCollector        — (etap 3) wartość ekwipunku i pieniądze na freeze end, typ kupna
KillCollector           — (etap 3) zabójstwa z czasem, pozycjami obu graczy, bronią, trade
PositionSampler         — (etap 5) pozycje wszystkich graczy co ~1 s
```

`IDemoParser.ParseAsync(stream, DemoParseOptions, ct)` — opcje mówią, które kolektory włączyć (import stratu nie potrzebuje ekonomii ani próbkowania pozycji). Wynik: `DemoTimeline` (rundy → zdarzenia) + obecny `DemoParseResult` liczony z niego.

**Granaty — źródła zdarzeń:** encja pocisku (`CBaseCSGrenadeProjectile`, utworzenie → pozycja rzutu i rzucający), zdarzenia `smokegrenade_detonate`, `flashbang_detonate`, `hegrenade_detonate`, `inferno_startburn`, `decoy_started` (pozycja lądowania, powiązanie z pociskiem po `entityid`), `player_blind` (czas oślepienia wrogów/swoich). Dokładne nazwy typów w DemoFile.Net 0.44.1 do potwierdzenia kompilacją — tak jak przy pierwszej wersji parsera.

## 4. Etap 2 — import stratu z demki

Odpowiednik opisanej aplikacji GUI („wybierz demkę, rundę i stronę, nazwij strat, wyklucz graczy przed eksportem”), wbudowany w aplikację — wynik ląduje od razu w Taktykach i Granatach, więc bez osobnego `.exe`.

**Przepływ (kreator na stronie Taktyk / Playbook → „Importuj z demki”):**
1. Upload demki — istniejący `PresignDemoUpload` → S3.
2. `ExtractDemoNades` (nowy slice): parsuje z `GrenadeCollector` + `RoundCollector`, zwraca **wszystkie rundy naraz** (wybór rundy/strony bez ponownego parsowania), usuwa plik z S3.
3. Wybór rundy (numer, wynik po rundzie, zwycięzca, typ kupna gdy dostępny) i strony T/CT.
4. Podgląd na radarze: linie rzut → lądowanie, ikony wg typu granatu, oś czasu rundy.
5. Lista pogrupowana po graczach, checkbox na graczu (wyklucza wszystkie jego granaty) i na pojedynczym granacie.
6. Nazwa stratu, ekonomia, notatka → zapis.

**Zapis (`ImportTacticFromDemo`):**
- `Tactic` (mapa, strona, nazwa, ekonomia) + `TacticPoint` w punktach lądowania; opis punktu: „Smoke · nick · 0:14”.
- Opcja „Dodaj granaty do biblioteki” → `NadeEntry` z pinem lądowania, powiązany przez `TacticPoint.NadeEntryId`; duplikat (ten sam typ, lądowanie w promieniu ~2% radaru) jest linkowany zamiast tworzony drugi raz.
- Migracja: `NadeEntry.ThrowX/ThrowY` (nullable) — pozycja rzutu do rysowania linii.

**Rozmiar:** M. **Zależność:** fundament parsera (rozdz. 3).

## 5. Etap 3 — oś czasu meczu i ekonomia

- Każda analizowana demka zapisuje `DemoTimeline` jako `matches/{matchResultId}/timeline.json.gz` w S3 (~100–500 KB) + encję `MatchDemoAnalysis` (klucz obiektu, wersja parsera, data).
- Na rundę: typ kupna obu drużyn (pistol / eco / force / full wg wartości ekwipunku), zabójstwa z czasem, plant + strefa (A/B), powód końca.
- **Strefy map:** wielokąty callout'ów (A, B, Mid, spawn) per mapa w ułamkach radaru — plik konfiguracyjny w repo; edytor do poprawek w Playbooku później. Potrzebne też w etapie 5 (tendencje rywala).
- Strona meczu → zakładki **Przegląd** (automatyczne wnioski: „0/4 force buyów”, „62% otwarć przegranych na CT B”) i **Rundy** (oś czasu z ikonami kupna i wyniku).
- `DemoParseSession` znika, zastąpiony kolektorami.

**Rozmiar:** L.

## 6. Etap 4 — głębsza analiza meczu

- Trade'y (zabójstwo zabójcy w ≤ 5 s), clutche 1vX, mapa otwierających duelów T/CT.
- Granaty z meczu porównane z biblioteką („rzucacie 3 z 9 trenowanych smoków na Mirage A”), oślepienia własnej drużyny.
- Agregaty wielomeczowe per mapa w Playbooku → zakładka Statystyki mapy (WR T/CT, wejścia na bombsite, skuteczność taktyk).

**Rozmiar:** M.

## 7. Etap 5 — Raport rywala

### 7.1 Faza A — FACEIT: oni vs my (bez demek)

**Cel:** jedna strona z porównaniem „co grają oni, co gramy my, gdzie mamy przewagę, co banować”.

**Integracja:**
- `IFaceitClient` (Application/Abstractions) + `FaceitClient` (Infrastructure, typed `HttpClient`, `FaceitOptions.ApiKey` z user-secrets/env). Darmowy klucz Data API v4 — bez wniosku.
- **Nasi gracze** mapowani automatycznie po `User.SteamId64` (wyszukanie gracza FACEIT po identyfikatorze gry) — zero ręcznej konfiguracji.
- **Rywal**: trener wkleja link do drużyny FACEIT, do pokoju meczowego albo listę nicków → zapis jako `OpponentFaceitLink` (klucz przeciwnika jak w `OpponentNames.ToKey`, lista `FaceitPlayerId`, opcjonalnie `FaceitTeamId`).
- **Cache** w bazie: `FaceitPlayer`, `FaceitMatch` (id, data, mapa, typ rozgrywek, wynik, składy), `FaceitMatchPlayerStat`. Ostatnie ~50 meczów / 120 dni na gracza. Odświeżanie: `FaceitSyncService` (BackgroundService, jak `EventReminderService`) dla rywali z zaplanowanym meczem w ciągu 7 dni + przycisk „Odśwież” (z limitem częstotliwości).

**Mecz drużynowy vs solo:** mecz liczy się jako „drużynowy”, gdy ≥ 3 graczy z listy grało po tej samej stronie. Statystyki drużyny liczone tylko z takich meczów; mecze solo zasilają profil komfortu graczy (osobna sekcja, mniejsza waga).

**Metryki per mapa (osobno dla nich i dla nas):**
- liczba meczów drużynowych, WR, średnia różnica rund, ostatnio grana, trend (ostatnie 5 vs wcześniejsze),
- udział mapy w ich meczach („komfort”),
- per gracz: mecze, K/D, ADR na mapie (z meczów + segmentów per mapa w statystykach gracza).
- Dla nas dodatkowo nasze wewnętrzne wyniki z HarnasHub (`MatchResults`) i status w `MapPool` — FACEIT nie widzi scrimów, LAN-ów ani innych platform.

**Przewaga na mapie:**
- WR z wygładzeniem do 50% przy małej próbce: `wr' = (wins + k·0,5) / (games + k)`, `k = 5` → 2/2 wygrane to ~64%, nie 100%.
- `Przewaga = wr'(my) − wr'(oni)`; poziom pewności z min(liczba meczów): < 3 niska, 3–9 średnia, ≥ 10 wysoka.
- Wynik trafia jako dodatkowe wejście do istniejącego `VetoScoring` (obok statusu w puli, H2H, liczby taktyk, ich dotychczasowych pick/ban z `EventVetoStep`).

**Przewidywane veto rywala** (API nie podaje kolejności banów — tylko zagraną mapę):
- mapa z puli, której nie grają (0–1 mecz z ostatnich N) → prawdopodobny perma-ban,
- najczęściej grana + wysoki WR → prawdopodobny pick / ostatni ban z naszej strony,
- symulacja formatu (BO1: naprzemienne bany do jednej mapy; BO3: ban-ban-pick-pick-ban-ban-decider) → rekomendowana kolejność naszych ruchów z uzasadnieniem każdego kroku.

**Strona `/opponents/:key/report`:**
1. **TL;DR (3–5 punktów)** — np. „Grają głównie Mirage i Ancient (68% meczów)”, „Nie grają Nuke — prawie pewny ban”, „Słabi na Inferno (38%), my 70% → pick”, „Groźny: X (AWP, ADR 92 na Mirage)”.
2. **Macierz map** — wiersz na mapę: ich mecze / WR | nasze mecze / WR | przewaga (kolor) | pewność | przewidywanie (ban / pick / neutral).
3. **Rekomendowane veto** krok po kroku z uzasadnieniem.
4. **Gracze do pilnowania** — top 2–3 per mapa, rola (AWP/entry z multi-kill/entry stats, gdy dostępne).
5. **Forma** — ostatnie 10 meczów drużynowych, seria, zmiany składu (nowi gracze w ostatnich meczach).
6. **Akcje:** „Utwórz plan meczu” (wypełnia `GamePlan` i `Veto` wydarzenia), „Dodaj notatkę”.

**Generowanie:** `GetOpponentReport` → `OpponentReportDto` z listą `Insight { Kind, Severity, Text, Evidence }` budowaną przez czysty `OpponentInsightRules` (testy jednostkowe na każdą regułę). Raport zapisywany jako `OpponentReportSnapshot` (JSON + data danych), żeby cała drużyna widziała ten sam stan i było widać, z kiedy są dane.

**Rozmiar:** L. **Zależność:** brak — można robić równolegle z etapami 2–4.

### 7.2 Faza B — demki rywala: jak grają

**Źródła demek:**
1. Ręczny upload wielu plików naraz (ich mecze z FACEIT, nasze mecze przeciw nim) — dostępne od razu.
2. FACEIT Downloads API — automatyczne pobranie demek z ostatnich N meczów drużynowych na wybranych mapach. **Wymaga wniosku do FACEIT (odpowiedź do ~30 dni) — warto złożyć już teraz.**

**Rozpoznanie strony rywala bez klikania:** gracze FACEIT mają SteamID64 → parser wie, którzy gracze w demce to rywal (ta sama logika większości co `DemoScoreCalculator`).

**Model:** `OpponentDemoAnalysis` (przeciwnik, mapa, data meczu, źródło, klucz timeline w S3) + agregaty liczone z osi czasu (etap 3) — więc faza B wymaga etapu 3 i stref map.

**Co wyciągamy (per mapa, z N demek / M rund):**

| Strona | Tendencja | Jak liczona |
|---|---|---|
| T | rozkład wejść A / B / Mid, egzekucje szybkie vs późne | strefa pierwszego kontaktu i plantu, czas od freeze end |
| T | standardowe granaty przy egzekucjach | klasteryzacja punktów lądowania (promień ~2% radaru), częstość w rundach |
| T | pistolówka, zachowanie po przegranej pistolówce (eco / force) | typ kupna rund 1–3 i 13–15 |
| CT | domyślne ustawienie („2A-1M-2B”), stack'i | strefy graczy w 20. sekundzie rundy, klasteryzacja |
| CT | pozycje AWP, agresywne wyjścia (early picks) | pozycja i czas pierwszych zabójstw CT |
| CT | retake vs save | zachowanie po plancie |
| Gracze | entry, AWPer, clutcher | udział w otwarciach, broń, 1vX |

**Prezentacja:** w raporcie rywala na karcie każdej mapy sekcja „Tendencje (z N demek)” z pewnością, mini-radarem (heatmapa ustawień CT, strzałki wejść T, klastry smoków) i regułami anty-strat, np. „71% rund T idą B po 1:10 → rozważ stack B lub agresję na aps po 1:00”. Klastry granatów rywala można jednym kliknięciem zapisać jako tablicę w `AnalysisBoards` albo jako „ich granaty” w bibliotece.

**Rozmiar:** L. **Zależność:** etap 3 (timeline + strefy), opcjonalnie wniosek Downloads API.

## 8. Etap 6 — zaawansowane

- Odtwarzacz 2D rundy (pozycje co 1 s, `PositionSampler`) na naszym radarze z kalibracją z `MapCalibration`.
- „Utwórz tablicę analizy z tej rundy” — zrzut pozycji/granatów do `AnalysisBoards`.
- Automatyczne dopasowanie naszych rund do taktyk z Playbooka (pozycje i granaty z pierwszych sekund vs `TacticPoint`) → skuteczność każdej taktyki.
- Uzupełnienie kalibracji radarów Dust2/Inferno/Nuke (`RadarImageCrops`) — potrzebna demka z każdej mapy.

## 9. Kolejność i zależności

```
Etap 1 (UI) ─────────────────────────────────────────────┐
Etap 2 (fundament parsera + import stratu) → Etap 3 → Etap 4 → Etap 5B → Etap 6
Etap 5A (FACEIT) ──────────── niezależny ───────────────→ Etap 5B
Wniosek FACEIT Downloads API — złożyć od razu (czas oczekiwania)
```

Rekomendowana kolejność: **1 → 2 → 5A → 3 → 4 → 5B → 6**. 5A daje trenerowi najwięcej wartości najszybciej (raport przed meczem bez żadnych demek), a 5B naturalnie korzysta z osi czasu z etapu 3.

## 10. Ryzyka

| Ryzyko | Mitigacja |
|---|---|
| Limity FACEIT API | cache w bazie, odświeżanie w tle tylko dla rywali z bliskim meczem, limit na ręczne „Odśwież” |
| Mała próbka meczów drużynowych rywala | wygładzanie WR, poziom pewności przy każdym wniosku, osobna sekcja „komfort graczy” z meczów solo |
| Rywal gra poza FACEIT (ESEA, LAN, scrimy) | faza B z ręcznym uploadem, notatki, nasze wewnętrzne wyniki |
| Zmiana puli map CS2 | `MapName` + kalibracja + strefy jako dane per mapa; nieznana mapa = brak pozycji, reszta działa |
| Zmiany API DemoFile.Net przy aktualizacji gry | wersja parsera zapisywana przy analizie, kolektory testowane na plikach przykładowych |
| Rozmiar osi czasu w S3 | gzip, próbkowanie pozycji tylko gdy włączone (etap 6) |

## 11. Testy (po wszystkich etapach)

- Testy jednostkowe pisane razem z każdym handlerem/walidatorem/regułą (`OpponentInsightRules`, wygładzanie WR, symulacja veta, klasteryzacja, wykrywanie meczu drużynowego, `ImportTacticFromDemo`), fake `IFaceitClient` i `IDemoParser`.
- Po zakończeniu wszystkich etapów: jeden pełny build + `dotnet test` + `vitest`, `dotnet format --verify-no-changes`, migracje na Postgresie z Docker Compose, przejście scenariuszy w Chrome (import stratu, raport rywala, strona meczu) i zrzut ekranu jako dowód.
