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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fcs_utility
{
    [HarmonyPatch("forgotten_construction_set.ReferenceList", "contextMenu_Opening")]
    static class ReferenceList_contextMenu_Opening_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(
            dynamic __instance,
            CancelEventArgs e,
            dynamic ___nav,
            ToolStripMenuItem ___openItem,
            ToolStripMenuItem ___copyID,
            ToolStripMenuItem ___revertItem,
            ToolStripMenuItem ___removeItem,
            ToolStripMenuItem ___replaceWithCopy,
            ToolStripMenuItem ___removeSection,
            ToolStripMenuItem ___referenceInfo
            )
        {
            PropertyGrid.PropertyGrid grid = __instance.grid;
            if (grid.SelectedSection == null
                || grid.SelectedItem == null
                || grid.SelectedItem.Data != null)
                return true;

            string id = grid.SelectedItem.Data?.ToString();
            dynamic item = id == null ? null : ___nav.ou.gameData.getItem(id);

            ___openItem.Visible = true;
            ___copyID.Visible = true;
            ___revertItem.Visible = id != null && __instance.Item.getState(grid.SelectedSection.Name, id).HasFlag((Enum)Enum.Parse(AccessTools.TypeByName("forgotten_construction_set.GameData+State"), "MODIFIED"));
            ___removeItem.Visible = true;
            ___replaceWithCopy.Visible = item != null && (int)item.type != 18 && (int)item.type != 19;
            ___removeSection.Visible = false;
            ___referenceInfo.Visible = true;

            Color color = (Color)AccessTools.Method("forgotten_construction_set.StateColours:GetStateColor").Invoke(null, new object[] { (Enum)Enum.Parse(AccessTools.TypeByName("forgotten_construction_set.GameData+State"), "LOCKED") });
            bool enabled = grid.SelectedItem.TextColour != color && !___nav.ReadOnly;
            ___revertItem.Enabled = enabled;
            ___removeItem.Enabled = enabled;
            ___replaceWithCopy.Enabled = enabled;
            return false;
        }
    }
}
