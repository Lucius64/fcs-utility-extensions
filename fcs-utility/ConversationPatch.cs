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
}
