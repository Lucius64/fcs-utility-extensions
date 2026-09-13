/*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Copyright (C) 2026 Lucius
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, version 3.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fcs_utility
{
    static class Navigation_validateFile_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            dynamic __instance,
            string file,
            ref string __result)
        {
            file = file.Replace('\\', '/');
            if (file == "")
            {
                __result = "";
                return false;
            }

            if ((int)__instance.FileMode == 0)
            {
                __result = file;
                return false;
            }

            if (file.StartsWith(__instance.ModPath, StringComparison.InvariantCultureIgnoreCase) || file.StartsWith(__instance.getAssetsFolder(), StringComparison.InvariantCultureIgnoreCase))
            {
                __result = "." + file.Substring(__instance.RootPath.Length);
                return false;
            }

            if (file.EndsWith(".bnk"))
                return true;

            var from = __instance.MdiParent;
            if (from.GetType() == AccessTools.TypeByName("forgotten_construction_set.baseForm"))
            {
                string modFolder = "./" + __instance.ModFolderName + "/";
                if (file.StartsWith(__instance.getModFolder()))
                    file = "." + file.Substring(__instance.RootPath.Length);

                foreach (string mod in from.activeMods)
                {
                    if (file.StartsWith(modFolder + Path.GetFileNameWithoutExtension(mod) + "/"))
                    {
                        __result = file;
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
