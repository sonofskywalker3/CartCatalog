using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Internal;
using StardewValley.Menus;

namespace CartCatalog.Catalog
{
    /// <summary>Builds a vanilla ShopMenu over today's Traveling Cart stock, with marked-up prices
    /// routed through MailOrderSalable so purchases become next-day mail orders.</summary>
    internal sealed class CatalogMenu
    {
        private const string TravelerShopId = "Traveler";

        private readonly IMonitor _monitor;
        private readonly Func<double> _markup;
        private readonly Action<ISalable, int> _onOrder;

        public CatalogMenu(IMonitor monitor, Func<double> markup, Action<ISalable, int> onOrder)
        {
            _monitor = monitor;
            _markup = markup;
            _onOrder = onOrder;
        }

        /// <summary>Open the catalog as the active menu, or show a message if the cart has no stock today.</summary>
        public void Open()
        {
            Dictionary<ISalable, ItemStockInformation> raw = ShopBuilder.GetShopStock(TravelerShopId);
            if (raw == null || raw.Count == 0)
            {
                Game1.drawObjectDialogue("The cart has nothing for sale today.");
                return;
            }

            double markup = _markup();
            var stock = new Dictionary<ISalable, ItemStockInformation>();
            foreach (KeyValuePair<ISalable, ItemStockInformation> pair in raw)
            {
                // Skip only entries the mail-order path genuinely can't handle: non-Items (nothing to
                // mail/identify), recipes (the wrapper isn't an Item so the decorator never learns the
                // recipe), and the catalog book itself (mustn't be orderable through itself). Every
                // other real item the cart stocks is mailable: the delivery letter uses the universal
                // "%item id" token, which honors any qualified id — (O)(BC)(F)(H)(W)(B)(S/P)(R) — so the
                // earlier (O)/(BC)/(F)-only gate that silently dropped cart-sold hats/weapons/boots/
                // rings/clothing is gone.
                if (pair.Key is not Item item) continue;
                if (item.IsRecipe) continue;
                if (item.QualifiedItemId == BookItem.QualifiedId) continue;

                int orderPrice = CartCatalog.Core.Pricing.OrderPrice(pair.Value.Price, markup);
                var wrapped = new MailOrderSalable(pair.Key, orderPrice);
                stock[wrapped] = new ItemStockInformation(orderPrice, pair.Value.Stock);
            }

            if (stock.Count == 0)
            {
                Game1.drawObjectDialogue("The cart has nothing for sale today.");
                return;
            }

            var menu = new ShopMenu(TravelerShopId, stock, on_purchase: this.OnPurchase);
            // Make the catalog strictly buy-only. ShopMenu seeds categoriesToSellHere from the
            // location the menu opens in (SeedShop, FishShop, Blacksmith, ...), so opening the catalog
            // while standing in a shop would let the player sell that shop's categories to the cart
            // from anywhere. The cart is mail-order purchase only — clear the sell categories.
            menu.categoriesToSellHere.Clear();
            Game1.activeClickableMenu = menu;
            _monitor.Log($"Cart Catalog opened with {stock.Count} item(s).", LogLevel.Info);
        }

        /// <summary>ShopMenu purchase callback. Fires after actionWhenPurchased (which suppresses the
        /// grant) and carries the real number of "buys" (stockToBuy); unwrap the salable and queue the
        /// real item quantity (countTaken * the wrapped item's per-unit stack). Returns false so the
        /// shop stays open.</summary>
        private bool OnPurchase(ISalable salable, Farmer who, int countTaken, ItemStockInformation stock)
        {
            if (salable is MailOrderSalable wrapped && wrapped.Inner is Item)
                _onOrder(wrapped.Inner, countTaken * wrapped.UnitStack);
            return false; // do not force-close the shop
        }
    }
}
