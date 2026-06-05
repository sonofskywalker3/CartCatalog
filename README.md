# Cart Catalog

[**⬇ Download on Nexus Mods**](https://www.nexusmods.com/stardewvalley/mods/47146)

**Mail-order from the Traveling Cart's actual daily stock.** Buy the Cart Catalog book from the cart, then from any day onward browse the cart's real stock for the day, place an order, and it arrives as a package on your porch the next morning — with a small shipping markup.

> This README mirrors the [Nexus Mods description](https://www.nexusmods.com/stardewvalley/mods/47146); the two are kept identical (the "Building from source" section below is GitHub-only).

## What's New in v1.0.0 — Porch Package Delivery

**Orders now arrive as a physical package on your porch instead of a mailbox letter.** Walk up to the crate the next morning and press the action button to collect everything inside. This fixes the old limit where ordering several different items in one day crammed them into a single hard-to-grab letter slot — every item now comes through in full, and uncollected days accumulate into one crate. Updating from a mail-based version? Any letter still unread in your mailbox is auto-converted into a porch package on your next load — nothing lost.

## Features

- **Order the cart's real daily stock** — the catalog reads the same day-seeded Traveling Cart stock the cart itself sells, so it's deterministic per day (you can't reroll by reopening it).
- **Delivered to your porch** — ordered items arrive in a crate by your farmhouse door the next morning (configurable delay). Walk up and press the action button to collect; if your inventory is full, the rest drops beside the crate. Gold is charged once, up front, at the marked-up price.
- **One crate, everything inside** — a whole day's orders (and any days you haven't collected yet) accumulate into a single package, so distinct items never get lost in a cramped mail slot.
- **Any orderable cart item** — objects, big craftables, furniture, hats, weapons, boots, rings and clothing the cart stocks can all be delivered.
- **Buy-only** — the catalog never lets you sell to the cart; it's a purchase channel only.
- **GMCM support** — tune everything in-game if Generic Mod Config Menu is installed (optional).

## Requirements

- Stardew Valley 1.6.15+
- [SMAPI](https://smapi.io/) 4.0+
- (Optional) [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) for an in-game settings UI

## Install

1. Install [SMAPI](https://smapi.io/).
2. Download this mod and unzip it into your `Stardew Valley/Mods` folder (so you have `Mods/CartCatalog/CartCatalog.dll`).
3. Run the game with SMAPI.

## How to use

1. The **Cart Catalog** book appears in the Traveling Cart for **5,000g**, at roughly a **50% chance per cart visit** until you own one.
2. Buy it, then **place the book** somewhere (it's placeable furniture).
3. **Act on the placed book** (action button) any day to open the catalog over the cart's stock for that day. Order what you want.
4. Your order arrives in a **crate on your porch the next morning** — walk up and press the action button to collect it.

## Configuration

Settings live in `config.json` (created on first run) and can also be edited via GMCM:

- **Markup** (default 1.05) — shipping multiplier applied to each item's cart price (1.05 = +5%).
- **DeliveryDelayDays** (default 1) — whole days between ordering and porch delivery (1 = next morning).
- **BookPrice** (default 5000) — gold price of the Cart Catalog book in the Traveling Cart.
- **AppearanceChance** (default 0.5) — chance the book appears in the cart each visit, while not already owned.

## Art help welcome

Full disclosure: I have zero artistic ability, so the in-game book sprite and the cover art are rough programmer placeholders. If you'd like to make a nicer icon or cover, please do — I'd happily use it and credit you.

## Also by this author

- [**Android Consolizer**](https://www.nexusmods.com/stardewvalley/mods/41869) — Full console-style controller support for Stardew Valley on Android. Play the whole game on a gamepad.
- [**Nap Time**](https://www.nexusmods.com/stardewvalley/mods/42616) — Nap in bed to recover energy without ending the day. Configurable rate and wake-up cap. PC + Android.
- [**The Longest Year**](https://www.nexusmods.com/stardewvalley/mods/47192) — A roguelite time-loop: restore the Community Center within a year, or the Junimos rewind the seasons and you begin again, a little stronger. (Beta)

## Source

Open source (MIT) — [github.com/sonofskywalker3/CartCatalog](https://github.com/sonofskywalker3/CartCatalog)

---

<!-- GitHub-only appendix (not part of the Nexus description) -->

## Building from source

Requires the .NET 6 SDK and a local Stardew Valley + SMAPI install (the [ModBuildConfig](https://github.com/Pathoschild/Stardew.ModBuildConfig) NuGet package locates the game automatically).

```sh
dotnet test tests/CartCatalog.Tests/CartCatalog.Tests.csproj   # run unit tests
dotnet build src/CartCatalog/CartCatalog.csproj                # build (auto-deploys to Mods if the game is closed)
```

License: [MIT](LICENSE) © sonofskywalker3
