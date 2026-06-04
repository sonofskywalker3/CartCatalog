using System.Collections.Generic;
using CartCatalog.Core;
using StardewModdingAPI;
using StardewValley;

namespace CartCatalog.Delivery
{
    /// <summary>One-time recovery for players updating from the mailbox-letter versions: any Cart
    /// Catalog letter still UNREAD in the mailbox would be lost once the mod stopped injecting letter
    /// bodies. We read the legacy "delivered-letters" save data, re-queue each unread letter's items as
    /// immediately-due porch orders, and drop the dead mailbox key. Recovery happens on load (into the
    /// in-memory OrderStore) but is only marked done + the legacy data cleared on the next Saving — so a
    /// quit before saving safely re-runs it rather than losing items (nothing persisted = no
    /// duplication).</summary>
    internal static class MailMigration
    {
        private const string LegacyLettersKey = "delivered-letters";
        private const string MigratedFlagKey = "letters-migrated";

        /// <summary>Recover unread legacy letters into the order store. No-op once committed.</summary>
        public static void RecoverOnLoad(IModHelper helper, IMonitor monitor, OrderStore orders)
        {
            if (helper.Data.ReadSaveData<string>(MigratedFlagKey) == "1") return;

            var letters = helper.Data.ReadSaveData<Dictionary<string, string>>(LegacyLettersKey);
            if (letters == null || letters.Count == 0) return;

            int recovered = 0;
            foreach (KeyValuePair<string, string> kv in letters)
            {
                if (Game1.player.mailReceived.Contains(kv.Key)) continue;   // already read → already granted
                foreach (PendingOrder o in LetterItemParser.Parse(kv.Value))
                {
                    orders.Add(o);
                    recovered++;
                }
                Game1.player.mailbox.Remove(kv.Key);   // drop the now-bodyless letter
            }

            if (recovered > 0)
                monitor.Log($"Recovered {recovered} item(s) from {letters.Count} legacy Cart Catalog letter(s) " +
                            "into a porch delivery.", LogLevel.Info);
        }

        /// <summary>Mark the migration done and clear the legacy data. Call on Saving, AFTER the order
        /// store is saved, so the recovered orders and the done flag persist together.</summary>
        public static void CommitOnSave(IModHelper helper)
        {
            if (helper.Data.ReadSaveData<string>(MigratedFlagKey) == "1") return;
            helper.Data.WriteSaveData(MigratedFlagKey, "1");
            helper.Data.WriteSaveData<Dictionary<string, string>>(LegacyLettersKey, null);
        }
    }
}
