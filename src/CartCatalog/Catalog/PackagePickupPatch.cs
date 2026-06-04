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

            int today = Game1.Date.TotalDays;
            var (due, remaining) = PendingOrders.Due(Orders.Orders, today);
            foreach (PendingOrder o in OrderMerge.Merge(due))
            {
                Item item = ItemRegistry.Create(o.ItemId, o.Quantity);
                Item leftover = Game1.player.addItemToInventory(item);
                if (leftover != null && leftover.Stack > 0)
                    Game1.createItemDebris(leftover, pixel, -1, farm);
            }

            Orders.Replace(remaining);
            farm.objects.Remove(tile);
            Game1.playSound("pickUpItem");
            Monitor?.Log($"Collected Cart Catalog package: {due.Count} order(s).", LogLevel.Info);

            __result = true;
            return false;
        }
    }
}
