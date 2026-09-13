/*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Copyright (C) 2026 Lucius
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, version 3.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
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
