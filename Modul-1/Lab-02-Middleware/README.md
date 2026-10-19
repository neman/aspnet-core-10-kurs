# Lab 02 — HTTP middleware u CourseApi (ASP.NET Core 10)

**Vreme:** 11:12–11:47 (35 minuta) · **Rad:** individualno ili u paru · **Početni kod:** `CourseApi/` u ovom folderu

## Cilj

Napraviti middleware koji piše logove pre i posle obrade HTTP zahteva, registrovati ga preko extension metode i objasniti kako se pipeline izvršava. Sačuvati rad postojećih servisa i endpoint-a.

## Početno stanje

Starter već sadrži `OrderService`, izbor kanala Email/Sms preko `Notifications:Channel`, `ILogger<OrderService>` i DI lifetime primer. Postoje endpoint-i `GET /health`, `GET /orders/confirm`, `GET /orders/{id:int}/confirm`, `GET /di/lifetime` i `GET /pipeline-demo`.

**Ne pravite novi projekat.** Ne morate završiti prvi DI lab da biste započeli ovaj.

## Pokretanje

U folderu starter projekta:

```bash
cd Modul-1/Lab-02-Middleware/CourseApi
dotnet restore
dotnet build
dotnet run
```

Ili otvori projekat u Visual Studiu i pokreni ga sa **F5**. Oba načina koriste profil iz `Properties/launchSettings.json`, pa je adresa ista: `http://localhost:5100`. Zahteve za proveru imaš i u `CourseApi/CourseApi.http`.

U drugom terminalu pozovite:

```bash
curl -i http://localhost:5100/health
```

Na Windows PowerShell-u koristite `curl.exe` ako je `curl` alias za `Invoke-WebRequest`.

## Zadatak A — Middleware i extension metoda (oko 16 min)

1. U zasebnom fajlu napravite `RequestLoggingMiddleware` klasu.
2. Kroz konstruktor primite `RequestDelegate` i `ILogger<RequestLoggingMiddleware>`.
3. U `InvokeAsync(HttpContext context)` zabeležite putanju zahteva u strukturisanom logu pre poziva sledeće komponente.
4. Pozovite `await _next(context)`.
5. Nakon toga zabeležite HTTP status odgovora.
6. Napravite extension metodu `UseRequestLogging(this IApplicationBuilder app)` koja koristi `UseMiddleware<RequestLoggingMiddleware>()`.
7. Registrujte middleware **jednom** u `Program.cs`, pre izvršavanja endpoint-a.

**Kriterijum uspeha:** `GET /orders/confirm` vraća 200, a konzola sadrži približno `Before /orders/confirm` i `After 200`.

## Zadatak B — Redosled izvršavanja (oko 10 min)

1. Posle `app.UseRequestLogging()` dodajte drugi, **inline** middleware.
2. Njegove poruke nazovite `Inner before` i `Inner after`; između njih pozovite `next`.
3. **Pre pokretanja** napišite redosled četiri middleware poruke.
4. Pozovite `GET /orders/confirm`, proverite konzolu i uporedite rezultat sa predviđanjem.

**Kriterijum uspeha:** možete da objasnite zašto se poruka `Inner after` pojavljuje pre spoljašnje `After 200` poruke. Logovi iz `OrderService` mogu se pojaviti između njih.

**Napomena:** linije `Email: ...` i `SMS: ...` ispisuje `Console.WriteLine`, koji piše odmah, dok logger piše preko pozadinskog reda. Zato se u konzoli mogu pojaviti na drugom mestu nego što očekujete. Poredite samo log poruke middleware-a.

## Zadatak C — Verifikacija (oko 6 min)

```bash
curl -i http://localhost:5100/orders/confirm
curl -i http://localhost:5100/di/lifetime
curl -i http://localhost:5100/health
```

Proverite: oba ključna endpoint-a rade, scoped `InstanceMarker` daje `Same: true` unutar zahteva, logovi imaju očekivan redosled i status 200 za uspešan zahtev.

## Ako zapnete

- `UseRequestLogging` nije dostupna: proverite namespace i `using` u `Program.cs`.
- Nema logova: proverite poziciju `UseRequestLogging()` i da li ste restartovali aplikaciju.
- Build ne prolazi: proverite potpis metode `InvokeAsync` i da li se poziva `_next(context)`.
- Različiti GUID-ovi sami po sebi nisu greška. Bitno je polje `Same` i odabrani lifetime.

Referentno rešenje biće objavljeno u ovom folderu kao `Resenje/` posle lab-a. Niko ne zavisi od završetka ove vežbe da bi započeo Modul 2.

## Opciono (12:00–13:00)

