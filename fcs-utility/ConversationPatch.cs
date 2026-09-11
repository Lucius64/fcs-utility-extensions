/*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Copyright (C) 2026 Lucius
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, version 3.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fcs_utility
{
    [HarmonyPatch("forgotten_construction_set.conversation", "InitializeComponent")]
    static class Conversation_InitializeComponent_Patch
    {
        [HarmonyPostfix]
        static void Postfix(NumericUpDown ___effectValue)
        {
            ___effectValue.Maximum = 10000000;
            ___effectValue.Minimum = -10000000;
        }
    }

    [HarmonyPatch("forgotten_construction_set.dialog.ConditionControl", "InitializeComponent")]
    static class ConditionControl_InitializeComponent_Patch
    {
        [HarmonyPostfix]
        static void Postfix(NumericUpDown ___conditionsValue)
        {
            ___conditionsValue.Maximum = 10000000;
            ___conditionsValue.Minimum = -10000000;
        }
    }

    [HarmonyPatch]
    static class Conversation_Constructor_Patch
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Constructor(
                AccessTools.TypeByName("forgotten_construction_set.conversation"),
                new Type[]
                {
                    AccessTools.TypeByName("forgotten_construction_set.GameData+Item"),
                    AccessTools.TypeByName("forgotten_construction_set.navigation")
                });
        }

        [HarmonyPostfix]
        static void Postfix(ComboBox ___PossibleEffects)
        {
            if (AccessTools.Method("forgotten_construction_set.FCSEnums:getEnum").Invoke(null, new object[] { "DialogActionEnum" }) is IEnumerable<KeyValuePair<string, int>> fcsEnum && fcsEnum.Count() != 0)
            {
                ___PossibleEffects.Items.Clear();

                foreach (KeyValuePair<string, int> item in fcsEnum)
                {
                    ___PossibleEffects.Items.Add(item.Key);
                }
            }
        }
    }

    public static class DialogCollectionPatchHelper
    {
        public static Dictionary<int, int> GetUsedEvents(dynamic Item)
        {
            var events = new Dictionary<int, int>();
            if (AccessTools.Method("forgotten_construction_set.FCSEnums:getEnum").Invoke(null, new object[] { "EventTriggerEnum" }) is IEnumerable<KeyValuePair<string, int>> fcsEnum && fcsEnum.Count() != 0)
            {
                foreach (KeyValuePair<string, int> item in fcsEnum)
                {
                    events.Add(item.Value, 0);
                }
            }

            foreach (dynamic item in Item.referenceData("dialogs"))
            {
                int stateFlag = (!Item.getState("dialogs", item.Key).HasFlag((Enum)Enum.Parse(AccessTools.TypeByName("forgotten_construction_set.GameData+State"), "INVALID"))) ? 1 : 2;
                if (item.Value.v0 > 0 && events.ContainsKey(item.Value.v0))
                    events[item.Value.v0] |= stateFlag;

                if (item.Value.v1 > 0 && events.ContainsKey(item.Value.v1))
                    events[item.Value.v1] |= stateFlag;

                if (item.Value.v2 > 0 && events.ContainsKey(item.Value.v2))
                    events[item.Value.v2] |= stateFlag;
            }

            return events;
        }
    }

    [HarmonyPatch("forgotten_construction_set.dialogCollection", "CreateEventsList")]
    static class DialogCollection_CreateEventsList_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(
            dynamic __instance,
            ListView ___eventsList,
            CheckBox ___showAllEvents)
        {
            Dictionary<int, int> usedEvents = ___showAllEvents.Checked ? DialogCollectionPatchHelper.GetUsedEvents(__instance.Item) : null;
            ___eventsList.Items.Clear();
            ___eventsList.BeginUpdate();

            var isValidTriggerEnum = AccessTools.Method("forgotten_construction_set.dialogCollection:isValidTriggerEnum");
            if (AccessTools.Method("forgotten_construction_set.FCSEnums:getEnum").Invoke(null, new object[] { "EventTriggerEnum" }) is IEnumerable<KeyValuePair<string, int>> fcsEnum)
            {
                foreach (var value in Enum.GetValues(AccessTools.TypeByName("forgotten_construction_set.EventTriggerEnum")))
                {
                    if ((usedEvents == null || usedEvents[(int)value] > 0) && isValidTriggerEnum.Invoke(__instance, new object[] { value, fcsEnum }))
                        ___eventsList.Items.Add(value.ToString()).Tag = value;
                }
            }

            ___eventsList.EndUpdate();
            ___eventsList.Sort();
            if (___eventsList.Items.Count > 0)
                ___eventsList.SelectedIndices.Add(0);

            return false;
        }
    }

    [HarmonyPatch("forgotten_construction_set.dialogCollection", "RefreshUI")]
    static class DialogCollection_RefreshUI_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(
            dynamic __instance,
            TreeView ___treeView1,
            ListView ___eventsList,
            dynamic ___conditionPanel,
            dynamic ___conditionControl1,
            dynamic ___objectPropertyBox1)
        {
            Dictionary<int, int> usedEvents = DialogCollectionPatchHelper.GetUsedEvents(__instance.Item);

            var GetStateColor = AccessTools.Method("forgotten_construction_set.StateColours:GetStateColor");

            foreach (ListViewItem item in ___eventsList.Items)
            {
                int stateFlag = usedEvents[(int)item.Tag];
                if (stateFlag > 1)
                    item.ForeColor = (Color)GetStateColor.Invoke(null, new object[] { Enum.Parse(AccessTools.TypeByName("forgotten_construction_set.GameData+State"), "INVALID") });
                else if (stateFlag == 0)
                    item.ForeColor = (Color)GetStateColor.Invoke(null, new object[] { Enum.Parse(AccessTools.TypeByName("forgotten_construction_set.GameData+State"), "REMOVED") });
                else
                    item.ForeColor = ___eventsList.ForeColor;
            }

            ___treeView1.BeginUpdate();
            ___treeView1.Nodes.Clear();

            int SelectedEvent = (int)AccessTools.PropertyGetter("forgotten_construction_set.dialogCollection:SelectedEvent").Invoke(__instance, null);

            foreach (var dialogItem in __instance.Item.referenceData("dialogs"))
            {
                if (dialogItem.Value.v0 != SelectedEvent && dialogItem.Value.v1 != SelectedEvent && dialogItem.Value.v2 != SelectedEvent)
                    continue;

                dynamic item = __instance.nav.ou.gameData.getItem(dialogItem.Key);
                TreeNode treeNode;
                if (item == null)
                {
                    treeNode = ___treeView1.Nodes.Add("Invalid dialogue " + dialogItem.Key);
                    treeNode.ForeColor = (Color)GetStateColor.Invoke(null, new object[] { Enum.Parse(AccessTools.TypeByName("forgotten_construction_set.GameData+State"), "INVALID") });
                    treeNode.Tag = dialogItem.Key;
                    continue;
                }

                treeNode = ___treeView1.Nodes.Add(item.Name);
                treeNode.Tag = item;
                AccessTools.Method("forgotten_construction_set.conversation:refreshNode").Invoke(null, new object[] { treeNode, item.Name });
                treeNode.ForeColor = (Color)GetStateColor.Invoke(null, new object[] { __instance.Item.getState("dialogs", dialogItem.Key) });

                if (!item.getState().HasFlag((Enum)Enum.Parse(AccessTools.TypeByName("forgotten_construction_set.GameData+State"), "REMOVED")))
                    AccessTools.Method("forgotten_construction_set.conversation:createConversationTree").Invoke(null, new object[] { __instance.nav.ou.gameData, item, treeNode.Nodes, null });
            }

            ___treeView1.EndUpdate();
            ___conditionPanel.SetData(__instance.nav, (__instance.SelectedItem != null && (int)__instance.SelectedItem.type == 18) ? __instance.SelectedItem : null, false);
            ___conditionControl1.refresh(__instance.SelectedItem);
            ___objectPropertyBox1.refresh(__instance.SelectedItem);

            return false;
        }
    }
}
