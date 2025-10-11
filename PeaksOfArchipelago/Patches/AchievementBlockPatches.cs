using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace PeaksOfArchipelago.Patches
{
    [HarmonyPatch(typeof(StatsAndAchievements))]
    internal class AchievementBlockPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnEnable")]
        public static bool AchievementDisabler()
        {
            PeaksOfArchipelago.Logger.LogInfo("Disabled Achievements!");
            return false;
        }
    }
}