- **A — Short-circuit:** middleware sam odgovara na `GET /blocked` sa 403 i ne prosleđuje zahtev dalje.
- **B — Jedna vrednost iz konfiguracije:** `GET /notifications/channel` vraća samo aktivni kanal.
- **C — Trajanje zahteva:** `RequestLoggingMiddleware` meri trajanje sa `Stopwatch` i loguje `{ElapsedMs}`.
- **D — Serilog:** logovi idu u konzolu i u tekstualni fajl.

Za svaki zadatak ispod postoje koraci, provera i referentno rešenje. Izaberite jedan i uradite ga do kraja. **Početno stanje** za sve: vaš `CourseApi` iz ovog lab-a ili referentno rešenje iz `Resenje/`.

**Svi dodatni zadaci su opcioni.** Obavezna nastava završava se u 12:00; predavač ne mora da bude prisutan do 13:00. Sutra svi koriste zasebni `HeroRegistry` starter.

## Opciono A — Short-circuit: middleware koji ne poziva `next` (oko 10 min)

**Cilj:** videti šta se dešava kada middleware sam napiše odgovor. Do sada je svaki middleware pozivao `await next(context)` i tako predavao zahtev sledećoj komponenti. Ako `next` ne pozove, zahtev se tu završava: komponente iza njega i endpoint se ne izvršavaju. To se zove *short-circuit*. Na isti način rade `UseStaticFiles` (vrati fajl) i `UseAuthorization` (vrati 401/403).

U .NET Frameworku isto radi `HttpModule` koji u `BeginRequest` napiše odgovor i pozove `HttpApplication.CompleteRequest()`.

### Koraci

1. **Pre koda** predvidite: koje će se od četiri poruke (`Before`, `Inner before`, `Inner after`, `After`) pojaviti za `GET /blocked`? Endpoint `/blocked` ne postoji. Koji status biste dobili bez novog middleware-a?
2. U `Program.cs` dodajte inline middleware (`app.Use`) **posle** `app.UseRequestLogging()`, a **pre** `Inner` middleware-a iz zadatka B.
3. Ako je `context.Request.Path` jednak `/blocked`: postavite `context.Response.StatusCode` na `StatusCodes.Status403Forbidden`, upišite tekst `Blocked by middleware` sa `context.Response.WriteAsync(...)` i izađite sa `return`, **bez** poziva `next`.
4. Za sve ostale putanje pozovite `await next(context)`.
5. Restartujte aplikaciju i pozovite:

   ```bash
   curl -i http://localhost:5100/blocked
   curl -i http://localhost:5100/orders/confirm
   ```

### Provera

- `/blocked` vraća **403** i telo `Blocked by middleware`, a ne 404.
- U konzoli su samo `Before /blocked` i `After 403`; `Inner before` i `Inner after` se ne pojavljuju.
- `/orders/confirm` i dalje vraća 200 sa sve četiri poruke, kao pre.

### Pitanja za razmišljanje

1. Zašto se `After 403` ipak pojavljuje, iako niko posle novog middleware-a nije pozvan?
2. Šta biste videli u konzoli da ste novi middleware stavili **ispred** `app.UseRequestLogging()`?

<details>
<summary>Referentno rešenje (otvorite tek kad završite ili zapnete)</summary>

```csharp
app.UseRequestLogging();

// Short-circuit: /blocked never reaches the inner middleware or an endpoint.
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/blocked")
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync("Blocked by middleware");
        return;
    }

    await next(context);
});

// Inner middleware from Task B stays here.
```

**Odgovori na pitanja:**

1. `RequestLoggingMiddleware` je pozvao `await _next(context)`, a taj poziv se normalno vratio, samo ranije. Posle povratka on loguje status koji je novi middleware upisao.
2. Ništa od naših poruka. `RequestLoggingMiddleware` bi bio iza komponente koja prekida zahtev, pa se ne bi ni izvršio.

</details>

## Opciono B — Jedna vrednost iz konfiguracije (oko 10 min)

**Cilj:** pročitati vrednost iz konfiguracije u endpoint-u i videti da je ista vrednost različita u Development i Production okruženju, bez promene koda.

### Koraci

1. U `Program.cs` dodajte endpoint `GET /notifications/channel`.
2. Neka endpoint kao parametar primi `IConfiguration`; DI ga ubacuje kao i `OrderService`.
3. Vratite JSON sa **samo jednim** poljem: `Channel`, vrednost ključa `Notifications:Channel`.
4. Pokrenite aplikaciju na uobičajen način (F5 ili `dotnet run`, okruženje Development) i pozovite:

   ```bash
   curl -i http://localhost:5100/notifications/channel
   ```

5. Zaustavite aplikaciju i pokrenite je bez launch profila, čime okruženje postaje Production:

   ```bash
   dotnet run --no-launch-profile --urls http://localhost:5100
   ```

   Ponovo pozovite isti URL.

