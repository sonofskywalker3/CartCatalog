using System;
using StardewModdingAPI;

namespace CartCatalog.Integration
{
    /// <summary>Subset of the Generic Mod Config Menu API. Declared locally so the project compiles
    /// without a hard dependency on GMCM; resolved at runtime via Helper.ModRegistry.GetApi.
    /// Reference: https://github.com/spacechase0/StardewValleyMods/blob/develop/GenericModConfigMenu/IGenericModConfigMenuApi.cs</summary>
    public interface IGenericModConfigMenuApi
    {
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);
        void AddSectionTitle(IManifest mod, Func<string> text, Func<string> tooltip = null);
        void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue,
            Func<string> name, Func<string> tooltip = null, int? min = null, int? max = null,
            int? interval = null, Func<int, string> formatValue = null, string fieldId = null);
        void AddNumberOption(IManifest mod, Func<float> getValue, Action<float> setValue,
            Func<string> name, Func<string> tooltip = null, float? min = null, float? max = null,
            float? interval = null, Func<float, string> formatValue = null, string fieldId = null);
    }
}
