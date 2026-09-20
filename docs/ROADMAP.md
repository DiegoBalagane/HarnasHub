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

## Faza 7 — Biblioteka taktyk
- [x] Zapisane taktyki per mapa i strona (CT/TT), z tagiem ekonomii (Eco/Force buy/Full buy/Anti-eco) i dowolną liczbą ponumerowanych punktów na radarze — punkt ma opis i opcjonalny link do konkretnego lineupu z bazy granatów; tworzy/edytuje/usuwa Coach/Manager, cała drużyna ma podgląd
- [x] Edytor zapisuje cały układ pinezek jedną akcją „Zapisz" zamiast osobnego zapytania na każde przeciągnięcie/dodanie/usunięcie punktu

## Faza 8 — Materiały do przeglądu dla zawodników
- [x] Zadanie może mieć doczepiony konkretny materiał z biblioteki (`TaskItem.TrainingMaterialId`) — Coach/Manager wybiera go przy przydzielaniu, zawodnik widzi link bezpośrednio na liście swoich zadań; reużywa całego istniejącego mechanizmu Zadań (przydział, powiadomienie Discord/SignalR, odznaczanie „Zrobione") zamiast osobnej funkcji

## Faza 9 — Usuwanie nieaktywnego zawodnika
- [x] Manager może trwale usunąć konto gracza z `/roster` — kasuje tylko jego prywatne dane (dostępność, urlopy, pozycja na radarze, dodatkowe role, zadania mu przypisane); wyniki meczów, granaty, taktyki i zadania które przydzielił innym zostają, bez utraty dorobku drużyny
- [x] Panel staty meczu pokazuje wiersz usuniętego gracza jako „Usunięty zawodnik" zamiast po cichu go ukrywać

## Faza 10 — Mapa granatów, wideo, statystyki meczowe
- [x] Pinezki granatów na radarze mapy (10.1)
- [x] Bogatszy embed wideo lineupów, facade + `youtube-nocookie.com` (10.2)
- [x] Sparing/Liga/Turniej z grupowaniem wyników (10.3)
- [x] Automatyczny import staty z demek `.dem` (10.4)

### 10.1 Pinezki granatów na radarze mapy — ✅ zaimplementowane
`NadeEntry` ma teraz `LandingX`/`LandingY` (float?, [0,1], migracja `AddNadeLandingPosition`, nullable więc stare wpisy bez pozycji nadal działają — pokazują się tylko na liście). Nowy slice `Features/Nades/UpdateNadePosition` (`PATCH /api/nades/{id}/position`, `X`/`Y` oboje null lub oboje w [0,1] — pilnuje `UpdateNadePositionCommandValidator`), autoryzacja jak przy usuwaniu: autor wpisu lub Coach/Manager (`UpdateNadePositionHandler`), 5 testów jednostkowych.

Frontend: `/nades` ma teraz przełącznik Lista/Mapa (`NadesPage`). Widok mapy (`NadeMapView`) — wybór mapy z listy, radar z pinezkami kolorowanymi i oznaczonymi literą po `GrenadeType` (D/F/M/G — dymna/flasha/molotov/granat, `NadePin`), filtrowalny checkboxami po typie. Klik w pinezkę otwiera kartę ze szczegółami i wideo (patrz 10.2), przeciągnięcie zmienia pozycję (ten sam wzorzec pointer-capture co `MapRadar`/`TacticEditor`, zapis raz na `pointerup`). Wpisy bez pozycji trafiają do listy "Bez pozycji na mapie" z przyciskiem "Ustaw pozycję" — uzbraja tryb umieszczenia, kolejny klik na radarze zapisuje pinezkę (ten sam "klik pusty radar" wzorzec co `TacticEditor.handleRadarClick`, tylko uzbrajany per wpis zamiast zawsze aktywny). Druga pinezka "stąd rzucasz" (`ThrowFromX`/`Y`) świadomie pominięta na starcie — jeden punkt lądowania wystarcza do znalezienia wpisu na mapie, dwupunktowy tryb rzutu można dodać później bez migracji łamiącej istniejące dane.

### 10.2 Wideo lineupów — format i osadzanie — ✅ zaimplementowane
Zbadane pod kątem konwencji branżowej (csnades.gg, scope.gg, cs2nades.gg): standardem są **krótkie (10–20 s) klipy bez montażu**, pokazujące tylko rzut i efekt. Zdecydowano zostać przy YouTube jako głównym źródle (bez kosztu hostingu, dobrze znane graczom) — Streamable/Medal.tv nadal działają, bo pole akceptuje dowolny link, po prostu bez podglądu dla nierozpoznanego formatu.

Zaimplementowane w `components/YoutubeEmbed.tsx` (współdzielony, używany przez `NadeLibrary` i `NadeMapView`):
- Parser (`parseYoutubeUrl`) wyciąga ID wideo z `watch?v=`, `youtu.be/`, `embed/`, `shorts/`, z opcjonalnym `?t=`/`&start=`. Nierozpoznany link → fallback "Otwórz wideo ↗" zamiast błędu.
- Embed przez **`youtube-nocookie.com`** (privacy-enhanced) zamiast `youtube.com`.
- Facade: miniaturka (`img.youtube.com/vi/ID/hqdefault.jpg`) z przyciskiem play zamiast żywego `<iframe>` — sam iframe (z `loading="lazy"`, `autoplay=1`) montuje się dopiero po kliknięciu, więc lista kilkudziesięciu granatów na mapie nie ładuje tylu playerów naraz.
- Nieużyte pod `MatchResult.DemoUrl` na razie (to link do demki `.dem`, nie VOD-a) — gdyby ktoś zaczął tam wklejać linki do nagrań, komponent jest gotowy do reużycia bez zmian.

### 10.3 Statystyki i wyniki: sparing / liga / turniej, z grupowaniem — ✅ zaimplementowane (bez filtra na `/stats`)
`MatchResult` ma teraz `Category` (`MatchCategory`: Scrimmage/League/Tournament, string w bazie), `TournamentId`/`LeagueId` (loose linki, jak `TacticPoint.NadeEntryId` — bez FK). Migracja `AddTournamentsAndLeagues`; istniejące wiersze sprzed tej funkcji dostały `Category = Scrimmage` jako wartość domyślną (nie pusty string — `HasConversion<string>()` nie potrafiłby go odczytać z powrotem jako enum).

- **`Tournament`** (`Id`, `Name`, `CreatedByUserId`, `CreatedAtUtc`) — realna encja zamiast wolnego tekstu, żeby literówka nie rozbijała grupowania. Slice `Features/Tournaments/{CreateTournament,GetTournaments}`.
- **`League`** (`Id`, `Name`, `Season`, `Type`: `LeagueType` enum Online/Lan/Division1/Division2/Other) — slice `Features/Leagues/{CreateLeague,GetLeagues}`.
- `AddResultCommand`/`AddResultCommandValidator` — `TournamentId` wymagany tylko dla `Category == Tournament`, `LeagueId` tylko dla `Category == League`, w obie strony pilnowane (ustawienie złego pola dla złej kategorii to błąd walidacji). **Uwaga wyniesiona z testu**: `.When()` w FluentValidation domyślnie działa wstecz na cały łańcuch reguł w danym `RuleFor` (`ApplyConditionTo.AllValidators`), więc dwa `.When()` na tym samym `RuleFor` bez jawnego `ApplyConditionTo.CurrentValidator` nadpisywały sobie nawzajem warunek — złapane przez `AddResultCommandValidatorTests`, nie ręcznie.
- `GetResultsHandler` dołącza nazwę turnieju / nazwę+sezon+typ ligi podwójnym `GroupJoin` (wzór z `GetMyTasksHandler`) zamiast osobnych zapytań per wynik.
- Frontend: `AddResultForm` ma select kategorii + warunkowy picker turnieju/ligi z inline tworzeniem nowego wpisu (bez opuszczania formularza). `ResultList` grupuje wyniki: sparingi płasko, turnieje i ligi w sekcjach nazwanych turniejem / sezonem+typem ligi.
- **Pominięte na razie**: filtr kategorii na `/stats` i wykresie trendu drużynowego (`GetTeamTrendQuery`) — nadal liczy wszystko razem. Nie jest to trudne do dodania (analogiczny `MatchCategory?` filtr jak w `GetNadesQuery`), ale to osobna zmiana w innym slice'u niż grupowanie wyników, więc świadomie zostawione poza tym zakresem.

### 10.4 Automatyczny import staty z demek `.dem` — ✅ zaimplementowane
Punkt z "Rozważane później" w poprzednich fazach, teraz zbudowany:

- Biblioteka: **`DemoFile`/`DemoFile.Game.Cs`** (github.com/saul/demofile-net, MIT, v0.44.1 — pakiet **`demofile-net` nie istnieje na NuGet**, właściwa nazwa to `DemoFile`/`DemoFile.Game.Cs`; poprawka względem wcześniejszej wersji tego dokumentu). API zweryfikowane kompilacją, nie tylko dokumentacją — `CCSPlayerController.SteamID`/`PlayerName`, `Source1GameEvents.PlayerDeath/PlayerHurt/RoundEnd`, `DemoFileReader.Create(...).ReadAllAsync(...)` zgadzają się z README za pierwszym razem.
- **Inwersja zależności**: `IDemoParser`/`DemoParseResult`/`DemoPlayerStats` żyją w `Application/Abstractions` (bez referencji do biblioteki), implementacja `DemoFileParser` w `Infrastructure/Demos/` — ten sam wzorzec co `IDiscordNotifier`/`IRealtimeNotifier`. Dzięki temu handler ma testy jednostkowe **bez** prawdziwego pliku `.dem` (fake `IDemoParser` w `TestDemoParser`, 5 testów: liczenie ADR/HS%, dopasowanie po SteamID64, błąd dla uszkodzonej demki, błąd dla 0 rund).
- Endpoint `POST /api/stats/import-demo` (Coach/Manager) — multipart upload, limit 320 MB podniesiony na tym jednym endpoincie przez `IHttpMaxRequestBodySizeFeature` (Kestrel domyślnie ma 30 MB), plik streamowany bezpośrednio do parsera przez `IFormFile.OpenReadStream()` — **nigdy nie trafia do trwałego storage**, parsujemy i odrzucamy bajty, zostają tylko wyliczone staty. To odrębna decyzja od "prawdziwego uploadu do R2" niżej.
- `Features/Stats/ImportStatsFromDemo` — nic nie zapisuje bezpośrednio; zwraca listę `ParsedPlayerStatDto` (K/D/A/ADR/HS%/przybliżony `Rating`), Coach/Manager przegląda i edytuje każdy wiersz w `DemoImportPanel` (frontend), zapis per-wiersz przez **już istniejący** `AddPlayerStat` — żadnego nowego endpointu do zapisu.
- `Rating` to jawnie oznaczone **przybliżenie** (kille/śmierci/asysty na rundę + składowa obrażeń, bez KAST/impact jak w prawdziwym HLTV Rating 2.0) — zawsze edytowalne przed zapisem.
- **`User.SteamId64` jako `string`, nie `long`** — złapane przed frontendem, nie po: SteamID64 (~7,66×10¹⁶) przekracza `Number.MAX_SAFE_INTEGER` (~9×10¹⁵), więc jako JSON-owa liczba tracił(by) precyzję w przeglądarce dokładnie tak samo jak `DiscordId` (dlatego ten też jest stringiem). Ustawiane raz przez gracza w `/settings` (`SteamIdSettings`, ten sam wzorzec co `InGameNickname`/`PinMark` — edytuje wyłącznie właściciel), walidacja: dokładnie 17 cyfr i wartość ≥ najniższego kiedykolwiek wydanego SteamID64. Gracz bez ustawionego SteamID64 pojawia się w podglądzie importu jako "Niedopasowany", Coach ręcznie przypisuje z dropdowna zamiast tracić jego wiersz.

## Propozycje UX (do rozważenia, nie tylko dla powyższego)
- **Zablokowana pierwsza kolumna kalendarza** — `WeeklyCalendar` przewija się w poziomie (`overflow-x-auto`) na wąskich ekranach, a kolumna z nickiem gracza ucieka razem z resztą tabeli, więc po przewinięciu nie widać, czyj to wiersz. `position: sticky; left: 0` na komórce z nickiem (z tłem, żeby nie prześwitywały komórki pod spodem) rozwiązuje to bez zmiany layoutu.
- **Widok kartowy na telefonie** zamiast tabeli 7 kolumn poniżej ok. 640px — dziś `min-w-[900px]` wymusza poziomy scroll na każdym telefonie; dzień-po-dniu lista kart (jak już częściowo robi dashboard) czytałoby się wygodniej niż przewijanie tabeli kciukiem.
- **Baner "nie zadeklarowałeś dostępności"** na dashboardzie, jeśli najbliższe wydarzenie jest za <48h a gracz nie kliknął jeszcze statusu — dziś trzeba wejść w kalendarz, żeby to zauważyć.
- Reszta (ranking/ELO, prawdziwe push, upload demek do R2) — patrz niżej, bez zmian względem poprzednich faz.

## Rozważane później (bez zmian priorytetu)
- Wewnętrzny ranking/ELO na bazie sparingów — liczony z już istniejących `MatchResult`/`PlayerMatchStat`, niezależny od źródła danych
- Prawdziwe powiadomienia push (VAPID + custom service worker) — po realnym wdrożeniu
- Upload plików (demek) do własnego storage (Cloudflare R2) zamiast linków — częściowo pokrywa się z 10.4, ale to osobna decyzja (trwałe przechowywanie demki do pobrania, nie tylko jednorazowy parsing)
