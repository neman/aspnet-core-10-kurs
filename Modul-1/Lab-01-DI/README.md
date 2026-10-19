# Lab 01 — Dependency Injection u CourseApi

**Vreme:** 09:45–10:15 (30 min, uklj. proveru)  
**Način:** samostalno ili u paru.  
**Početna tačka:** projekat `CourseApi/` u ovom folderu (radi bez prethodne vežbe).

```bash
cd Modul-1/Lab-01-DI/CourseApi
dotnet run
```

Ili otvori projekat u Visual Studiu i pokreni ga sa **F5**. Oba načina koriste profil iz `Properties/launchSettings.json`, pa je adresa ista: `http://localhost:5100`.

Na Windows PowerShell-u za pozive koristi `curl.exe` ili browser: `http://localhost:5100/health`. U Visual Studiu sve pozive iz lab-a imaš u `CourseApi/CourseApi.http` (**Send request** iznad svakog zahteva).

## Cilj
Zameniti implementaciju obaveštavanja konfiguracijom bez menjanja `OrderService`, a zatim eksperimentom proveriti kako DI lifetime utiče na deljenje instanci.

## Plan rada
- **Prva 2 min** — pročitaj zadatke i pokreni projekat; proveri `/health` i `/orders/confirm`.
- **Sledećih 12 min** — zadaci 1–3 (Email/SMS i factory).
- **Sledećih 12 min** — zadatak 4 (lifetime eksperiment).
- **Završna 4 min** — proveri rezultat s predavačem.

## Zadatak 1 — druga implementacija
Napravi `SmsSender : INotificationSender` koji u konzoli ispisuje `SMS: {message}`. Postojeći `OrderService` ne menjaj.

## Zadatak 2 — izbor iz konfiguracije
U `appsettings.json` koristi `Notifications:Channel` sa vrednostima `Email` i `Sms`. Umesto direktne DI registracije EmailSender-a implementiraj **factory** za `INotificationSender` koji čita konfiguraciju i bira odgovarajuću implementaciju. Ostavi `OrderService` zavisnim samo od interfejsa.

## Zadatak 3 — funkcionalna provera
Pozovi `GET /orders/confirm` za oba kanala. Očekuje se HTTP 200 sa `"Order confirmed"`, a u konzoli `Email: Order confirmed` ili `SMS: Order confirmed`. Nakon izmene konfiguracije **restartuj aplikaciju** radi predvidljive provere. Oprez: starter `appsettings.Development.json` ne postavlja ovaj ključ.

## Zadatak 4 — koliko dugo živi instanca?
Napravi `InstanceMarker` sa svojstvom `Guid Id`, te dva potrošača `FirstConsumer` i `SecondConsumer` koji dobijaju `InstanceMarker` **kroz konstruktor** i izlažu `MarkerId`. Registruj potrošače kao `Transient`. Dodaj `GET /di/lifetime` koji dobija oba potrošača kroz parametre handler-a i vraća `{ first, second, same }`.

Prvo registruj marker kao **Scoped**, zatim probaj **Transient**, pa **Singleton** (jednu registraciju u svakom trenutku; posle promene restartuj). Pozovi endpoint dvaput u svakoj varijanti.

| Registracija | U jednom zahtevu | Između dva zahteva, bez restartovanja |
|---|---|---|
| Transient | `First != Second` | Nove instance |
| Scoped | `First == Second` | Novi Id |
| Singleton | `First == Second` | Isti Id |

## Gotovo kada...
- Email/SMS izbor radi bez izmene `OrderService`.
- `GET /di/lifetime` pokazuje očekivani rezultat za sva tri lifetime-a.
- U endpoint-u ne koristiš `IServiceProvider` za ručno rezolviranje instance.

## Pomoć, ako zapneš
- Kako da pročitam konfiguraciju? `IConfiguration` je već registrovan u DI, pa ga factory može zatražiti od `IServiceProvider`-a koji dobija kao parametar. Ugnežđeni ključ se čita sa dvotačkom: `config["Notifications:Channel"]`. Konfiguraciju detaljno radimo posle pauze.
- `Unable to resolve service`? Proveri DI registracije.
- Uvek Email? Proveri ključ `Notifications:Channel`, pravopis i aktivno okruženje.
- Scoped daje dva ID-ja? Proveri da su oba consumer-a u istom HTTP zahtevu.
- Zbunjujući rezultati? Menjaj samo jednu registraciju i restartuj aplikaciju između lifetime varijanti.

## Bonus — opciono, samo ako ostane vremena
Dodaj klasu `MarkerCache` koja kroz konstruktor dobija `InstanceMarker` i izlaže `MarkerId`. Registruj je kao **Singleton**, a marker ostavi kao **Scoped**. Dodaj `GET /di/captive` koji dobija `MarkerCache` i `InstanceMarker` kroz parametre handler-a i vraća `{ cached, current }`.

1. Pokreni aplikaciju kao i do sada (`dotnet run` ili F5). Šta se desi i zašto?
2. Pokreni je u Production okruženju: `dotnet run --no-launch-profile --urls http://localhost:5100`. Pozovi `/di/captive` dvaput i uporedi `cached` i `current`.
3. Objasni zašto ovo nije ispravan graf zavisnosti i zašto nije bezbedno da singleton zadrži scoped servis. Ispravku ne moraš da implementiraš.

Gotovo kada umeš da objasniš oba ishoda. Posle bonusa vrati Program.cs u prethodno stanje ili ukloni `AddSingleton<MarkerCache>()`, jer se aplikacija u Development okruženju inače ne pokreće.

**Važno:** referentno rešenje biće objavljeno u ovom folderu kao `Resenje/` posle lab-a. Bonus nije preduslov nastavka kursa, a middleware lab počinje od sopstvenog startera.
