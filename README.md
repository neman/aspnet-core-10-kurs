# ASP.NET Core 10 — kurs

Materijal za vežbe kursa ASP.NET Core 10 (Beograd, oktobar 2026).

## Šta je potrebno

- **.NET 10 SDK** — proveri sa `dotnet --version` (očekuje se `10.0.x`). `global.json` u korenu repozitorijuma bira .NET 10 SDK i kada imaš instaliran noviji; ako prijavi da kompatibilan SDK nije pronađen, instaliraj .NET 10 SDK.
- editor ili IDE po izboru: Visual Studio sa podrškom za .NET 10, VS Code sa C# Dev Kit ili Rider
- `curl` ili browser za pozive endpoint-a (na Windows PowerShell-u koristi `curl.exe`)

## Struktura

| Folder | Kada | Sadržaj |
|---|---|---|
| `Modul-1/Lab-01-DI/` | Dan 1, 09:45–10:15 | Dependency injection: Email/SMS izbor kroz factory i DI lifetimes |
| `Modul-1/Lab-02-Middleware/` | Dan 1, 11:12–11:47 | Sopstveni middleware, extension metoda i redosled izvršavanja |
| `Modul-2/HeroRegistry-starter/` | Dan 1, kraj | Početni MVC projekat za Superhero Registry |
| `Modul-2/Lab-01-MVC-Heroji/` | Dan 2, 09:30–10:15 | Kontroler, ViewModel, strongly typed view-ovi i layout: lista i detalji heroja |
| `Modul-2/Lab-02-Forma-Validacija/` | Dan 2, 10:50–11:35 | Forma za novog heroja, validacija, antiforgery i Post/Redirect/Get |
| `Modul-2/Opciono/` | Dan 2, 12:00–13:00 | Opcioni samostalni zadaci: izmena Draft heroja, View Component, klijentska validacija |

U svakom lab folderu uputstvo je u `README.md`, a projekat u podfolderu. Svaki lab počinje od sopstvenog startera, pa nijedan ne zavisi od toga da li je prethodni završen. Referentna rešenja se objavljuju u folderu `Resenje/` posle lab-a.

## Pokretanje projekta

```bash
cd Modul-1/Lab-01-DI/CourseApi
dotnet build
dotnet run
```

Svaki projekat možeš pokrenuti i iz Visual Studia sa **F5**. I F5 i `dotnet run` koriste profil iz `Properties/launchSettings.json`, isti u svim lab-ovima: `http://localhost:5100` i `https://localhost:5101`, okruženje Development. Pošto je port isti, u jednom trenutku radi samo jedna aplikacija. Zahtevi u `.http` fajlovima idu na `http`, pa ti development sertifikat nije potreban; za `https` adresu Visual Studio pri prvom pokretanju nudi da ga označiš kao pouzdan (ili `dotnet dev-certs https --trust`). Ako aplikacija ne startuje uz poruku `Unable to configure HTTPS endpoint`, na računaru nema development sertifikata: pokreni `dotnet dev-certs https --trust`, pa ponovo pokreni aplikaciju.

Endpoint-i za svaki lab navedeni su u njegovom uputstvu. Aplikaciju zaustavljaš sa `Ctrl+C` (u Visual Studiu **Stop**).
