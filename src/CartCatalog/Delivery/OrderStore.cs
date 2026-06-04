using System.Collections.Generic;
using CartCatalog.Core;
using StardewModdingAPI;

namespace CartCatalog.Delivery
{
    /// <summary>Holds the pending mail orders AND the bodies of delivered-but-maybe-unread letters
    /// for the loaded save, persisted as per-save data (committed on Saving, loaded on SaveLoaded).
    /// Letters must be persisted so a save/quit before the player reads the mail doesn't lose the
    /// item: the Data/mail edit is rebuilt from this map on load.</summary>
    internal sealed class OrderStore
    {
        private const string OrdersKey = "pending-orders";
        private const string LettersKey = "delivered-letters";
        private readonly IDataHelper _data;

        public List<PendingOrder> Orders { get; private set; } = new();

        /// <summary>Mail key -> letter text (incl. %item attachment) for delivered letters not yet
        /// confirmed read. Pruned once the key appears in the farmer's mailReceived.</summary>
        public Dictionary<string, string> Letters { get; private set; } = new();

        public OrderStore(IDataHelper data) => _data = data;

        public void Load()
        {
            Orders = _data.ReadSaveData<List<PendingOrder>>(OrdersKey) ?? new List<PendingOrder>();
            Letters = _data.ReadSaveData<Dictionary<string, string>>(LettersKey) ?? new Dictionary<string, string>();
        }

        public void Save()
        {
            _data.WriteSaveData(OrdersKey, Orders);
            _data.WriteSaveData(LettersKey, Letters);
        }

        public void Add(PendingOrder order) => Orders.Add(order);

        /// <summary>Replace the pending list (used after delivery removes the due orders).</summary>
        public void Replace(List<PendingOrder> remaining) => Orders = remaining;
    }
}