### Provera

- Development: `{"channel":"Sms"}` (vrednost iz `appsettings.Development.json`).
- Production: `{"channel":"Email"}` (vrednost iz `appsettings.json`).
- U konzoli na početku piše `Hosting environment: Development`, odnosno `Production`.

### Pitanja za razmišljanje

1. Zašto ne vratiti celu konfiguraciju, na primer sve parove ključ–vrednost?
2. U PowerShell-u postavite `$env:Notifications__Channel = "Push"` i pokrenite aplikaciju iz **istog** terminala. Šta vraća endpoint i zašto?

<details>
<summary>Referentno rešenje (otvorite tek kad završite ili zapnete)</summary>

```csharp
// Return a single, harmless configuration value.
app.MapGet("/notifications/channel", (IConfiguration config) =>
    Results.Ok(new { Channel = config["Notifications:Channel"] }));
```

**Odgovori na pitanja:**

1. Konfiguracija sadrži i connection string-ove, ključeve i lozinke iz user secrets ili environment varijabli. Endpoint treba da vrati samo ono što je bezbedno pokazati.
2. `Push`. Environment varijable imaju viši prioritet od `appsettings*.json`, a `__` u imenu zamenjuje `:`. Varijablu posle uklonite sa `Remove-Item Env:Notifications__Channel`.

</details>

## Opciono C — Trajanje zahteva sa `Stopwatch` (oko 15 min)

**Cilj:** proširiti `RequestLoggingMiddleware` tako da loguje koliko je obrada zahteva trajala. Middleware je pravo mesto za to, jer obuhvata sve što se izvršava posle njega.

### Koraci

1. U `RequestLoggingMiddleware.cs` dodajte `using System.Diagnostics;`.
2. U `InvokeAsync`, posle poruke `Before`, pokrenite merenje: `var watch = Stopwatch.StartNew();`.
3. Poziv `await _next(context)` i poruku `After` stavite u `try` blok.
4. U `finally` bloku zaustavite merenje i zabeležite strukturisanu poruku `Request took {ElapsedMs} ms` sa `watch.ElapsedMilliseconds`.
5. Restartujte aplikaciju i pozovite `/orders/confirm` nekoliko puta.

### Provera

- Posle `After 200` pojavljuje se `Request took N ms`, gde je N nula ili pozitivan broj. Prvi zahtev je obično sporiji od ostalih.
- HTTP odgovori su isti kao pre; middleware samo meri.
- `dotnet build` prolazi bez upozorenja.

### Izazov — zašto `finally`

Privremeno dodajte endpoint koji baca izuzetak:

```csharp
app.MapGet("/boom", () =>
{
    throw new InvalidOperationException("Boom");
});
```

Pozovite `/boom`. Koje se poruke iz `RequestLoggingMiddleware` pojavljuju? Zatim premestite log sa trajanjem iz `finally` posle `try` bloka i ponovite. Na kraju uklonite `/boom`.

<details>
<summary>Referentno rešenje (otvorite tek kad završite ili zapnete)</summary>

`Middleware/RequestLoggingMiddleware.cs`:

```csharp
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CourseApi.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Before {Path}", context.Request.Path);
        var watch = Stopwatch.StartNew();

        try
        {
            await _next(context);
            _logger.LogInformation("After {StatusCode}", context.Response.StatusCode);
        }
        finally
        {
            watch.Stop();
            _logger.LogInformation("Request took {ElapsedMs} ms", watch.ElapsedMilliseconds);
        }
    }
}
```

**Odgovor na izazov:** za `/boom` se pojavljuju `Before /boom` i `Request took N ms`, ali ne i `After`. Izuzetak izlazi iz `await _next(context)`, pa se ostatak `try` bloka preskače, a `finally` se izvršava uvek. Zatim izuzetak hvata developer exception page, koji ASP.NET Core u Development okruženju sam stavlja na početak pipeline-a, i vraća 500. Bez `finally` ne bi bilo ni poruke o trajanju, a upravo spori i neuspešni zahtevi su oni čije trajanje želimo da vidimo.

</details>

## Opciono D — Serilog: konzola i tekstualni fajl (oko 25 min, može i kao domaći)

**Cilj:** logovi idu u konzolu i u fajl preko Serilog-a, a `OrderService` i `RequestLoggingMiddleware` se ne menjaju ni u jednom redu. Kod piše u `ILogger<T>`, a provider odlučuje gde zapis završava.

**Početno stanje:** vaš `CourseApi` iz ovog lab-a ili referentno rešenje iz `Resenje/`. Za NuGet paket je potreban internet.

### Koraci

