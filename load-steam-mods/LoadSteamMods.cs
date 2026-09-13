/*--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Copyright (C) 2026 Lucius
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, version 3.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
using FCS_extended;
using HarmonyLib;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace load_steam_mods
{
    public class LoadSteamModsPlugin : IPlugin
    {
        public int Init(Assembly assembly)
        {
            Harmony harmony = new Harmony("load-steam-mods");
            harmony.PatchAll();
            Console.WriteLine("load-steam-mods plugin loaded.");
            return 0;
        }
    }

    static class SteamMods
    {
        internal static readonly Dictionary<string, string> list = new Dictionary<string, string>();

        internal static void Reflesh(string modDir)
        {
            Type type = AccessTools.TypeByName("forgotten_construction_set.SteamManager");
            if (type == null)
                return;

            list.Clear();

            object steamManager = AccessTools.PropertyGetter(type, "Instance").Invoke(null, null);
            if (!(AccessTools.PropertyGetter("forgotten_construction_set.SteamManager:Enabled").Invoke(steamManager, null) is bool enabled) || !enabled)
                return;

            HashSet<string> mods = new HashSet<string>();
            {
                var directory = new DirectoryInfo(modDir);
                if (directory.Exists)
                {
                    foreach (DirectoryInfo dir in directory.GetDirectories())
                    {
                        FileInfo[] files = dir.GetFiles(dir.Name + ".mod");
                        if (files.Length != 0)
                            mods.Add(files[0].Name);
                    }
                }
            }

            uint numItems = SteamUGC.GetNumSubscribedItems();
            PublishedFileId_t[] items = new PublishedFileId_t[numItems];
            SteamUGC.GetSubscribedItems(items, numItems);
            foreach (PublishedFileId_t item in items)
            {
                if ((SteamUGC.GetItemState(item) & (uint)EItemState.k_EItemStateInstalled) > 0)
                {
                    if (SteamUGC.GetItemInstallInfo(item, out _, out string folder, 260, out _))
                    {
                        var directory = new DirectoryInfo(folder);
                        if (directory.Exists)
                        {
                            FileInfo[] files = directory.GetFiles("*.mod");
                            if (files.Length != 0)
                            {
                                string filename = files[0].Name;
                                if (!mods.Contains(filename))
                                    list.Add(filename, directory.FullName.Replace("\\", "/"));
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch]
    static class InheritFiles_Constructor_Patch
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Constructor(
                AccessTools.TypeByName("forgotten_construction_set.InheritFiles"),
                new Type[]
                {
                        AccessTools.TypeByName("forgotten_construction_set.baseForm"),
                        typeof(string),
                        typeof(string)
                });
        }

        [HarmonyPostfix]
        static void Postfix(object __instance, string baseDir, string modDir, TreeView ___modList, HashSet<string> ___defaultChecked)
        {
            SteamMods.Reflesh(modDir);

            if (SteamMods.list.Count < 1)
                return;

            string[] activeMods = { };
            try
            {
                activeMods = File.ReadAllLines(Path.Combine(baseDir, "mods.cfg"));
            }
            catch (Exception)
            {
            }

            SortedDictionary<string, TreeNode> modNodePairs = new SortedDictionary<string, TreeNode>();

            foreach (TreeNode node in ___modList.Nodes)
            {
                modNodePairs.Add(node.Text, node);
            }

            if (!(AccessTools.PropertyGetter("forgotten_construction_set.InheritFiles:ForeColor").Invoke(__instance, null) is Color ForeColor))
                return;

            var loadHeader = AccessTools.Method("forgotten_construction_set.GameData:loadHeader");
            var createToolTip = AccessTools.Method("forgotten_construction_set.InheritFiles:createToolTip");

            foreach (var mod in SteamMods.list)
            {
                var header = loadHeader.Invoke(null, new object[] { Path.Combine(mod.Value, mod.Key) });

                TreeNode treeNode = new TreeNode(mod.Key)
                {
                    ForeColor = ForeColor,
                    Tag = header
                };

                if (createToolTip.Invoke(__instance, new object[] { treeNode.Text, header }) is string toolTip)
                    treeNode.ToolTipText = toolTip;

                modNodePairs.Add(mod.Key, treeNode);
            }

            ___modList.Nodes.Clear();
            foreach (var modName in activeMods)
            {
                if (modNodePairs.TryGetValue(modName, out var node))
                {
                    node.Checked = true;
                    ___modList.Nodes.Add(node);
                    ___defaultChecked.Add(modName);
                    modNodePairs.Remove(modName);
                }
            }

            foreach (var modNodePair in modNodePairs)
            {
                ___modList.Nodes.Add(modNodePair.Value);
            }
        }
    }

    [HarmonyPatch("forgotten_construction_set.InheritFiles", "loadButton_Click")]
    static class InheritFiles_loadButton_Click_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(TreeNode ___activeNode)
        {
            if (SteamMods.list.TryGetValue(___activeNode.Text, out _))
            {
                MessageBox.Show("Error : Steam mods cannot be edited.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            return true;
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
            Harmony harmony = new Harmony("load-steam-mods");
            harmony.Patch(AccessTools.Method("forgotten_construction_set.baseForm:addLoadedFile"), prefix: new HarmonyMethod(typeof(BaseForm_addLoadedFile_Patch).GetMethod("Prefix")));
            harmony.Patch(AccessTools.Method("forgotten_construction_set.navigation:validateFile"), prefix: new HarmonyMethod(typeof(Navigation_validateFile_Patch).GetMethod("Prefix")));
        }
    }

    static class BaseForm_addLoadedFile_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(ref string path, string file)
        {
            if (!Directory.Exists(path))
                if (SteamMods.list.TryGetValue(file, out string dir))
                    path = dir + "/";
        }
    }

    [HarmonyPatch]
    static class MergeDialog_Constructor_Patch
    {
        static System.Reflection.MethodBase TargetMethod()
        {
            return AccessTools.Constructor(
                AccessTools.TypeByName("forgotten_construction_set.MergeDialog"),
                new Type[]
                {
                        AccessTools.TypeByName("forgotten_construction_set.GameData"),
                        AccessTools.TypeByName("forgotten_construction_set.navigation")
                });
        }

        [HarmonyPostfix]
        static void Postfix(dynamic nav, ComboBox ___modBox)
        {
            if (nav.FileMode != 0
                && !(bool)AccessTools.PropertyGetter("forgotten_construction_set.TranslationManager:TranslationMode").Invoke(null, null))
            {
                if (nav.RootPath == null || nav.ModFolderName == null)
                    return;

                SteamMods.Reflesh(Path.Combine(nav.RootPath, nav.ModFolderName));
                if (SteamMods.list.Count < 1)
                    return;

                foreach (var mod in SteamMods.list)
                {
                    FileInfo fileInfo = new FileInfo(mod.Value + "/" + mod.Key);
                    if (fileInfo.Exists)
                    {
                        var ModListItem = AccessTools.Constructor(
                            AccessTools.TypeByName("forgotten_construction_set.MergeDialog+ModListItem"),
                            new Type[]
                            {
                                        typeof(string),
                                        typeof(string)
                            });
                        ___modBox.Items.Add(ModListItem.Invoke(new object[] { fileInfo.Name, fileInfo.FullName.Replace('\\', '/') }));
                    }
                }
            }
        }
    }

    [HarmonyPatch("forgotten_construction_set.MergeDialog", "getLevelDataFolder")]
    static class MergeDialog_getLevelDataFolder_Patch
    {
        [HarmonyPostfix]
        static void Postfix(string modPath, ref string __result)
        {
            if (SteamMods.list.Count < 1)
                return;

            if (SteamMods.list.TryGetValue(Path.GetFileName(modPath), out string dir))
                __result = dir.Replace("\\", "/") + "/leveldata/";
        }
    }
}
