
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SolastaUIUpdates.Patches
{
    class PowerSelectionPanelPatcher
    {
        private static RectTransform secondRow;
        private static RectTransform thirdRow;

        [HarmonyPatch(typeof(PowerSelectionPanel), "Bind")]
        internal static class PowerSelectionPanel_SecondLine
        {
            internal static void Postfix(PowerSelectionPanel __instance)
            {
                List<UsablePowerBox> powerBoxes = (List<UsablePowerBox>)Traverse.Create(__instance).Field("usablePowerBoxes").GetValue();
                RectTransform powersTable = (RectTransform)Traverse.Create(__instance).Field("powersTable").GetValue();
                if (powerBoxes.Count > 14)
                {
                    if (thirdRow == null)
                    {
                        thirdRow = GameObject.Instantiate(powersTable);
                    }
                    thirdRow.gameObject.SetActive(true);
                    thirdRow.DetachChildren();
                    thirdRow.SetParent(powersTable.parent.transform, true);
                    thirdRow.transform.position = new Vector3(powersTable.transform.position.x, powersTable.transform.position.y + 200, powersTable.transform.position.z);
                    int toStayCount = powersTable.childCount * 2 / 3;
                    for (int i = powersTable.childCount - 1; i > toStayCount; i--)
                    {
                        powersTable.GetChild(i).SetParent(thirdRow, false);
                    }
                    LayoutRebuilder.ForceRebuildLayoutImmediate(thirdRow);
                }
                if (powerBoxes.Count > 7)
                {
                    if (secondRow == null)
                    {
                        secondRow = GameObject.Instantiate(powersTable);
                    }
                    secondRow.gameObject.SetActive(true);
                    secondRow.DetachChildren();
                    secondRow.SetParent(powersTable.parent.transform, true);
                    secondRow.transform.position = new Vector3(powersTable.transform.position.x, powersTable.transform.position.y + 80, powersTable.transform.position.z);
                    int toStayCount = powersTable.childCount / 2;
                    for (int i = powersTable.childCount - 1; i > toStayCount; i--)
                    {
                        powersTable.GetChild(i).SetParent(secondRow, false);
                    }

                    float height = __instance.transform.parent.GetComponent<RectTransform>().rect.height;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(powersTable);
                    __instance.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, powersTable.rect.width);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(secondRow);
                }
            }
        }

        [HarmonyPatch(typeof(PowerSelectionPanel), "Unbind")]
        internal static class PowerSelectionPanel_SecondLineUnbind
        {
            internal static void Postfix(PowerSelectionPanel __instance)
            {
                if (secondRow != null && secondRow.gameObject.activeSelf)
                {
                    Gui.ReleaseChildrenToPool(secondRow);
                    secondRow.gameObject.SetActive(false);
                }

                if (thirdRow != null && thirdRow.gameObject.activeSelf)
                {
                    Gui.ReleaseChildrenToPool(thirdRow);
                    thirdRow.gameObject.SetActive(false);
                }
            }
        }
    }
}
