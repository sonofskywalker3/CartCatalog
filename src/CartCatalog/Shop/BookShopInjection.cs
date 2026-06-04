using System;
using System.Globalization;
using CartCatalog.Catalog;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;

namespace CartCatalog.Shop
{
    /// <summary>Adds the Cart Catalog book to the Traveling Cart's stock. The appearance chance is
    /// applied as a random gate; the book is withheld once the player already owns a copy.</summary>
    internal sealed class BookShopInjection
    {
        private const string ShopsAsset = "Data/Shops";
        private const string TravelerShopId = "Traveler";
        private const string EntryId = "sonofskywalker3.CartCatalog_BookEntry";

        private readonly Func<int> _price;
        private readonly Func<double> _chance;

        public BookShopInjection(IModHelper helper, Func<int> price, Func<double> chance)
        {
            _price = price;
            _chance = chance;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
        }

        private static bool PlayerOwnsBook()
        {
            if (!Context.IsWorldReady) return false;
            if (Game1.player.Items.ContainsId(BookItem.QualifiedId)) return true;
            bool placed = false;
            Utility.ForEachLocation(loc =>
            {
                foreach (StardewValley.Objects.Furniture f in loc.furniture)
                {
                    if (f.QualifiedItemId == BookItem.QualifiedId) { placed = true; return false; }
                }
                return true;
            });
            return placed;
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo(ShopsAsset)) return;
            e.Edit(asset =>
            {
                var shops = asset.AsDictionary<string, ShopData>().Data;
                if (!shops.TryGetValue(TravelerShopId, out ShopData traveler)) return;
                if (PlayerOwnsBook()) return; // don't offer once owned

                traveler.Items.Add(new ShopItemData
                {
                    Id = EntryId,
                    ItemId = BookItem.QualifiedId,
                    Price = _price(),
                    AvailableStock = 1,
                    // RANDOM <chance>: GameStateQuery re-evaluates each time the shop stock is built,
                    // so ~0.5 means roughly half of cart visits offer the book.
                    Condition = "RANDOM " + _chance().ToString("0.###", CultureInfo.InvariantCulture),
                });
            }, AssetEditPriority.Default);
        }
    }
}
