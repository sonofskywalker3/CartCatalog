using System.Collections.Generic;
using CartCatalog.Core;
using StardewModdingAPI;

namespace CartCatalog.Delivery
{
    /// <summary>Holds the pending Cart Catalog orders for the loaded save, persisted as per-save data
    /// (committed on Saving, loaded on SaveLoaded). Orders are the single source of truth; the porch
    /// package is rebuilt from this store on load and never serialized into the farm save.</summary>
    internal sealed class OrderStore
    {
        private const string OrdersKey = "pending-orders";
        private readonly IDataHelper _data;

        public List<PendingOrder> Orders { get; private set; } = new();

        public OrderStore(IDataHelper data) => _data = data;

        public void Load()
            => Orders = _data.ReadSaveData<List<PendingOrder>>(OrdersKey) ?? new List<PendingOrder>();

        public void Save() => _data.WriteSaveData(OrdersKey, Orders);

        public void Add(PendingOrder order) => Orders.Add(order);

        /// <summary>Replace the pending list (used after collection removes the due orders).</summary>
        public void Replace(List<PendingOrder> remaining) => Orders = remaining;
    }
}
