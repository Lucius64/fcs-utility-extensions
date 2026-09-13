using FCS_extended;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace fcs_utility
{
    public class FCSUtilitiesPlugin : IPlugin
    {
        public int Init(Assembly assembly)
        {
            Harmony harmony = new Harmony("fcs-utilities");
            harmony.PatchAll();
            Console.WriteLine("fcs-utilities loaded.");
            return 0;
        }
    }

    [HarmonyPatch]
    static class Navigation_Constructor_Patch
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Constructor(AccessTools.TypeByName("forgotten_construction_set.navigation"));
        }

        [HarmonyPostfix]
        static void Postfix()
        {
            Harmony harmony = new Harmony("fcs-utilities");
            harmony.Patch(AccessTools.Method("forgotten_construction_set.navigation:validateFile"), prefix: new HarmonyMethod(typeof(Navigation_validateFile_Patch).GetMethod("Prefix")));
        }
    }
}
