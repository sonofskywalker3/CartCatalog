using System.Collections.Generic;
using System.Linq;
using CartCatalog.Core;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CartCatalog.Delivery
{
    /// <summary>On each DayStarted, delivers due orders as mailbox letters with item attachments.
    /// Letter bodies are stored in the persisted OrderStore so they survive a save/quit before the
    /// player reads the mail; the Data/mail edit is rebuilt from that store on every load.</summary>
    internal sealed class MailDelivery
    {
        private const string MailKeyPrefix = "CartCatalog_Order_";
        private const string MailAsset = "Data/mail";

        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly OrderStore _orders;

        public MailDelivery(IModHelper helper, IMonitor monitor, OrderStore orders)
        {
            _helper = helper;
            _monitor = monitor;
            _orders = orders;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            PruneReadLetters();

            int today = Game1.Date.TotalDays;
            var (due, remaining) = PendingOrders.Due(_orders.Orders, today);
            if (due.Count == 0) return;

            // All of a day's orders arrive in ONE letter: each distinct item is its own "%item id"
            // attachment and same-item orders are merged into a single stack (handled by MailLetter).
            // One mail key per delivery day (TotalDays is unique), so no per-order suffix is needed.
            string key = MailKeyPrefix + today;
            _orders.Letters[key] = MailLetter.Build(due.Select(o => (o.ItemId, o.Quantity)));
            if (!Game1.player.mailbox.Contains(key))
                Game1.player.mailbox.Add(key);

            _orders.Replace(remaining);
            _helper.GameContent.InvalidateCache(MailAsset);
            _monitor.Log($"Delivered {due.Count} Cart Catalog order(s) in one letter.", LogLevel.Info);
        }

        /// <summary>Drop letters the player has already read (key now in mailReceived) so the
        /// persisted map doesn't grow without bound.</summary>
        private void PruneReadLetters()
        {
            if (!Context.IsWorldReady || _orders.Letters.Count == 0) return;
            List<string> read = _orders.Letters.Keys.Where(k => Game1.player.mailReceived.Contains(k)).ToList();
            foreach (string k in read) _orders.Letters.Remove(k);
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo(MailAsset)) return;
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, string>().Data;
                foreach (KeyValuePair<string, string> kv in _orders.Letters)
                    data[kv.Key] = kv.Value;
            }, AssetEditPriority.Default);
        }
    }
}
