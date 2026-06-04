using System;
using System.Collections.Generic;
using CartCatalog.Core;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using SObject = StardewValley.Object;

namespace CartCatalog.Delivery
{
    /// <summary>Delivers due orders as ONE physical package on the farmhouse porch. The package is
    /// transient — rebuilt from the persisted OrderStore on each load and removed before the game
    /// saves — so no unregistered object id is ever serialized into the farm save. Due orders remain
    /// in the OrderStore until the player collects the package (handled by PackagePickupPatch).</summary>
    internal sealed class PorchDelivery
    {
        /// <summary>modData key marking our package so the checkForAction patch only fires on it.</summary>
        internal const string PackageFlag = "sonofskywalker3.CartCatalog/Package";

        /// <summary>Internal name of the vanilla big-craftable used as the package sprite (a wooden
        /// delivery crate). Resolved to a qualified id by Name at runtime because the numeric id
        /// differs between game versions — never hardcode it.</summary>
        private const string PackageItemName = "Crate";
        private const string FallbackItemName = "Barrel";

        // Guaranteed-valid visible fallback if neither crate sprite is in the game data.
        private const string FallbackQualifiedId = "(O)388";   // Wood

        // Resolved qualified id ("(BC)<key>"); looked up once per session, cached.
        private string _packageQualifiedId;

        // Max ring radius for the nearest-free-tile fallback search around the door.
        private const int MaxSearchRadius = 6;

        // Preferred package tiles as X-offsets right of the door entry: +3 first (the porch tile the
        // user wants), +1 as fallback. Both are non-buildable porch tiles and dodge the spouse spot (+2).
        private static readonly int[] PreferredOffsetsX = { 3, 1 };

        private readonly IMonitor _monitor;
        private readonly OrderStore _orders;

        public PorchDelivery(IModHelper helper, IMonitor monitor, OrderStore orders)
        {
            _monitor = monitor;
            _orders = orders;
            helper.Events.GameLoop.DayStarted += (_, _) => this.PlaceIfDue();
        }

        /// <summary>Place the package if any orders are due and one isn't already on the farm. Due
        /// orders stay in the OrderStore until the player collects the package.</summary>
        public void PlaceIfDue()
        {
            if (!Context.IsWorldReady) return;

            int today = Game1.Date.TotalDays;
            var (due, _) = PendingOrders.Due(_orders.Orders, today);
            if (due.Count == 0) return;

            Farm farm = Game1.getFarm();
            if (farm == null) return;
            if (TryFindPackageTile(farm, out _)) return;   // already placed

            if (!TryFindFreeTile(farm, out Vector2 tile))
            {
                _monitor.Log("No free tile near the farmhouse door for the Cart Catalog package.", LogLevel.Warn);
                return;
            }

            var pkg = (SObject)ItemRegistry.Create(this.ResolvePackageId());
            pkg.modData[PackageFlag] = "1";
            // checkAction only routes "Crafting"/"interactive" types to checkForAction. Big-craftables
            // already qualify; promote the Wood fallback so the package is always collectable.
            if (pkg.Type != "Crafting" && pkg.Type != "interactive")
                pkg.Type = "Crafting";
            pkg.TileLocation = tile;
            pkg.Location = farm;
            farm.objects[tile] = pkg;
            Point entry = farm.GetMainFarmHouseEntry();
            _monitor.Log(
                $"Placed Cart Catalog package at {tile} (door entry {entry.X},{entry.Y}) carrying {due.Count} order(s).",
                LogLevel.Info);
        }

        /// <summary>Resolve the crate sprite's qualified id by Name from the loaded big-craftable data
        /// (version-proof). Falls back to a barrel, then to Wood, logging if the crate isn't found.</summary>
        private string ResolvePackageId()
        {
            if (_packageQualifiedId != null) return _packageQualifiedId;

            foreach (string wanted in new[] { PackageItemName, FallbackItemName })
            {
                foreach (KeyValuePair<string, BigCraftableData> kv in Game1.bigCraftableData)
                {
                    if (string.Equals(kv.Value.Name, wanted, StringComparison.OrdinalIgnoreCase))
                    {
                        _packageQualifiedId = "(BC)" + kv.Key;
                        return _packageQualifiedId;
                    }
                }
            }

            _packageQualifiedId = FallbackQualifiedId;
            _monitor.Log("Crate/Barrel big-craftable not found in data; using Wood as the package sprite.", LogLevel.Warn);
            return _packageQualifiedId;
        }

        /// <summary>Remove the package so it isn't serialized into the save (re-placed on next load
        /// from the persisted OrderStore). Called on Saving.</summary>
        public void RemovePackage()
        {
            Farm farm = Game1.getFarm();
            if (farm == null) return;
            if (TryFindPackageTile(farm, out Vector2 tile))
                farm.objects.Remove(tile);
        }

        private static bool TryFindPackageTile(Farm farm, out Vector2 tile)
        {
            foreach (KeyValuePair<Vector2, SObject> kv in farm.objects.Pairs)
            {
                if (kv.Value.modData.ContainsKey(PackageFlag))
                {
                    tile = kv.Key;
                    return true;
                }
            }
            tile = default;
            return false;
        }

        /// <summary>Pick the package tile. Preferred porch spots, in order: 3 tiles right of the door
        /// entry, then 1 tile right — both are non-buildable porch tiles (so they never collide with a
        /// player's chest/furnace), and both clear the spouse's porch-standing spot at entry.X+2
        /// (FarmHouse.getPorchStandingSpot) and the door at (entry.X, entry.Y-1). These are force-placed:
        /// we only require the tile is on the map and not already holding an object — NOT that the game
        /// considers it player-placeable (these porch tiles aren't). Falls back to a spiral search
        /// (skipping the door, its flanking tiles, and the spouse spot) if both preferred spots are
        /// taken.</summary>
        private static bool TryFindFreeTile(Farm farm, out Vector2 tile)
        {
            Point entry = farm.GetMainFarmHouseEntry();

            foreach (int dx in PreferredOffsetsX)
            {
                var spot = new Vector2(entry.X + dx, entry.Y);
                if (farm.isTileOnMap(spot) && !farm.objects.ContainsKey(spot))
                {
                    tile = spot;
                    return true;
                }
            }

            var avoid = new HashSet<Vector2>
            {
                new Vector2(entry.X, entry.Y - 1),   // door
                new Vector2(entry.X - 1, entry.Y),   // door-flanking (unusable)
                new Vector2(entry.X + 1, entry.Y),   // door-flanking (unusable)
                new Vector2(entry.X + 2, entry.Y),   // spouse porch-standing spot
            };

            for (int r = 1; r <= MaxSearchRadius; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                {
                    if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != r) continue;   // ring perimeter only
                    var c = new Vector2(entry.X + dx, entry.Y + dy);
                    if (avoid.Contains(c)) continue;
                    if (IsFree(farm, c))
                    {
                        tile = c;
                        return true;
                    }
                }
            }
            tile = default;
            return false;
        }

        private static bool IsFree(Farm farm, Vector2 tile)
            => !farm.objects.ContainsKey(tile) && farm.CanItemBePlacedHere(tile);
    }
}
