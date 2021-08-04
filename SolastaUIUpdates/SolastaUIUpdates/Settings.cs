using UnityModManagerNet;

namespace SolastaUIUpdates
{
    public class Core
    {

    }
    
    public class Settings : UnityModManager.ModSettings
    {
        public const InputCommands.Id CTRL_C = (InputCommands.Id)44440000;
        public const InputCommands.Id CTRL_M = (InputCommands.Id)44440001;
        public const InputCommands.Id CTRL_P = (InputCommands.Id)44440002;

        public int MaxSpellLevelsPerLine = 5;
        public float SpellPanelGapBetweenLines = 30f;
    }
}