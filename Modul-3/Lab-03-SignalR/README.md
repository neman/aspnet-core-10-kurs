# Lab 03 — A live hero list with SignalR

**Time:** 11:28–11:43 (15 min, including review)  
**Format:** individually or in pairs.  
**Starting point:** the `HeroRegistry/` project in this folder. It is the Lab 02 solution plus the instructor's Demo 3: `Hubs/HeroHub.cs`, `AddSignalR()` and `MapHub` in `Program.cs`, `register` sends `HeroRegistered`, and the JavaScript client at the bottom of `Views/Heroes/Index.cshtml` adds the new hero to the list.

The database is the same `HeroRegistry` as in Lab 02. If you start from this folder on a new machine, run `Update-Database` first. The page loads the SignalR client from `cdn.jsdelivr.net`, so it needs internet access.

## Goal

When a hero is retired through the API, every open `/Heroes` page removes it from the list without a refresh. That is the other half of the demo: the server sends, the browser reacts.

## Plan

- **First 3 min**: run the app, open `/Heroes`, and send the Lab 3 requests from `HeroRegistry.http` up to and including Register. Skylark appears in the list without a refresh.
- **Next 4 min**: task 1 (server).
- **Next 4 min**: task 2 (browser).
- **Next 2 min**: the check.
- **Last 2 min**: review with the instructor.

## Task 1 — the server sends `HeroRetired`

In `HeroesApiController.Retire`, after `SaveChangesAsync`, send the message `HeroRetired` to all clients with the hero's id. Look at how `Register` does it with `hub.Clients.All.SendAsync(...)`.

Why after the save, and not before it?

<details>
<summary>Hint (open only if you're stuck)</summary>

The id is enough: the browser already has the hero in its list.

```csharp
hero.Retire();
await store.SaveChangesAsync();

await hub.Clients.All.SendAsync("HeroRetired", hero.Id);
```

</details>

## Task 2 — the browser removes the hero

In `Views/Heroes/Index.cshtml`, next to `connection.on("HeroRegistered", ...)`, handle `HeroRetired`: find the `<li>` whose `data-hero-id` is the received id and remove it.

```js
heroes.querySelector(`[data-hero-id="${id}"]`)?.remove();
```

<details>
<summary>Hint (open only if you're stuck)</summary>

Before `connection.start()`:

```js
connection.on("HeroRetired", id => {
    heroes.querySelector(`[data-hero-id="${id}"]`)?.remove();
});
```

</details>

## Check

1. Open `/Heroes` in **two** browser windows.
2. Send Retire from `HeroRegistry.http` for Skylark, then for Aurora (`POST api/heroes/2/retire`).
3. Both windows remove the hero without a refresh. After F5 the list is the same, because it comes from the database.

## Done when...

- Retiring through the API removes the hero from every open `/Heroes` page.
- Registering still adds the hero (the demo keeps working).
- The message is sent only after `SaveChangesAsync`.

## Help, if you get stuck

- **Nothing happens on the page**: F12 → Console. `signalR is not defined` means the CDN script didn't load (internet). A JavaScript error in `on(...)` stops only that handler.
- **The message name doesn't match**: `"HeroRetired"` on the server and in `connection.on` must be the same; the name is case-insensitive, but not the spelling.
- **`id` is an object**: you sent `new { hero.Id }` instead of `hero.Id`; then use `data.id`.
- **Changes to `Index.cshtml` don't show**: reload with Ctrl+F5, or restart the app if Hot Reload is off.

## Bonus — optional, only if time allows

1. **Reconnect.** Add `.withAutomaticReconnect()` to the builder, and show the connection state on the page (`connection.onreconnecting`, `connection.onreconnected`). Stop the app and start it again: what does the page show?
2. **A typed hub.** Define `public interface IHeroClient { Task HeroRegistered(object hero); Task HeroRetired(int id); }` and use `Hub<IHeroClient>` and `IHubContext<HeroHub, IHeroClient>`. What do you gain compared to `SendAsync("HeroRetired", ...)`?

**Important:** the reference solution will be published in this folder as `Solution/` after the lab. It is the reference version at the end of Day 3, and Day 4 starts from it.
