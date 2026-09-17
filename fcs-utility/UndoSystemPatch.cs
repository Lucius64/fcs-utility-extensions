/*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Copyright (C) 2026 Lucius
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, version 3.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fcs_utility
{
    [HarmonyPatch("forgotten_construction_set.dialog.ConditionControl", "addCondition_Click")]
    static class ConditionControl_addCondition_Click_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var found = false;
            var codeInstructions = new List<CodeInstruction>(instructions);

            var method_refresh = AccessTools.Method("forgotten_construction_set.dialog.ConditionControl:refresh");

            for (int i = 0; i < codeInstructions.Count; i++)
            {
                var instruction = codeInstructions[i];
                if (3 <= i
                    && instruction.Calls(method_refresh))
                {
                    codeInstructions.InsertRange(i - 3,
                        new CodeInstruction[]
                        {
                            // nav.Undo.StartGroup();
                            new CodeInstruction(OpCodes.Ldarg_0, null),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field("forgotten_construction_set.dialog.ConditionControl:nav")),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter("forgotten_construction_set.navigation:Undo")),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method("forgotten_construction_set.UndoSystem:StartGroup")),

                            // nav.Undo.Add(new UndoSystem.NewItem(item));
                            new CodeInstruction(OpCodes.Ldarg_0, null),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field("forgotten_construction_set.dialog.ConditionControl:nav")),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter("forgotten_construction_set.navigation:Undo")),
                            new CodeInstruction(OpCodes.Ldloc_3, null),
                            new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(AccessTools.TypeByName("forgotten_construction_set.UndoSystem+NewItem"), new Type[]{ AccessTools.TypeByName("forgotten_construction_set.GameData+Item") })),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method("forgotten_construction_set.UndoSystem:Add")),

                            //nav.Undo.Add(new UndoSystem.ModRef(CurrentLine, "conditions", item.stringID, null, new GameData.TripleInt((int)conditionsValue.Value)));
                            new CodeInstruction(OpCodes.Ldarg_0, null),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field("forgotten_construction_set.dialog.ConditionControl:nav")),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter("forgotten_construction_set.navigation:Undo")),
                            new CodeInstruction(OpCodes.Ldarg_0, null),
                            new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter("forgotten_construction_set.dialog.ConditionControl:CurrentLine")),
                            new CodeInstruction(OpCodes.Ldstr, "conditions"),
                            new CodeInstruction(OpCodes.Ldloc_3, null),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter("forgotten_construction_set.GameData+Item:stringID")),
                            new CodeInstruction(OpCodes.Ldnull, null),
                            new CodeInstruction(OpCodes.Ldarg_0, null),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field("forgotten_construction_set.dialog.ConditionControl:conditionsValue")),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(NumericUpDown), "Value")),
                            new CodeInstruction(OpCodes.Call, typeof(decimal).GetMethods().Where(method => method.Name == "op_Explicit" && method.ReturnType == typeof(int)).First()),
                            new CodeInstruction(OpCodes.Ldc_I4_0, null),
                            new CodeInstruction(OpCodes.Ldc_I4_0, null),
                            new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(AccessTools.TypeByName("forgotten_construction_set.GameData+TripleInt"), new Type[]{ typeof(int), typeof(int), typeof(int) })),
                            new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(AccessTools.TypeByName("forgotten_construction_set.UndoSystem+ModRef"), new Type[]{ AccessTools.TypeByName("forgotten_construction_set.GameData+Item"), typeof(string), typeof(string), AccessTools.TypeByName("forgotten_construction_set.GameData+TripleInt"), AccessTools.TypeByName("forgotten_construction_set.GameData+TripleInt") })),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method("forgotten_construction_set.UndoSystem:Add")),

                            //nav.Undo.EndGroup();
                            new CodeInstruction(OpCodes.Ldarg_0, null),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field("forgotten_construction_set.dialog.ConditionControl:nav")),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter("forgotten_construction_set.navigation:Undo")),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method("forgotten_construction_set.UndoSystem:EndGroup"))
                        });

                    found = true;
                    break;
                }
            }

            if (!found)
                throw new ArgumentException("Cannot find Callvirt forgotten_construction_set.dialog.ConditionControl.refresh in forgotten_construction_set.dialog.ConditionControl.addCondition_Click");

            return codeInstructions;
        }
    }

    [HarmonyPatch("forgotten_construction_set.dialog.ConditionControl", "removeCondition_Click")]
    static class ConditionControl_removeCondition_Click_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var found = false;
            var codeInstructions = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codeInstructions.Count; i++)
            {
                if (i + 3 < codeInstructions.Count
                    && codeInstructions[i].LoadsField(AccessTools.Field("forgotten_construction_set.dialog.ConditionControl:listView1conditions"))
                    && codeInstructions[i + 1].Calls(AccessTools.PropertyGetter(typeof(ListView), "SelectedItems"))
                    && codeInstructions[i + 2].Calls(AccessTools.PropertyGetter(typeof(ListView.SelectedListViewItemCollection), "Count")))
                {
                    codeInstructions.InsertRange(i,
                        new CodeInstruction[]
                        {
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field("forgotten_construction_set.dialog.ConditionControl:nav")),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter("forgotten_construction_set.navigation:Undo")),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method("forgotten_construction_set.UndoSystem:StartGroup")),
                            new CodeInstruction(OpCodes.Ldarg_0, null)
                        });

                    found = true;
                    break;
                }
            }

            if (!found)
                throw new ArgumentException("Cannot find listView1conditions.SelectedItems.Count in forgotten_construction_set.dialog.ConditionControl.removeCondition_Click");
            found = false;

            var method_removeReference = AccessTools.Method("forgotten_construction_set.GameData+Item:removeReference", new Type[] { typeof(string), typeof(int) });

            for (int i = 0; i < codeInstructions.Count; i++)
            {
                var instruction = codeInstructions[i];
                if (9 <= i
                    && instruction.Calls(method_removeReference))
                {
                    codeInstructions.InsertRange(i - 9,
                        new CodeInstruction[]
                        {
                            new CodeInstruction(OpCodes.Ldarg_0, null),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field("forgotten_construction_set.dialog.ConditionControl:nav")),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter("forgotten_construction_set.navigation:Undo")),
                            new CodeInstruction(OpCodes.Ldarg_0, null),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field("forgotten_construction_set.dialog.ConditionControl:nav")),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field("forgotten_construction_set.navigation:ou")),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field("forgotten_construction_set.head:gameData")),
                            new CodeInstruction(OpCodes.Ldloc_1, null),
                            new CodeInstruction(OpCodes.Ldc_I4_1, null),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method("forgotten_construction_set.UndoSystem:AddDeleteItem"))
                        });

                    found = true;
                    break;
                }
            }

            if (!found)
                throw new ArgumentException("Cannot find Callvirt forgotten_construction_set.GameData.Item.removeReference in forgotten_construction_set.dialog.ConditionControl.removeCondition_Click");
            found = false;

            var method_refresh = AccessTools.Method("forgotten_construction_set.dialog.ConditionControl:refresh");

            for (int i = 0; i < codeInstructions.Count; i++)
            {
                var instruction = codeInstructions[i];
                if (3 <= i
                    && instruction.Calls(method_refresh))
                {
                    codeInstructions.InsertRange(i - 3,
                        new CodeInstruction[]
                        {
                            new CodeInstruction(OpCodes.Ldarg_0, null),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field("forgotten_construction_set.dialog.ConditionControl:nav")),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter("forgotten_construction_set.navigation:Undo")),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method("forgotten_construction_set.UndoSystem:EndGroup"))
                        });

                    found = true;
                    break;
                }
            }

            if (!found)
                throw new ArgumentException("Cannot find Callvirt forgotten_construction_set.dialog.ConditionControl.refresh in forgotten_construction_set.dialog.ConditionControl.removeCondition_Click");

            return codeInstructions;
        }
    }

    [HarmonyPatch("forgotten_construction_set.dialog.ConditionControl", "updateCondition")]
    static class ConditionControl_updateCondition_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var found = false;
            var codeInstructions = new List<CodeInstruction>(instructions);

            var method_removeReference = AccessTools.Method("forgotten_construction_set.GameData+Item:removeReference", new Type[] { typeof(string), typeof(int) });

            for (int i = 0; i < codeInstructions.Count; i++)
            {
                var instruction = codeInstructions[i];
                if (7 <= i
                    && instruction.Calls(method_removeReference))
                {
                    codeInstructions.InsertRange(i - 7,
                        new CodeInstruction[]
                        {
                            // nav.Undo.StartGroup();
                            new CodeInstruction(OpCodes.Ldarg_0, null),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field("forgotten_construction_set.dialog.ConditionControl:nav")),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter("forgotten_construction_set.navigation:Undo")),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method("forgotten_construction_set.UndoSystem:StartGroup")),

                            // nav.Undo.AddDeleteItem(nav.ou.gameData, referenceItem);
                            new CodeInstruction(OpCodes.Ldarg_0, null),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field("forgotten_construction_set.dialog.ConditionControl:nav")),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter("forgotten_construction_set.navigation:Undo")),
                            new CodeInstruction(OpCodes.Ldarg_0, null),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field("forgotten_construction_set.dialog.ConditionControl:nav")),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field("forgotten_construction_set.navigation:ou")),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field("forgotten_construction_set.head:gameData")),
                            new CodeInstruction(OpCodes.Ldloc_0, null),
                            new CodeInstruction(OpCodes.Ldc_I4_1, null),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method("forgotten_construction_set.UndoSystem:AddDeleteItem"))
                        });

                    found = true;
                    break;
                }
            }

            if (!found)
                throw new ArgumentException("Cannot find Callvirt forgotten_construction_set.GameData.Item.removeReference in forgotten_construction_set.dialog.ConditionControl.updateCondition");
            found = false;

            var method_addCondition_Click = AccessTools.Method("forgotten_construction_set.dialog.ConditionControl:addCondition_Click");

            for (int i = 0; i < codeInstructions.Count - 1; i++)
            {
                var instruction = codeInstructions[i];
                if (instruction.Calls(method_addCondition_Click))
                {
                    codeInstructions.InsertRange(i + 1,
                        new CodeInstruction[]
                        {
                            // nav.Undo.EndGroup();
                            new CodeInstruction(OpCodes.Ldarg_0, null),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field("forgotten_construction_set.dialog.ConditionControl:nav")),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter("forgotten_construction_set.navigation:Undo")),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method("forgotten_construction_set.UndoSystem:EndGroup"))
                        });

                    found = true;
                    break;
                }
            }

            if (!found)
                throw new ArgumentException("Cannot find Callvirt forgotten_construction_set.dialog.ConditionControl.addCondition_Click in forgotten_construction_set.dialog.ConditionControl.updateCondition");

            return codeInstructions;
        }
    }
}
