using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace CartCatalog.Catalog
{
    /// <summary>Wraps a Traveling Cart salable so the shop shows the marked-up shipping price and,
    /// on purchase, queues a next-day mail order instead of granting the item immediately.
    /// All display/identity members delegate to the wrapped item.</summary>
    internal sealed class MailOrderSalable : ISalable
    {
        private readonly ISalable _inner;
        private readonly int _orderPrice;
        private readonly int _unitStack;

        public MailOrderSalable(ISalable inner, int orderPrice)
        {
            _inner = inner;
            _orderPrice = orderPrice;
            // Capture the per-unit stack at wrap time: ShopMenu grants stockToBuy * item.Stack, so a
            // cart entry that bundles N units per "buy" (Stack > 1) must order countTaken * N. Captured
            // here because the shop may mutate the live Stack during the transaction.
            _unitStack = inner.Stack;
        }

        /// <summary>The wrapped Traveling Cart salable, so the shop's on_purchase callback can unwrap
        /// it and queue the real order against the underlying item.</summary>
        public ISalable Inner => _inner;

        /// <summary>The wrapped salable's per-unit stack size as it was when wrapped — used to convert
        /// the shop's "count of buys" into the real item quantity to mail.</summary>
        public int UnitStack => _unitStack;

        // --- price (marked up) ---
        public int salePrice(bool ignoreProfitMargins = false) => _orderPrice;
        public int sellToStorePrice(long specificPlayerID = -1L) => _inner.sellToStorePrice(specificPlayerID);
        public bool appliesProfitMargins() => _inner.appliesProfitMargins();

        // --- purchase: do NOT add to inventory (return true) ---
        // ShopMenu clears heldItem when this returns true, so the item is never granted to the
        // player. The actual order (with the real count bought) is queued by the ShopMenu's
        // on_purchase callback, which fires AFTER this and carries stockToBuy.
        public bool actionWhenPurchased(string shopId) => true;

        // --- identity / display: delegate to the wrapped item ---
        public string TypeDefinitionId => _inner.TypeDefinitionId;
        public string QualifiedItemId => _inner.QualifiedItemId;
        public string DisplayName => _inner.DisplayName;
        public string Name => _inner.Name;
        public bool IsRecipe { get => _inner.IsRecipe; set => _inner.IsRecipe = value; }
        public int Stack { get => _inner.Stack; set => _inner.Stack = value; }
        public int Quality { get => _inner.Quality; set => _inner.Quality = value; }

        public string GetItemTypeId() => _inner.GetItemTypeId();
        public string getDescription() => _inner.getDescription();
        public int maximumStackSize() => _inner.maximumStackSize();
        public int addToStack(Item stack) => _inner.addToStack(stack);
        public bool canStackWith(ISalable other) => false; // each order is distinct in the menu
        public bool CanBuyItem(Farmer farmer) => _inner.CanBuyItem(farmer);
        public bool IsInfiniteStock() => _inner.IsInfiniteStock();
        public ISalable GetSalableInstance() => this;
        public void FixStackSize() => _inner.FixStackSize();
        public void FixQuality() => _inner.FixQuality();
        public bool ShouldDrawIcon() => _inner.ShouldDrawIcon();

        public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize,
            float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
            => _inner.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth,
                drawStackNumber, color, drawShadow);
    }
}
