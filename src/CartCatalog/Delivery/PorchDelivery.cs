using System;
using System.Collections.Generic;
using CartCatalog.Core;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
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

        /// <summary>Big-craftable Supply Crate sprite — a wooden delivery crate. Verify it resolves in
        /// the SMAPI log on first run; if ItemRegistry logs an unknown-id error, swap for another
        /// crate/box id and rebuild.</summary>
        internal const string PackageItemId = "(BC)165";

        // Max ring radius for the nearest-free-tile fallback search around the door.
        private const int MaxSearchRadius = 6;

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

            var pkg = (SObject)ItemRegistry.Create(PackageItemId);
            pkg.modData[PackageFlag] = "1";
            pkg.TileLocation = tile;
            pkg.Location = farm;
            farm.objects[tile] = pkg;
            _monitor.Log($"Placed Cart Catalog package at {tile} carrying {due.Count} order(s).", LogLevel.Info);
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

        /// <summary>Use the door entry tile if free, else the nearest free placeable tile spiraling
        /// outward, skipping the two tiles flanking the door (user reports those are unusable).</summary>
        private static bool TryFindFreeTile(Farm farm, out Vector2 tile)
        {
            Point entry = farm.GetMainFarmHouseEntry();
            var flanking = new HashSet<Vector2>
            {
                new Vector2(entry.X - 1, entry.Y),
                new Vector2(entry.X + 1, entry.Y),
            };

            var center = new Vector2(entry.X, entry.Y);
            if (IsFree(farm, center))
            {
                tile = center;
                return true;
            }

            for (int r = 1; r <= MaxSearchRadius; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                {
                    if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != r) continue;   // ring perimeter only
                    var c = new Vector2(entry.X + dx, entry.Y + dy);
                    if (flanking.Contains(c)) continue;
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
