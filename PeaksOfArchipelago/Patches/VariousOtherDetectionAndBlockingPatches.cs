using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using PeaksOfArchipelago.Session;

namespace PeaksOfArchipelago.Patches
{
    [HarmonyPatch(typeof(Rewired.Player))]
    internal class HandBlockingPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("GetButton", [typeof(String)])]
        public static bool HandDisabler(ref bool __result, string actionName)
        {
            if (actionName == "Arm Right" && !Connection.Instance.slotData.HasTool(GameData.Tools.RightHand)) return false;
            if (actionName == "Arm Left" && !Connection.Instance.slotData.HasTool(GameData.Tools.leftHand)) return false;
            return true;
        }
    }
    // some more notification blockers or similar
}
