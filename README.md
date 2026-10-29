# AI Platform

Platforma rekrutacyjna zbudowana w architekturze mikroserwisów (.NET 10, PostgreSQL, YARP). Kandydat przesyła CV i aplikuje na oferty pracy, a model językowy analizuje CV oraz ocenia stopień dopasowania kandydata do wymagań oferty.

| Serwis        | Odpowiedzialność                                   | Dostęp z zewnątrz         |
|---------------|----------------------------------------------------|---------------------------|
| `gateway`     | Reverse proxy (YARP), jedyny publiczny punkt wejścia | `http://localhost:8080` |
| `auth`        | Rejestracja, logowanie, wydawanie tokenów JWT      | przez gateway: `/auth/...` |
| `user`        | Profile użytkowników, przesyłanie i analiza CV     | przez gateway: `/user/api/...` |
| `offer`       | Oferty pracy                                       | przez gateway: `/offer/...` |
| `recruitment` | Aplikacje, statusy, oceny (ręczne i automatyczne)  | przez gateway: `/recruitment/api/...` |
| `ai`          | Komunikacja z modelem językowym (analiza CV, scoring) | wyłącznie wewnętrzny |
| `db`          | PostgreSQL 17                                      | `localhost:5433` (postgres / admin123) |

## Wymagania

