﻿

using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SolastaUIUpdates.Patches
{
    class SpellSelectionPanelPatcher
    {
		private static readonly int MAX_LEVELS_PER_LINE = 5;
		private static List<RectTransform> spellLineTables = new List<RectTransform>();

		[HarmonyPatch(typeof(SpellSelectionPanel), "Bind")]
		internal static class SpellSelectionPanel_SecondLine
		{
			internal static void Postfix(SpellSelectionPanel __instance, GameLocationCharacter caster, SpellSelectionPanel.SpellcastCancelledHandler spellcastCancelled, SpellsByLevelBox.SpellCastEngagedHandler spellCastEngaged, ActionDefinitions.ActionType actionType, bool cantripOnly)
			{
				List<SpellRepertoireLine> spellRepertoireLines = (List<SpellRepertoireLine>)Traverse.Create(__instance).Field("spellRepertoireLines").GetValue();
				SpellRepertoireLine spellRepertoireSecondaryLine = (SpellRepertoireLine)Traverse.Create(__instance).Field("spellRepertoireSecondaryLine").GetValue();
				RectTransform spellRepertoireLinesTable = (RectTransform)Traverse.Create(__instance).Field("spellRepertoireLinesTable").GetValue();
				SlotAdvancementPanel slotAdvancementPanel = (SlotAdvancementPanel)Traverse.Create(__instance).Field("slotAdvancementPanel").GetValue();
				foreach (SpellRepertoireLine spellRepertoireLine in spellRepertoireLines)
				{
					spellRepertoireLine.Unbind();
				}
				spellRepertoireLines.Clear();
				Gui.ReleaseChildrenToPool(spellRepertoireLinesTable);
				spellRepertoireSecondaryLine.Unbind();
				spellRepertoireSecondaryLine.gameObject.SetActive(false);

				if (spellRepertoireLinesTable.parent.GetChild(0).GetComponent<VerticalLayoutGroup>() == null)
                {
					GameObject spellLineHolder = new GameObject();
					VerticalLayoutGroup vertGroup = spellLineHolder.AddComponent<VerticalLayoutGroup>();
					vertGroup.spacing = 30f;
					spellLineHolder.transform.SetParent(spellRepertoireLinesTable.parent);
					spellLineHolder.transform.SetAsFirstSibling();
					spellRepertoireLinesTable.SetParent(spellLineHolder.transform);
				}

				List<RulesetSpellRepertoire> spellRepertoires = __instance.Caster.RulesetCharacter.SpellRepertoires;
				int spellLevels = 0;
				using (List<RulesetSpellRepertoire>.Enumerator enumerator = spellRepertoires.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						RulesetSpellRepertoire rulesetSpellRepertoire = enumerator.Current;
						spellLevels += ActiveSpellLevelsForRepetoire(rulesetSpellRepertoire, actionType);
					}
				}
				
				spellRepertoireLines.Clear();
				bool needNewLine = true;
				int lineIndex = 0;
				int indexOfLine = 0;
				int spellLevelsOnLine = 0;
				RectTransform curTable = spellRepertoireLinesTable;
				for (int repertoireIndex = 0; repertoireIndex < spellRepertoires.Count; repertoireIndex++)
				{
					RulesetSpellRepertoire rulesetSpellRepertoire = spellRepertoires[repertoireIndex];

					int startLevel = 0;
					for (int level = startLevel; level <= rulesetSpellRepertoire.MaxSpellLevelOfSpellCastingLevel; level++)
					{
						if (IsLevelActive(rulesetSpellRepertoire, level, actionType))
						{
							spellLevelsOnLine++;
							if (spellLevelsOnLine >= MAX_LEVELS_PER_LINE)
                            {
								curTable = AddActiveSpellsToLine(__instance, caster, spellCastEngaged, actionType, cantripOnly, spellRepertoireLines, curTable, slotAdvancementPanel, spellRepertoires, needNewLine, lineIndex, indexOfLine, rulesetSpellRepertoire, startLevel, level);
                                startLevel = level + 1;
                                lineIndex++;
                                spellLevelsOnLine = 0;
								needNewLine = true;
								indexOfLine = 0;

							}
                        }
					}
					if (spellLevelsOnLine != 0)
					{
						curTable = AddActiveSpellsToLine(__instance, caster, spellCastEngaged, actionType, cantripOnly, spellRepertoireLines, curTable, slotAdvancementPanel, spellRepertoires, needNewLine, lineIndex, indexOfLine, rulesetSpellRepertoire, startLevel, rulesetSpellRepertoire.MaxSpellLevelOfSpellCastingLevel);
						needNewLine = false;
						indexOfLine++;
					}
				}
				LayoutRebuilder.ForceRebuildLayoutImmediate(curTable);
				__instance.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, curTable.rect.width);
			}

            private static RectTransform AddActiveSpellsToLine(SpellSelectionPanel __instance, GameLocationCharacter caster,
				SpellsByLevelBox.SpellCastEngagedHandler spellCastEngaged, ActionDefinitions.ActionType actionType, bool cantripOnly,
				List<SpellRepertoireLine> spellRepertoireLines, RectTransform spellRepertoireLinesTable,
				SlotAdvancementPanel slotAdvancementPanel, List<RulesetSpellRepertoire> spellRepertoires, bool needNewLine, int lineIndex,
				int indexOfLine, RulesetSpellRepertoire rulesetSpellRepertoire, int startLevel, int level)
            {
                if (needNewLine)
                {
					RectTransform previousTable = spellRepertoireLinesTable;
					LayoutRebuilder.ForceRebuildLayoutImmediate(previousTable);
					__instance.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, previousTable.rect.width);

					if (lineIndex > 0)
                    {
						// instantiate new table
						spellRepertoireLinesTable = GameObject.Instantiate(spellRepertoireLinesTable);
						// clear it of children
						spellRepertoireLinesTable.DetachChildren();
						spellRepertoireLinesTable.SetParent(previousTable.parent.transform, true);
						spellRepertoireLinesTable.localScale = previousTable.localScale;
						spellRepertoireLinesTable.transform.SetAsFirstSibling();
						spellLineTables.Add(spellRepertoireLinesTable);
					}
				}

				SpellRepertoireLine curLine = SetUpNewLine(indexOfLine, spellRepertoireLinesTable, spellRepertoireLines, __instance);
                curLine.Bind(caster.RulesetCharacter, rulesetSpellRepertoire, spellRepertoires.Count > 1, spellCastEngaged, slotAdvancementPanel, actionType, cantripOnly, startLevel, level);
                return spellRepertoireLinesTable;
            }

            private static SpellRepertoireLine SetUpNewLine(int index, RectTransform spellRepertoireLinesTable, List<SpellRepertoireLine> spellRepertoireLines, SpellSelectionPanel __instance)
            {
				GameObject newLine;
				if (spellRepertoireLinesTable.childCount <= index)
                {
					newLine = Gui.GetPrefabFromPool((GameObject)Traverse.Create(__instance).Field("spellRepertoireLinePrefab").GetValue(),
						spellRepertoireLinesTable);
				} else
                {
					newLine = spellRepertoireLinesTable.GetChild(index).gameObject;
				}
				newLine.SetActive(true);
				SpellRepertoireLine component = newLine.GetComponent<SpellRepertoireLine>();
				spellRepertoireLines.Add(component);
				return component;
			}

			private static bool IsLevelActive(RulesetSpellRepertoire spellRepertoire, int level, ActionDefinitions.ActionType actionType)
            {
				RuleDefinitions.ActivationTime spellActivationTime = RuleDefinitions.ActivationTime.Action;
				switch (actionType)
                {
					case ActionDefinitions.ActionType.Bonus:
						spellActivationTime = RuleDefinitions.ActivationTime.BonusAction;
						break;
					case ActionDefinitions.ActionType.Main:
						spellActivationTime = RuleDefinitions.ActivationTime.Action;
						break;
					case ActionDefinitions.ActionType.Reaction:
						spellActivationTime = RuleDefinitions.ActivationTime.Reaction;
						break;
					case ActionDefinitions.ActionType.NoCost:
						spellActivationTime = RuleDefinitions.ActivationTime.NoCost;
						break;

				}
				if (level == 0)
                {
					foreach (SpellDefinition cantrip in spellRepertoire.KnownCantrips)
                    {
						if (cantrip.ActivationTime == spellActivationTime)
                        {
							return true;
                        }

					}
					return false;
				}

				if (spellRepertoire.SpellCastingFeature.SpellReadyness == RuleDefinitions.SpellReadyness.Prepared)
				{
					using (List<SpellDefinition>.Enumerator enumerator = spellRepertoire.PreparedSpells.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							SpellDefinition spellDefinition = enumerator.Current;
							if (spellDefinition.SpellLevel == level && spellDefinition.ActivationTime == spellActivationTime)
							{
								return true;
							}
						}
					}
				}
				if (spellRepertoire.SpellCastingFeature.SpellReadyness == RuleDefinitions.SpellReadyness.AllKnown)
				{
					foreach (SpellDefinition spellDefinition2 in spellRepertoire.KnownSpells)
					{
						if (spellDefinition2.SpellLevel == level)
						{
							return true;
						}
					}
				}

				return false;
			}

			private static int ActiveSpellLevelsForRepetoire(RulesetSpellRepertoire spellRepertoire, ActionDefinitions.ActionType actionType)
            {
				int activeSpellLevels = 0;
				for (int level = 0; level < spellRepertoire.MaxSpellLevelOfSpellCastingLevel; level++)
				{
					if (IsLevelActive(spellRepertoire, level, actionType))
					{
						activeSpellLevels++;
					}
				}
				return activeSpellLevels;
			}
		}

        [HarmonyPatch(typeof(SpellSelectionPanel), "Unbind")]
        internal static class SpellSelectionPanel_SecondLineUnbind
        {
            internal static void Postfix()
            {
				foreach(RectTransform spellTable in spellLineTables)
                {
					if (spellTable != null && spellTable.gameObject.activeSelf & spellTable.childCount > 0)
					{
						Gui.ReleaseChildrenToPool(spellTable);
						spellTable.SetParent(null);
						GameObject.Destroy(spellTable.gameObject);
					}
                }
				spellLineTables.Clear();
            }
        }
    }
}
