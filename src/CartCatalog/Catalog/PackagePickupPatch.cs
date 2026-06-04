using CartCatalog.Core;
using CartCatalog.Delivery;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;

namespace CartCatalog.Catalog
{
    /// <summary>Collect the porch package: pressing the action button (checkForAction) on the
    /// modData-flagged package hands its merged due orders to the player (overflow drops as ground
    /// debris at the tile), clears those orders from the OrderStore, removes the object, and plays a
    /// pickup sound. Gated strictly by the modData flag so no other object is affected.
    ///
    /// Statics are set from ModEntry so this static patch can reach the live OrderStore/Monitor. The
    /// OrderStore reference is stable for the session; its .Orders list getter always returns the
    /// current list even after Load()/Replace() reassign it.</summary>
    [HarmonyPatch(typeof(SObject), nameof(SObject.checkForAction))]
    internal static class Object_checkForAction_PackagePatch
    {
        internal static OrderStore Orders;
        internal static IMonitor Monitor;

        private static bool Prefix(SObject __instance, bool justCheckingForActivity, ref bool __result)
        {
            if (Orders == null || !__instance.modData.ContainsKey(PorchDelivery.PackageFlag))
                return true;   // not our package (or not wired yet) — run vanilla

            if (justCheckingForActivity)
            {
                __result = true;   // report actionable without consuming
                return false;
            }

            Vector2 tile = __instance.TileLocation;
            GameLocation farm = __instance.Location ?? Game1.getFarm();
            Vector2 pixel = tile * 64f + new Vector2(32f, 32f);

            var (due, remaining) = PendingOrders.Due(Orders.Orders, Game1.Date.TotalDays);
            foreach (PendingOrder o in OrderMerge.Merge(due))
                GrantOrDrop(o.ItemId, o.Quantity, pixel, farm);

            Orders.Replace(remaining);
            // TileLocation and the objects-dict key are set together in PorchDelivery.PlaceIfDue().
            farm.objects.Remove(tile);
            Game1.playSound("pickUpItem");

            // due.Count == 0 should be unreachable (a package is only placed when orders are due),
            // but a hand-edited/corrupt save could reach it — warn instead of silently eating the crate.
            if (due.Count > 0)
                Monitor?.Log($"Collected Cart Catalog package: {due.Count} order(s).", LogLevel.Info);
            else
                Monitor?.Log("Removed a stale Cart Catalog package with no due orders.", LogLevel.Warn);

            __result = true;
            return false;
        }

        /// <summary>Add <paramref name="quantity"/> of an item to the player, split into max-stack-sized
        /// chunks so non-stackable types (furniture/rings/boots/hats/weapons/big-craftables) never form
        /// an illegal over-stack. Anything that doesn't fit drops as ground debris at the package tile.</summary>
        private static void GrantOrDrop(string itemId, int quantity, Vector2 pixel, GameLocation location)
        {
            int max = ItemRegistry.Create(itemId, 1).maximumStackSize();
            if (max < 1) max = 1;
            int remaining = quantity;
            while (remaining > 0)
            {
                int n = System.Math.Min(max, remaining);
                Item leftover = Game1.player.addItemToInventory(ItemRegistry.Create(itemId, n));
                if (leftover != null && leftover.Stack > 0)
                    Game1.createItemDebris(leftover, pixel, -1, location);
                remaining -= n;
            }
        }
    }

    /// <summary>Stop a tool swing — or the stand-on-tile action path, which tries the pickaxe/axe
    /// first — from destroying the porch package. Returning false skips vanilla destruction;
    /// GameLocation.checkAction then falls through to checkForAction, so pressing action while standing
    /// on the crate still collects it. Gated by the same modData flag.</summary>
    [HarmonyPatch(typeof(SObject), nameof(SObject.performToolAction))]
    internal static class Object_performToolAction_PackagePatch
    {
        private static bool Prefix(SObject __instance, ref bool __result)
        {
            if (!__instance.modData.ContainsKey(PorchDelivery.PackageFlag))
                return true;     // not our package — run vanilla
            __result = false;    // not destroyed/consumed by the tool
            return false;
        }
    }
}