1. U folderu projekta dodajte paket (u Visual Studiu: desni klik na projekat → **Manage NuGet Packages** → `Serilog.AspNetCore`):

   ```bash
   dotnet add package Serilog.AspNetCore
   ```

   Proveren je `Serilog.AspNetCore` 10.0.0. Paket već sadrži Console i File sink i čitanje podešavanja iz konfiguracije, pa drugi paketi nisu potrebni.

2. U `Program.cs` dodajte `using Serilog;`. Odmah posle `WebApplication.CreateBuilder(args)` registrujte Serilog preko `builder.Services.AddSerilog(...)`, tako da podešavanja čita iz konfiguracije sa `ReadFrom.Configuration(builder.Configuration)`.
3. U `appsettings.json` dodajte sekciju `"Serilog"`:
   - `MinimumLevel`: `Default` je `Information`, a `Microsoft.AspNetCore` je `Warning`, isto kao u postojećoj sekciji `Logging`
   - `WriteTo`: niz sa dva sink-a, `Console` i `File`; File sink dobija `Args` sa `"path": "logs/courseapi-.log"` i `"rollingInterval": "Day"`
4. Pokrenite aplikaciju (F5 ili `dotnet run`) i pozovite:

   ```bash
   curl -i http://localhost:5100/orders/42/confirm
   ```

### Provera

- Konzola ima Serilog format, na primer `[10:15:02 INF] Before /orders/42/confirm`.
- U folderu projekta postoji `logs/courseapi-YYYYMMDD.log`; datum u ime dodaje `rollingInterval`. U fajlu su `Before /orders/42/confirm`, `Order 42 confirmed` i `After 200`, sa punim datumom i vremenom.
- `OrderService` i middleware su nepromenjeni, a `dotnet build` prolazi bez upozorenja.

### Izazov — strukturisani log u JSON-u

Dodajte **drugi** File sink sa putanjom `logs/courseapi-.json` i formatterom `"Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"` (ključ `formatter` u `Args`). Ponovo pozovite `/orders/42/confirm`.

**Provera:** red za tu porudžbinu sadrži `"@mt":"Order {OrderId} confirmed"`, `"OrderId":42` i `"RequestPath":"/orders/42/confirm"`. `OrderId` je zasebno polje po kom se može pretraživati, a ne samo deo teksta. Zato se u message template-u koristi `{OrderId}`, a ne interpolacija.

### Pitanja za razmišljanje

1. U `appsettings.Development.json` promenite `Logging:LogLevel:Default` u `Warning` i restartujte aplikaciju. Da li su Information logovi nestali? Zašto?
2. U JSON redu postoji `RequestPath`, iako ga niste logovali. Odakle dolazi?
3. Linija `SMS: Order 42 confirmed` sada stoji tačno između `Order 42 confirmed` i `Inner after`, a sa ugrađenim loggerom mogla je da se pojavi na drugom mestu. Šta se promenilo?

### Ako zapnete

- **`AddSerilog` ne postoji:** nedostaje `using Serilog;` ili paket nije dodat (proverite `PackageReference` u `.csproj`).
- **Nema fajla, a nema ni greške:** Serilog tiho preskače sink sa pogrešnim imenom (na primer `"Files"`). Proverite da je `WriteTo` niz i da piše `"Name": "File"`.
- **Fajl je na neočekivanom mestu:** relativna putanja `logs/...` računa se od radnog foldera. F5 i `dotnet run` iz foldera projekta prave ga u folderu projekta.
- **Logovi u Git-u:** `logs/` je već u `.gitignore` ovog repozitorijuma.

<details>
<summary>Referentno rešenje (otvorite tek kad završite ili zapnete)</summary>

`Program.cs`, početak fajla; ostatak je nepromenjen:

```csharp
using CourseApi.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog replaces the default providers; sinks and levels come from the "Serilog" section.
builder.Services.AddSerilog(logger => logger.ReadFrom.Configuration(builder.Configuration));
```

`appsettings.json`, zajedno sa izazovom (drugi File sink):

```json
{
  "Logging": {
    "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": { "Microsoft.AspNetCore": "Warning" }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": { "path": "logs/courseapi-.log", "rollingInterval": "Day" }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/courseapi-.json",
          "rollingInterval": "Day",
          "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
        }
      }
    ]
  },
  "Notifications": { "Channel": "Email" },
  "AllowedHosts": "*"
}
```

**Odgovori na pitanja:**

1. Nisu. Posle `AddSerilog` nivoe određuje `Serilog:MinimumLevel`. Sekcija `Logging` važi za ugrađene providere, a oni se više ne koriste.
2. ASP.NET Core za svaki zahtev otvara logging scope sa `RequestId` i `RequestPath`, a Serilog svojstva scope-a upisuje kao polja zapisa.
3. Ugrađeni console logger piše preko pozadinskog reda, a Serilog Console sink piše odmah, kao i `Console.WriteLine`.

</details>