- Docker Desktop,
- opcjonalnie: [LM Studio](https://lmstudio.ai/) z modelem `qwen2.5-7b-instruct` udostępnionym na porcie `1234`. Jest ono wymagane wyłącznie do analizy CV i oceny AI. W przypadku jego braku pozostałe funkcje platformy działają poprawnie, natomiast CV otrzymuje status `Failed`,

## Uruchomienie

Należy otworzyć PowerShell w katalogu repozytorium, a następnie wykonać polecenie:

```powershell
docker compose up -d --build
```

Pierwsze budowanie obrazów trwa kilka minut. Migracje baz danych są wykonywane automatycznie podczas uruchamiania serwisów. Stan kontenerów oraz ich logi można sprawdzić poleceniami:

```powershell
docker compose ps
docker compose logs -f gateway auth
```

Zatrzymanie platformy (parametr `-v` powoduje dodatkowo usunięcie bazy danych):

```powershell
docker compose down
```


## Testowanie z poziomu konsoli (PowerShell)

Poniższe bloki poleceń należy wykonywać kolejno, w **tym samym oknie** PowerShella, uruchomionym w katalogu repozytorium. Kolejne kroki korzystają ze zmiennych utworzonych w krokach poprzednich (`$rec`, `$offer`, `$cv` itd.). Polecenie `Invoke-RestMethod` automatycznie przekształca odpowiedź JSON w obiekt, dzięki czemu tokeny i identyfikatory nie wymagają ręcznego kopiowania.

**Krok 0. Konfiguracja**

```powershell
$api  = "http://localhost:8080"
$json = "application/json; charset=utf-8"
$id   = Get-Date -Format "HHmmss"     # unikalny sufiks adresów e-mail, umożliwiający wielokrotne uruchamianie testów
```

**Krok 1. Rejestracja rekrutera i kandydata.** Odpowiedź zawiera pole `token` (token JWT ważny przez 60 minut).

```powershell
$body = @{ email = "rekruter$id@example.com"; password = "Password123!"; role = "Recruiter" } | ConvertTo-Json
$rec  = (Invoke-RestMethod "$api/auth/auth/register" -Method Post -ContentType $json -Body $body).token

$body = @{ email = "kandydat$id@example.com"; password = "Password123!"; role = "Applicant" } | ConvertTo-Json
$app  = (Invoke-RestMethod "$api/auth/auth/register" -Method Post -ContentType $json -Body $body).token

$recH = @{ Authorization = "Bearer $rec" }
$appH = @{ Authorization = "Bearer $app" }
```

Ponowne logowanie (np. po wygaśnięciu tokena):

```powershell
$body = @{ email = "kandydat$id@example.com"; password = "Password123!" } | ConvertTo-Json
$app  = (Invoke-RestMethod "$api/auth/auth/login" -Method Post -ContentType $json -Body $body).token
$appH = @{ Authorization = "Bearer $app" }
```

**Krok 2. Pobranie profilu zalogowanego użytkownika**

```powershell
Invoke-RestMethod "$api/user/api/users/me" -Headers $appH
```

**Krok 3. Utworzenie oferty przez rekrutera.** Wartości pola `category`: 0 – Ai, 1 – Backend, 2 – Pm, 3 – FullStack, 4 – Software, 5 – Frontend. Wartości pola `experience`: 0 – Trainee, 1 – Junior, 2 – Middle, 3 – Senior. Data `endDate` musi przypadać w przyszłości, w przeciwnym razie aplikowanie na ofertę nie jest możliwe.

```powershell
$body = @{
    title        = "Senior .NET Developer"
    description  = "Mikroserwisy w .NET z baza PostgreSQL."
    salary       = 20000
    category     = 1
    experience   = 3
    stack        = @("C#", ".NET", "PostgreSQL")
    requirements = @(
        @{ name = "C# / .NET";  description = "Komercyjne doswiadczenie w .NET";  weight = 5; isRequired = $true },
        @{ name = "PostgreSQL"; description = "Projektowanie schematow i zapytan"; weight = 3; isRequired = $false }
    )
    startDate    = (Get-Date).ToUniversalTime().AddHours(1).ToString("o")
    endDate      = (Get-Date).ToUniversalTime().AddDays(30).ToString("o")
    company      = "Test Corp"
} | ConvertTo-Json -Depth 5

$offer = (Invoke-RestMethod "$api/offer/offers" -Method Post -Headers $recH -ContentType $json -Body $body).id

Invoke-RestMethod "$api/offer/offers"            # lista ofert (bez uwierzytelnienia)
Invoke-RestMethod "$api/offer/offers/$offer"     # szczegóły oferty
```

**Krok 4. Przesłanie CV przez kandydata** (format PDF lub DOCX, maksymalnie 5 MB). Do przesłania pliku wykorzystywany jest program `curl.exe`, wbudowany w systemy Windows 10 i 11. Należy użyć pełnej nazwy `curl.exe`, ponieważ w PowerShellu `curl` jest aliasem innego polecenia. Odpowiedź na przesłanie pliku zawiera wyłącznie kod `201`, dlatego identyfikator CV jest pobierany z listy CV użytkownika.

```powershell
curl.exe -i -X POST "$api/user/api/users/me/cvs" -H "Authorization: Bearer $app" -F "file=@postman/fixtures/cv.pdf"

$cv = (Invoke-RestMethod "$api/user/api/users/me/cvs" -Headers $appH)[0].id

# szczegóły i status analizy: Pending -> Processing -> Completed / Failed (wymaga LM Studio)
Invoke-RestMethod "$api/user/api/users/me/cvs/$cv" -Headers $appH
```

**Krok 5. Złożenie aplikacji na ofertę przez kandydata**

```powershell
$body = @{ offerId = $offer; cvId = $cv; coverLetter = "Dzien dobry!" } | ConvertTo-Json
$appl = (Invoke-RestMethod "$api/recruitment/api/applications" -Method Post -Headers $appH -ContentType $json -Body $body).id

Invoke-RestMethod "$api/recruitment/api/applications/me" -Headers $appH
```

**Krok 6. Przegląd aplikacji przez rekrutera.** Pola `aiStatus` i `aiScore` zawierają status oraz wynik oceny AI.

```powershell
Invoke-RestMethod "$api/recruitment/api/offers/$offer/applications" -Headers $recH
Invoke-RestMethod "$api/recruitment/api/applications/$appl" -Headers $recH | ConvertTo-Json -Depth 6
```

Oczekiwanie na zakończenie oceny AI (sprawdzanie co 5 sekund, wymaga LM Studio):

```powershell
do {
    Start-Sleep 5
    $a = Invoke-RestMethod "$api/recruitment/api/applications/$appl" -Headers $recH
    "AI: $($a.aiStatus)  wynik: $($a.aiScore)"
} while ($a.aiStatus -in "Pending", "Processing")
```

**Krok 7. Zmiana statusu, dodanie oceny oraz ponowne zlecenie oceny AI przez rekrutera.** Dostępne statusy: `Submitted`, `InReview`, `Interview`, `Offered`, `Hired`, `Rejected`.

```powershell
$body = @{ status = "InReview"; note = "Ciekawy kandydat" } | ConvertTo-Json
Invoke-RestMethod "$api/recruitment/api/applications/$appl/status" -Method Patch -Headers $recH -ContentType $json -Body $body

$body = @{ score = 80; comment = "Dobre doswiadczenie" } | ConvertTo-Json
Invoke-RestMethod "$api/recruitment/api/applications/$appl/evaluations" -Method Post -Headers $recH -ContentType $json -Body $body

Invoke-RestMethod "$api/recruitment/api/applications/$appl/ai-evaluation" -Method Post -Headers $recH
```

**Krok 8. Wycofanie aplikacji przez kandydata**

```powershell
Invoke-RestMethod "$api/recruitment/api/applications/$appl" -Method Delete -Headers $appH
```

**Obsługa błędów (401, 403, 404, 409).** W przypadku odpowiedzi z kodem błędu polecenie `Invoke-RestMethod` wyświetla komunikat o błędzie zamiast treści odpowiedzi. Jest to zachowanie oczekiwane, np. przy zapytaniu bez tokena. Sam kod odpowiedzi można odczytać w następujący sposób:

```powershell
try { Invoke-RestMethod "$api/user/api/users/me" } catch { $_.Exception.Response.StatusCode.value__ }   # 401
try { Invoke-RestMethod "$api/user/api/users" -Headers $appH } catch { $_.Exception.Response.StatusCode.value__ }   # 403
```

## Pełny zestaw testów (Postman)

Katalog `postman/` zawiera kolekcję obejmującą ponad 60 zapytań wraz z asercjami, w tym przypadki brzegowe oraz scoring AI. Każde uruchomienie kolekcji rejestruje nowych użytkowników, dzięki czemu może ona być uruchamiana wielokrotnie na tej samej bazie danych.

- W aplikacji Postman: **Import** → plik `postman/AI Platform.postman_collection.json` → **Run collection**. Ze względu na względne ścieżki do plików CV należy ustawić katalog roboczy Postmana na katalog repozytorium (Settings → General → Working directory).
- Z poziomu konsoli (Postman CLI), z katalogu repozytorium:

  ```powershell
  postman collection run "postman/AI Platform.postman_collection.json"
  ```

Folder `6. AI scoring` wymaga uruchomionego LM Studio.

## Testowanie z poziomu przeglądarki

Projekt nie posiada interfejsu graficznego, a Swagger jest dostępny wyłącznie w środowisku `Development` (w kontenerach Docker serwisy działają w środowisku `Production`). Bez tokena w przeglądarce dostępne są jedynie publiczne endpointy:

- http://localhost:8080/offer/offers – lista ofert,
- http://localhost:8080/offer/offers/{id} – szczegóły oferty.


## Główne endpointy (przez gateway)

| Metoda | Ścieżka | Uprawnienia |
|--------|---------|-------------|
| POST | `/auth/auth/register`, `/auth/auth/login` | wszyscy |
| GET / PUT | `/user/api/users/me` | zalogowany użytkownik |
| GET | `/user/api/users`, `/user/api/users/{id}` | Recruiter |
| POST / GET | `/user/api/users/me/cvs` | Applicant |
| GET / DELETE | `/user/api/users/me/cvs/{cvId}` | Applicant |
| GET | `/offer/offers`, `/offer/offers/{id}` | wszyscy |
| POST / PUT / DELETE | `/offer/offers`, `/offer/offers/{id}` | Recruiter (autor oferty) |
| POST | `/recruitment/api/applications` | Applicant |
| GET | `/recruitment/api/applications/me` | Applicant |
| DELETE | `/recruitment/api/applications/{id}` (wycofanie) | Applicant |
| GET | `/recruitment/api/offers/{offerId}/applications` | Recruiter |
| GET | `/recruitment/api/applications/{id}` | Recruiter |
| PATCH | `/recruitment/api/applications/{id}/status` | Recruiter |
| POST | `/recruitment/api/applications/{id}/evaluations` | Recruiter |
| POST | `/recruitment/api/applications/{id}/ai-evaluation` | Recruiter |
