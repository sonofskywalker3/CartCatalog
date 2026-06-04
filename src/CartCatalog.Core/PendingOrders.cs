using System.Collections.Generic;

namespace CartCatalog.Core
{
    /// <summary>Pure partitioning of queued orders into "deliver now" vs "still pending".</summary>
    public static class PendingOrders
    {
        /// <summary>Split <paramref name="orders"/> into those due (<c>DeliveryDay &lt;= today</c>)
        /// and those still pending. Neither input nor inputs' elements are mutated.</summary>
        public static (List<PendingOrder> due, List<PendingOrder> remaining) Due(
            IEnumerable<PendingOrder> orders, int today)
        {
            var due = new List<PendingOrder>();
            var remaining = new List<PendingOrder>();
            foreach (PendingOrder o in orders)
            {
                if (o.DeliveryDay <= today) due.Add(o);
                else remaining.Add(o);
            }
            return (due, remaining);
        }
    }
}
