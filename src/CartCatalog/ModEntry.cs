using CartCatalog.Catalog;
using CartCatalog.Core;
using CartCatalog.Delivery;
using CartCatalog.Integration;
using CartCatalog.Shop;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CartCatalog
{
    public sealed class ModEntry : Mod
    {
        private ModConfig _config;
        private BookItem _bookItem;
        private OrderStore _orders;
        private CatalogMenu _catalogMenu;

        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();
            // Write back so newly-added fields appear in config.json for the player to edit.
            helper.WriteConfig(_config);

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            _orders = new OrderStore(helper.Data);
            _ = new MailDelivery(helper, this.Monitor, _orders);
            _ = new BookShopInjection(helper, () => _config.BookPrice, () => _config.AppearanceChance);
            helper.Events.GameLoop.DayStarted += (_, _) => helper.GameContent.InvalidateCache("Data/Shops");
            _catalogMenu = new CatalogMenu(this.Monitor, () => _config.Markup, this.QueueOrder);
            _bookItem = new BookItem(helper, () => _catalogMenu.Open());

            var harmony = new Harmony(this.ModManifest.UniqueID);
            harmony.PatchAll();

            // Arm the porch-package pickup patch (the patch stays inert until a flagged package
            // exists; PorchDelivery, which places one, is wired in a later task).
            Catalog.Object_checkForAction_PackagePatch.Orders = _orders;
            Catalog.Object_checkForAction_PackagePatch.Monitor = this.Monitor;

            helper.Events.GameLoop.SaveLoaded += (_, _) =>
            {
                _orders.Load();
                // Rebuild the book's cart entry for this save (PlayerOwnsBook gate), and re-inject
                // persisted, not-yet-read letters into Data/mail so a save/quit before reading the
                // mail doesn't lose the item (the body isn't stored in the vanilla save).
                helper.GameContent.InvalidateCache("Data/Shops");
                helper.GameContent.InvalidateCache("Data/mail");
            };
            helper.Events.GameLoop.Saving += (_, _) => _orders.Save();

            helper.ConsoleCommands.Add("cc_givebook",
                "Give the Cart Catalog book to the player.", this.CmdGiveBook);
            helper.ConsoleCommands.Add("cc_orders",
                "List the player's pending Cart Catalog orders.", this.CmdListOrders);
            helper.ConsoleCommands.Add("cc_open",
                "Open the Cart Catalog menu (requires a loaded save).", this.CmdOpen);

            this.Monitor.Log(
                $"Cart Catalog loaded. Markup x{_config.Markup}, delivery {_config.DeliveryDelayDays}d, " +
                $"book {_config.BookPrice}g, appearance {_config.AppearanceChance:P0}.",
                LogLevel.Info);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var gmcm = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (gmcm == null)
            {
                this.Monitor.Log("GMCM not installed — config edits via config.json only.", LogLevel.Trace);
                return;
            }

            gmcm.Register(this.ModManifest,
                reset: () => _config = new ModConfig(),
                save: () => this.Helper.WriteConfig(_config));

            gmcm.AddSectionTitle(this.ModManifest, () => "Cart Catalog");
            gmcm.AddNumberOption(this.ModManifest,
                getValue: () => (float)_config.Markup, setValue: v => _config.Markup = v,
                name: () => "Shipping markup", tooltip: () => "Price multiplier per ordered item (1.05 = +5%).",
                min: 1.0f, max: 2.0f, interval: 0.01f);
            gmcm.AddNumberOption(this.ModManifest,
                getValue: () => _config.DeliveryDelayDays, setValue: v => _config.DeliveryDelayDays = v,
                name: () => "Delivery delay (days)", tooltip: () => "Days until ordered items arrive (1 = next morning).",
                min: 1, max: 7, interval: 1);
            gmcm.AddNumberOption(this.ModManifest,
                getValue: () => _config.BookPrice, setValue: v => _config.BookPrice = v,
                name: () => "Book price (gold)", tooltip: () => "Cost of the Cart Catalog book in the Traveling Cart.",
                min: 0, max: 50000, interval: 250);
            gmcm.AddNumberOption(this.ModManifest,
                getValue: () => (float)_config.AppearanceChance, setValue: v => _config.AppearanceChance = v,
                name: () => "Book appearance chance", tooltip: () => "Chance the book appears each cart visit, until owned.",
                min: 0f, max: 1f, interval: 0.05f);

            this.Monitor.Log("Registered GMCM options.", LogLevel.Info);
        }

        private void CmdGiveBook(string command, string[] args)
        {
            if (!Context.IsWorldReady) { this.Monitor.Log("Load a save first.", LogLevel.Warn); return; }
            var book = StardewValley.ItemRegistry.Create(BookItem.QualifiedId);
            StardewValley.Game1.player.addItemByMenuIfNecessary(book);
            this.Monitor.Log("Gave Cart Catalog book.", LogLevel.Info);
        }

        private void CmdListOrders(string command, string[] args)
        {
            this.Monitor.Log($"Pending orders: {_orders.Orders.Count}", LogLevel.Info);
            foreach (var o in _orders.Orders)
                this.Monitor.Log($"  {o.Quantity}x {o.ItemId} -> day {o.DeliveryDay}", LogLevel.Info);
        }

        private void CmdOpen(string command, string[] args)
        {
            if (!Context.IsWorldReady) { this.Monitor.Log("Load a save first.", LogLevel.Warn); return; }
            _catalogMenu.Open();
        }

        /// <summary>Queue one unit of the purchased item for next-day mail delivery. Gold was already
        /// deducted by the ShopMenu using the marked-up salable price.</summary>
        private void QueueOrder(ISalable inner, int quantity)
        {
            if (inner is not Item item)
            {
                this.Monitor.Log($"Cannot mail-order non-item salable '{inner?.DisplayName}'.", LogLevel.Warn);
                return;
            }

            int deliveryDay = Game1.Date.TotalDays + _config.DeliveryDelayDays;
            _orders.Add(new PendingOrder
            {
                ItemId = item.QualifiedItemId,
                Quantity = quantity,
                DeliveryDay = deliveryDay,
            });
            this.Monitor.Log(
                $"Queued {quantity}x {item.DisplayName} for delivery on day {deliveryDay} " +
                $"(today {Game1.Date.TotalDays}).", LogLevel.Info);
        }
    }
}
