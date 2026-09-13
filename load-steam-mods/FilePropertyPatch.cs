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

namespace load_steam_mods
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

            if (file.EndsWith(".bnk"))
                return true;

            var from = __instance.MdiParent;
            if (from.GetType() == AccessTools.TypeByName("forgotten_construction_set.baseForm"))
            {
                string modFolder = "./" + __instance.ModFolderName + "/";
                foreach (string mod in from.activeMods)
                {
                    if (SteamMods.list.TryGetValue(mod, out string dir))
                    {
                        if (file.StartsWith(dir))
                        {
                            __result = modFolder + Path.GetFileNameWithoutExtension(mod) + file.Substring(dir.Length);
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
