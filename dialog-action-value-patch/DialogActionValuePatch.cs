using FCS_extended;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dialog_action_value_patch
{
    public class DialogActionValuePatch : IPlugin
    {
        public int Init(Assembly assembly)
        {
            Harmony harmony = new Harmony("dialog-action-value-patch");
            harmony.PatchAll();
            Console.WriteLine("dialog-action-value-patch loaded.");
            return 0;
        }

        [HarmonyPatch("forgotten_construction_set.conversation", "InitializeComponent")]
        public static class Conversation_InitializeComponent_Patch
        {
            [HarmonyPostfix]
            static void Postfix(NumericUpDown ___effectValue)
            {
                ___effectValue.Maximum = 10000000;
                ___effectValue.Minimum = -10000000;
            }
        }

        [HarmonyPatch("forgotten_construction_set.dialog.ConditionControl", "InitializeComponent")]
        public static class ConditionControl_InitializeComponent_Patch
        {
            [HarmonyPostfix]
            static void Postfix(NumericUpDown ___conditionsValue)
            {
                ___conditionsValue.Maximum = 10000000;
                ___conditionsValue.Minimum = -10000000;
            }
        }
    }
}
