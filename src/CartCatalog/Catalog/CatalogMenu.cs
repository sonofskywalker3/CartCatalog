using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Internal;
using StardewValley.Menus;

namespace CartCatalog.Catalog
{
    /// <summary>Builds a vanilla ShopMenu over today's Traveling Cart stock, with marked-up prices and
    /// purchases routed into next-day mail orders instead of being granted immediately.</summary>
    internal sealed class CatalogMenu
    {
        private const string TravelerShopId = "Traveler";

        private readonly IMonitor _monitor;
        private readonly Func<double> _markup;
        private readonly Action<ISalable, int> _onOrder;
        private ShopMenu _menu;

        /// <summary>The currently-open catalog ShopMenu instance, or null. Used by the
        /// actionWhenPurchased Harmony patch to suppress the immediate item grant ONLY for our menu
        /// (the real Traveling Cart shares the "Traveler" ShopId, so we must match by instance).</summary>
        internal static ShopMenu ActiveCatalog { get; private set; }

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
                // mail/identify), recipes (the player would need to learn them, not receive an item), and
                // the catalog book itself (mustn't be orderable through itself). Every other real item the
                // cart stocks is mailable: the delivery letter uses the universal "%item id" token, which
                // handles any qualified id — (O)(BC)(F)(H)(W)(B)(S/P)(R).
                if (pair.Key is not Item item) continue;
                if (item.IsRecipe) continue;
                if (item.QualifiedItemId == BookItem.QualifiedId) continue;

                // Use the REAL cart item as the salable (NOT a custom ISalable wrapper). Android's
                // ShopMenu.draw casts the selected salable to Item — `(currentItem as Item).drawInMenu(...)`
                // — and would NRE on a non-Item wrapper the moment you navigate the list. The marked-up
                // shipping price is carried by ItemStockInformation.Price (what the shop displays and
                // charges); the grant is suppressed and the order queued in OnPurchase.
                //
                // COPY the original ItemStockInformation and override only Price. We must preserve
                // SyncedKey + LimitedStockMode (and ItemToSyncStack) so purchases persist: ShopMenu
                // records limited-stock buys via team.synchronizedShopStock keyed by SyncedKey, and
                // GetShopStock subtracts them on the next open. Rebuilding with just (price, stock)
                // dropped SyncedKey, so buys recorded under a null key the reopen couldn't match — the
                // catalog "restocked" every time. Preserving it also shares stock with the real cart.
                ItemStockInformation src = pair.Value;
                int orderPrice = CartCatalog.Core.Pricing.OrderPrice(src.Price, markup);
                stock[item] = new ItemStockInformation(orderPrice, src.Stock, src.TradeItem,
                    src.TradeItemCount, src.LimitedStockMode, src.SyncedKey, src.ItemToSyncStack,
                    src.StackDrawType, src.ActionsOnPurchase);
            }

            if (stock.Count == 0)
            {
                Game1.drawObjectDialogue("The cart has nothing for sale today.");
                return;
            }

            _menu = new ShopMenu(TravelerShopId, stock, on_purchase: this.OnPurchase);
            // Make the catalog strictly buy-only. ShopMenu seeds categoriesToSellHere from the
            // location the menu opens in (SeedShop, FishShop, Blacksmith, ...), so opening the catalog
            // while standing in a shop would let the player sell that shop's categories to the cart
            // from anywhere. The cart is mail-order purchase only — clear the sell categories.
            _menu.categoriesToSellHere.Clear();
            ActiveCatalog = _menu;
            Game1.activeClickableMenu = _menu;
            _monitor.Log($"Cart Catalog opened with {stock.Count} item(s).", LogLevel.Info);
        }

        /// <summary>ShopMenu purchase callback. The player has been charged the marked-up price; queue
        /// the item as a next-day mail order. The immediate inventory grant is suppressed by the
        /// actionWhenPurchased Harmony patch (scoped to <see cref="ActiveCatalog"/>), which both the
        /// vanilla ShopMenu and AndroidConsolizer's A-button purchase path honor. Returns false so the
        /// shop stays open.</summary>
        private bool OnPurchase(ISalable salable, Farmer who, int countTaken, ItemStockInformation stock)
        {
            if (salable is Item item)
                _onOrder(item, countTaken * item.Stack);
            return false; // do not force-close the shop
        }
    }
}
