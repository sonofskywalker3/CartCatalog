# Cart Catalog

A [Stardew Valley](https://www.stardewvalley.net/) [SMAPI](https://smapi.io/) mod that lets you
**mail-order from the Traveling Cart's actual daily stock**. Buy the *Cart Catalog* book from the
cart, then from any day onward browse the cart's real stock for the day, place an order, and it
arrives in your mailbox the next morning — with a small shipping markup.

> **Release 0.9** — feature-complete public preview. Please report issues on the
> [issue tracker](https://github.com/sonofskywalker3/CartCatalog/issues).

## Features

- **Order the cart's real daily stock** — the catalog reads the same day-seeded Traveling Cart
  stock the cart itself sells, so it's deterministic per day (you can't reroll by reopening it).
- **Delivered by mail** — ordered items arrive as a mailbox letter the next morning (configurable
  delay). Gold is charged once, up front, at the marked-up price.
- **Any orderable cart item** — objects, big craftables, furniture, hats, weapons, boots, rings,
  and clothing the cart stocks are all mailable.
- **Buy-only** — the catalog never lets you sell to the cart; it's a purchase channel only.
- **GMCM support** — tune everything in-game if [Generic Mod Config
  Menu](https://www.nexusmods.com/stardewvalley/mods/5098) is installed (optional).

## Requirements

- Stardew Valley 1.6.15+
- [SMAPI](https://smapi.io/) 4.0.0 or newer
- (Optional) [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) for an
  in-game settings UI

## Install

1. Install [SMAPI](https://smapi.io/).
2. Download the latest `CartCatalog` release and unzip it into your `Stardew Valley/Mods` folder
   (so you have `Mods/CartCatalog/CartCatalog.dll`).
3. Run the game with SMAPI.

## How to use

1. The **Cart Catalog** book appears in the Traveling Cart for **5,000g**, at roughly a **50% chance
   per cart visit** until you own one.
2. Buy it, then **place the book** somewhere (it's placeable furniture).
3. **Act on the placed book** (action button) any day to open the catalog over the cart's stock for
   that day. Order what you want.
4. Your order arrives in the **mailbox the next morning**.

## Configuration

Settings live in `config.json` (created on first run) and can also be edited via GMCM:

| Setting | Default | Description |
|---|---|---|
| `Markup` | `1.05` | Shipping multiplier applied to each item's cart price (`1.05` = +5%). |
| `DeliveryDelayDays` | `1` | Whole days between ordering and mailbox delivery (`1` = next morning). |
| `BookPrice` | `5000` | Gold price of the Cart Catalog book in the Traveling Cart. |
| `AppearanceChance` | `0.5` | Chance the book appears in the cart each visit, while not already owned. |

## Building from source

Requires the .NET 6 SDK and a local Stardew Valley + SMAPI install (the
[ModBuildConfig](https://github.com/Pathoschild/Stardew.ModBuildConfig) NuGet package locates the
game automatically).

```sh
dotnet test tests/CartCatalog.Tests/CartCatalog.Tests.csproj   # run unit tests
dotnet build src/CartCatalog/CartCatalog.csproj                # build (auto-deploys to Mods if the game is closed)
```

## License

[MIT](LICENSE) © sonofskywalker3
