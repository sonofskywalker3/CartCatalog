using System.Collections.Generic;

namespace CartCatalog.Core
{
    /// <summary>Merges orders that share a qualified item id into a single summed stack, preserving
    /// first-seen order. Pure, kept in Core so it can be unit-tested without the game/SMAPI assemblies.
    /// Used to consolidate a delivery day's orders into one porch package.</summary>
    public static class OrderMerge
    {
        /// <summary>Sum the quantities of same-id orders, preserving first-seen order. Each merged
        /// order carries the summed Quantity; DeliveryDay is taken from that id's first-seen order
        /// (irrelevant to collection, kept deterministic).</summary>
        public static List<PendingOrder> Merge(IEnumerable<PendingOrder> orders)
        {
            var order = new List<string>();
            var totals = new Dictionary<string, int>();
            var firstDay = new Dictionary<string, int>();
            foreach (PendingOrder o in orders)
            {
                if (!totals.ContainsKey(o.ItemId))
                {
                    order.Add(o.ItemId);
                    firstDay[o.ItemId] = o.DeliveryDay;
                }
                totals[o.ItemId] = totals.TryGetValue(o.ItemId, out int cur) ? cur + o.Quantity : o.Quantity;
            }

            var merged = new List<PendingOrder>();
            foreach (string id in order)
                merged.Add(new PendingOrder { ItemId = id, Quantity = totals[id], DeliveryDay = firstDay[id] });
            return merged;
        }
    }
}
