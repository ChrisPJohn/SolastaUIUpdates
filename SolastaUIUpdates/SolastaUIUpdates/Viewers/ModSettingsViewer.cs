using UnityModManagerNet;
using ModKit;

namespace SolastaUIUpdates.Viewers
{
    public class ModSettingsViewer : IMenuSelectablePage
    {
        public string Name => "Settings";

        public int Priority => 1;

        private static void DisplaySpellPanelSettings()
        {
            UI.Label("");
            UI.Label("Spells Panel".yellow().bold());

            int intValue = Main.Settings.MaxSpellLevelsPerLine;
            if (UI.Slider("Max Levels per line", ref intValue, 3, 7, 5, "", UI.AutoWidth()))
            {
                Main.Settings.MaxSpellLevelsPerLine = intValue;
            }

            float floatValue = Main.Settings.SpellPanelGapBetweenLines;
            if (UI.Slider("Gap between spell lines", ref floatValue, 0f, 200f, 50f, 0, "", UI.AutoWidth()))
            {
                Main.Settings.SpellPanelGapBetweenLines = floatValue;
            }
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Main.Mod == null) return;

            UI.Label("Welcome to UI Updates".yellow().bold());

            DisplaySpellPanelSettings();
        }
    }
}