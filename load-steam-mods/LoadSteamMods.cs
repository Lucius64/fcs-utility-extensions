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
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace load_steam_mods
{
    public class LoadSteamModsPlugin : IPlugin
    {
        static readonly Harmony harmony = new Harmony("load-steam-mods");

        public int Init(Assembly assembly)
        {
            harmony.PatchAll();
            Console.WriteLine("load-steam-mods plugin loaded.");
            return 0;
        }

        static readonly Dictionary<string, string> steamModList = new Dictionary<string, string>();

        [HarmonyPatch]
        public static class InheritFiles_Constructor_Patch
        {
            static System.Reflection.MethodBase TargetMethod()
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
            static void Postfix(object __instance, string baseDir, TreeView ___modList, HashSet<string> ___defaultChecked)
            {
                steamModList.Clear();

                object SteamManager_Instance = AccessTools.PropertyGetter("forgotten_construction_set.SteamManager:Instance").Invoke(null, null);
                if (SteamManager_Instance == null)
                    return;

                if (!(AccessTools.PropertyGetter("forgotten_construction_set.SteamManager:Enabled").Invoke(SteamManager_Instance, null) is bool enabled))
                    return;

                if (!enabled)
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

                uint numItems = SteamUGC.GetNumSubscribedItems();
                PublishedFileId_t[] items = new PublishedFileId_t[numItems];
                SteamUGC.GetSubscribedItems(items, numItems);
                foreach (PublishedFileId_t item in items)
                {
                    if ((SteamUGC.GetItemState(item) & (uint)EItemState.k_EItemStateInstalled) > 0)
                    {
                        if (SteamUGC.GetItemInstallInfo(item, out ulong size, out string folder, 260, out uint timestamp))
                        {
                            var directory = new DirectoryInfo(folder);
                            if (directory.Exists)
                            {
                                FileInfo[] files = directory.GetFiles("*.mod");
                                if (files.Length != 0)
                                {
                                    string filename = files[0].Name;
                                    if (!modNodePairs.ContainsKey(filename))
                                    {
                                        var header = loadHeader.Invoke(null, new object[] { files[0].FullName });

                                        TreeNode treeNode = new TreeNode(filename)
                                        {
                                            ForeColor = ForeColor,
                                            Tag = header
                                        };

                                        if (createToolTip.Invoke(__instance, new object[] { treeNode.Text, header }) is string toolTip)
                                            treeNode.ToolTipText = toolTip;

                                        modNodePairs.Add(filename, treeNode);
                                        steamModList.Add(filename, directory.FullName.Replace("\\", "/"));
                                    }
                                }
                            }
                        }
                    }
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
        public static class InheritFiles_loadButton_Click_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(object __instance, TreeNode ___activeNode)
            {
                if (steamModList.TryGetValue(___activeNode.Text, out var dir))
                {
                    MessageBox.Show("Error : Steam mods cannot be edited.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch]
        public static class navigation_Constructor_Patch
        {
            static System.Reflection.MethodBase TargetMethod()
            {
                return AccessTools.Constructor(AccessTools.TypeByName("forgotten_construction_set.navigation"));
            }

            [HarmonyPostfix]
            static void Postfix()
            {
                harmony.Patch(AccessTools.Method("forgotten_construction_set.baseForm:addLoadedFile"), prefix: new HarmonyMethod(typeof(baseForm_addLoadedFile_Patch).GetMethod("Prefix")));
            }
        }

        public static class baseForm_addLoadedFile_Patch
        {
            [HarmonyPrefix]
            public static void Prefix(ref string path, string file)
            {
                if (!Directory.Exists(path))
                    if (steamModList.TryGetValue(file, out string dir))
                        path = dir + "/";
            }
        }

        [HarmonyPatch]
        public static class MergeDialog_Constructor_Patch
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
            static void Postfix(object nav, ComboBox ___modBox)
            {
                if ((int)AccessTools.PropertyGetter("forgotten_construction_set.navigation:FileMode").Invoke(nav, null) != 0
                    && !(bool)AccessTools.PropertyGetter("forgotten_construction_set.TranslationManager:TranslationMode").Invoke(null, null))
                {
                    if (0 < steamModList.Count)
                    {
                        foreach (var mod in steamModList)
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
        }
    }
}
