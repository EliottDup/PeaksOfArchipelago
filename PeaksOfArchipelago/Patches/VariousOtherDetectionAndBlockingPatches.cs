using HarmonyLib;
using PeaksOfArchipelago.GameData;
using PeaksOfArchipelago.Session;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [HarmonyPatch(typeof(OilLamp))]
    internal class OilLampPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OilLampLightUp")]
        public static void Postfix(OilLamp __instance, ref IEnumerator __result)
        {
            if (Connection.Instance.slotData.HasTool(GameData.Tools.Lamp))
            {
                return;
            }
            else
            {
                __instance.oilLampLight.intensity = 0;
                __result = null;
            }
        }
    }

    [HarmonyPatch(typeof(NPCSystem))]
    internal class GivePlayerMonocularPatch
    {
        [HarmonyPatch("GivePlayerMonocular")]
        [HarmonyPostfix]
        public static void TakeAwayMonocular()
        {
            GameManager.control.monocular = Connection.Instance.slotData.HasTool(GameData.Tools.Monocular);
        }

        [HarmonyPatch("GivePlayerCoffee")]
        [HarmonyPostfix]
        public static void TakeAwayCoffee()
        {
            GameManager.control.coffee = Connection.Instance.slotData.HasTool(GameData.Tools.Coffee);
        }

        [HarmonyPatch("GivePlayerRope")]
        [HarmonyPostfix]
        public static void TakeAwayRope(NPCSystem __instance)
        {
            Ropes rope = (Ropes)(-1);
            if (__instance.isStHaelga)
            {
                rope = Ropes.StHaelga;
            }
            else if (__instance.isGreatGaol)
            {
                rope = Ropes.GreatGaol;
            }
            else
            {
                throw new Exception("Unknown rope NPC");
            }
            GameManager.control.ropesCollected--;
            GameObject.FindGameObjectWithTag("Player").GetComponent<RopeAnchor>().anchorsInBackpack--;

            Connection.Instance.CompleteRopeLocation(rope);
        }
    }

    [HarmonyPatch(typeof(RopeAnchor))]
    internal class RopeAnchorPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        public static void Prefix(RopeAnchor __instance)
        {
            GameManager.control.ropesCollected = 0;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void Postfix(RopeAnchor __instance)
        {
            int ropeCount = Connection.Instance.slotData.GetTotalRopeCount();
            __instance.anchorsInBackpack = ropeCount;
            GameManager.control.ropesCollected = ropeCount;
            __instance.UpdateRopesCollected();
        }
    }
    
    [HarmonyPatch(typeof(FallingEvent))]
    internal class DeathFallPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("FellToDeath")]
        public static void Kill()
        {
            Connection.Instance?.HandleDeath();
        }
    }
    
}
