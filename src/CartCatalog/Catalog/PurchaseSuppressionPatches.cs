using HarmonyLib;
using StardewValley;
using SObject = StardewValley.Object;

namespace CartCatalog.Catalog
{
    /// <summary>While the Cart Catalog menu is open, force every salable's <c>actionWhenPurchased</c>
    /// to return true so the item is NOT granted to the inventory — it's mail-ordered instead.
    ///
    /// Both purchase paths skip the immediate grant when <c>actionWhenPurchased</c> returns true:
    /// vanilla <c>ShopMenu.tryToPurchaseItem</c> nulls <c>heldItem</c> (so nothing is added), and
    /// AndroidConsolizer's A-button purchase prefix marks the buy "handled" and skips its own
    /// <c>addItemToInventoryBool</c>. (Just nulling <c>heldItem</c> in the on_purchase callback was
    /// NOT enough — AC's path adds the item directly and ignores heldItem.)
    ///
    /// Scoped strictly to OUR menu instance: the real Traveling Cart uses the same "Traveler" ShopId,
    /// so we match on <see cref="CatalogMenu.ActiveCatalog"/> by reference, never by id.
    ///
    /// Most cart salables are <c>StardewValley.Object</c> (and Furniture/Ring subclasses), which use
    /// <c>Object.actionWhenPurchased</c>; hats/boots/weapons/clothing use <c>Item.actionWhenPurchased</c>.
    /// Patch both base methods so every mailable type is covered.</summary>
    internal static class PurchaseSuppression
    {
        internal static bool ShouldSuppressGrant()
            => CatalogMenu.ActiveCatalog != null
               && ReferenceEquals(Game1.activeClickableMenu, CatalogMenu.ActiveCatalog);
    }

    [HarmonyPatch(typeof(Item), nameof(Item.actionWhenPurchased))]
    internal static class Item_actionWhenPurchased_Patch
    {
        private static bool Prefix(ref bool __result)
        {
            if (!PurchaseSuppression.ShouldSuppressGrant()) return true;
            __result = true;   // treat as "handled" → no grant; order is mailed
            return false;      // skip the original
        }
    }

    [HarmonyPatch(typeof(SObject), nameof(SObject.actionWhenPurchased))]
    internal static class Object_actionWhenPurchased_Patch
    {
        private static bool Prefix(ref bool __result)
        {
            if (!PurchaseSuppression.ShouldSuppressGrant()) return true;
            __result = true;
            return false;
        }
    }
}
