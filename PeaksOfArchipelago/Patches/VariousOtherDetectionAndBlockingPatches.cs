using HarmonyLib;
using PeaksOfArchipelago.GameData;
using PeaksOfArchipelago.Session;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    internal class NPCGivenItemsPatch
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

    [HarmonyPatch(typeof(NPC_Climber))]
    internal class ClimberGivenItemsPatch
    {
        [HarmonyPatch("GivePlayerRope")]
        [HarmonyPostfix]
        public static void CollectRope(NPC_Climber __instance)
        {
            Ropes rope = (Ropes)(-1);
            if (__instance.isWaltersCrag) rope = Ropes.WaltersCrag;
            else if (__instance.isWalkersPillar) rope = Ropes.WalkersPillar;
            else
            {
                throw new Exception("Unknown rope NPC");
            }
            GameManager.control.ropesCollected--;
            GameObject.FindGameObjectWithTag("Player").GetComponent<RopeAnchor>().anchorsInBackpack--;

            Connection.Instance.CompleteRopeLocation(rope);
        }
    }

    [HarmonyPatch(typeof(TimeAttack))]
    internal class TimeAttackNullReferenceExceptionPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static bool UpdatePatch()
        {
            if (SceneManager.GetActiveScene().buildIndex == 37) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(RopeAnchor))]
    internal class RopeAnchorPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        public static void Prefix(RopeAnchor __instance, out int __state)
        {
            __state = GameManager.control.ropesCollected;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void Postfix(RopeAnchor __instance, int __state)
        {
            __instance.anchorsInBackpack = __state;
            GameManager.control.ropesCollected = __state;
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
