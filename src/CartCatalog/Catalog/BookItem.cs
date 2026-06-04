using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CartCatalog.Catalog
{
    /// <summary>Registers the Cart Catalog book as custom PLACEABLE furniture via a Data/Furniture
    /// edit, and opens the catalog when the placed book is acted on (set it down, press the action
    /// button on it) — like a placed in-world recipe book.</summary>
    internal sealed class BookItem
    {
        /// <summary>Unqualified item id. Qualified id is "(F)" + this (it's furniture).</summary>
        public const string ItemId = "sonofskywalker3.CartCatalog_Book";
        public static string QualifiedId => "(F)" + ItemId;

        private const string FurnitureAsset = "Data/Furniture";
        // The mod-provided book sprite (assets/book.png) the Data/Furniture row points at.
        internal const string BookTextureAsset = "Mods/sonofskywalker3.CartCatalog/Book";

        private readonly IModHelper _helper;
        // Static so the static Harmony checkForAction patch can reach it.
        private static Action _openCatalog;

        public BookItem(IModHelper helper, Action openCatalog)
        {
            _helper = helper;
            _openCatalog = openCatalog;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(BookTextureAsset))
            {
                e.LoadFromModFile<Texture2D>("assets/book.png", AssetLoadPriority.Medium);
                return;
            }

            if (e.NameWithoutLocale.IsEquivalentTo(FurnitureAsset))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;
                    // Format: name/type/tilesheetSize/boundingBox/rotations/price/placementRestriction/DisplayName/spriteIndex/texture
                    // decor, 1x1 tile, 1 rotation, free, no placement restriction (-1), sprite 0 (16x16).
                    // The texture path MUST use BACKSLASHES: Data/Furniture is split on '/', so a "Mods/.../Book"
                    // value would be truncated to "Mods" in the texture field. SMAPI still matches it on load.
                    string tex = BookTextureAsset.Replace('/', '\\');
                    data[ItemId] = $"Cart Catalog/decor/1 1/1 1/1/0/-1/Cart Catalog/0/{tex}";
                }, AssetEditPriority.Default);
            }
        }

        /// <summary>Open the catalog when our placed book furniture is acted on (set it down, then
        /// press the action button). Returns false to swallow vanilla furniture behavior for our id.</summary>
        [HarmonyLib.HarmonyPatch(typeof(StardewValley.Objects.Furniture), nameof(StardewValley.Objects.Furniture.checkForAction))]
        internal static class CheckForActionPatch
        {
            private static bool Prefix(StardewValley.Objects.Furniture __instance, bool justCheckingForActivity, ref bool __result)
            {
                if (justCheckingForActivity) return true;
                if (__instance.ItemId != ItemId) return true;   // only our book
                if (_openCatalog == null) return true;
                _openCatalog();
                __result = true;
                return false;   // skip vanilla furniture action
            }
        }

        /// <summary>Draw the carried book at full inventory-slot scale. Vanilla caps a 1x1 furniture's
        /// menu icon at half-slot size, so the book looked tiny in the toolbar; render the sprite scaled
        /// to fill the slot like a normal item. Everything else falls through to vanilla.</summary>
        [HarmonyLib.HarmonyPatch(typeof(StardewValley.Objects.Furniture), nameof(StardewValley.Objects.Furniture.drawInMenu),
            new System.Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float),
                typeof(StardewValley.StackDrawType), typeof(Color), typeof(bool) })]
        internal static class DrawInMenuPatch
        {
            private static bool Prefix(StardewValley.Objects.Furniture __instance, SpriteBatch spriteBatch,
                Vector2 location, float scaleSize, float transparency, float layerDepth, Color color)
            {
                if (__instance.ItemId != ItemId) return true;

                StardewValley.ItemTypeDefinitions.ParsedItemData data =
                    StardewValley.ItemRegistry.GetDataOrErrorItem(__instance.QualifiedItemId);
                Texture2D tex = data.GetTexture();
                Rectangle src = data.GetSourceRect();
                float scale = (64f / Math.Max(src.Width, src.Height)) * scaleSize;
                spriteBatch.Draw(tex, location + new Vector2(32f, 32f), src, color * transparency, 0f,
                    new Vector2(src.Width / 2f, src.Height / 2f), scale, SpriteEffects.None, layerDepth);
                return false;
            }
        }
    }
}
