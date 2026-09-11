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
}
