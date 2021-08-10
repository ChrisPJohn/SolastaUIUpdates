using HarmonyLib;
using UnityEngine;

namespace SolastaUIUpdates.Patches
{
    class GameManagerPatcher
    {
        [HarmonyPatch(typeof(GameManager), "BindPostDatabase")]
        internal static class GameManager_BindPostDatabase_Patch
        {
            internal static void Postfix()
            {
                ServiceRepository.GetService<IInputService>().RegisterCommand(Settings.CTRL_C, (int)KeyCode.C, (int)KeyCode.LeftControl, -1, -1, -1, -1);
                ServiceRepository.GetService<IInputService>().RegisterCommand(Settings.CTRL_L, (int)KeyCode.L, (int)KeyCode.LeftControl, -1, -1, -1, -1);
                ServiceRepository.GetService<IInputService>().RegisterCommand(Settings.CTRL_M, (int)KeyCode.M, (int)KeyCode.LeftControl, -1, -1, -1, -1);
                ServiceRepository.GetService<IInputService>().RegisterCommand(Settings.CTRL_P, (int)KeyCode.P, (int)KeyCode.LeftControl, -1, -1, -1, -1);
            }
        }
    }
}