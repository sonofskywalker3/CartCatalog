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

            int n = 0;
            foreach (PendingOrder order in due)
            {
                string key = MailKeyPrefix + today + "_" + (n++);
                _orders.Letters[key] = BuildLetter(order);
                if (!Game1.player.mailbox.Contains(key))
                    Game1.player.mailbox.Add(key);
            }

            _orders.Replace(remaining);
            _helper.GameContent.InvalidateCache(MailAsset);
            _monitor.Log("Delivered " + due.Count + " Cart Catalog order(s) to the mailbox.", LogLevel.Info);
        }

        /// <summary>Drop letters the player has already read (key now in mailReceived) so the
        /// persisted map doesn't grow without bound.</summary>
        private void PruneReadLetters()
        {
            if (!Context.IsWorldReady || _orders.Letters.Count == 0) return;
            List<string> read = _orders.Letters.Keys.Where(k => Game1.player.mailReceived.Contains(k)).ToList();
            foreach (string k in read) _orders.Letters.Remove(k);
        }

        /// <summary>Build the letter body. Delegates to <see cref="MailLetter.Build"/>, which uses
        /// the universal "%item id" token so quantity is honored for every mailable item type — not
        /// just objects. (The old per-type "bigobject"/"furniture" tokens ignored count.)</summary>
        private static string BuildLetter(PendingOrder order)
            => MailLetter.Build(order.ItemId, order.Quantity);

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
