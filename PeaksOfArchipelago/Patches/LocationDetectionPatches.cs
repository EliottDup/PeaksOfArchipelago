using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using PeaksOfArchipelago.GameData;
using PeaksOfArchipelago.Session;
using UnityEngine;


namespace PeaksOfArchipelago.Patches
{
    [HarmonyPatch(typeof(StamperPeakSummit))]
    internal class StamperPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("StampJournal")]
        public static void PeakLocationComplete(StamperPeakSummit __instance)
        {
            if (__instance.isCustomLevel)
            {
                return;
            }

            Peaks peak = ItemTypes.PeakfromStamper(__instance.peakNames);
            
            PeaksOfArchipelago.Logger.LogInfo($"Stamping journal for peak {peak} (derived from {__instance.peakNames})");

            Connection.Instance.CompletePeakLocation(peak);

            if (Mappings.HasFreeSolo(peak) && GameObject.FindGameObjectWithTag("Player").GetComponent<RopeAnchor>().ropesPlacedDuringMap == 0)
            {
                Connection.Instance.CompleteFSPeakLocation(peak);
            }
        }

        public struct State
        {
            public bool crampons;
            public bool cramponUpgrade;
            public bool rope;
            public int ropesCollected;
            public bool ropeUpgrade;
            public bool pipe;
            public bool chalkbag;
            public bool coffee;
            public bool instantiated;
        }

        [HarmonyPrefix]
        [HarmonyPatch("SavePeakProgress")]
        public static void PrePeakProgressSave(out State __state)
        {
            __state = new State
            {
                crampons = GameManager.control.crampons,
                cramponUpgrade = GameManager.control.cramponsUpgrade,
                rope = GameManager.control.rope,
                ropesCollected = GameManager.control.ropesCollected,
                ropeUpgrade = GameManager.control.ropesUpgrade,
                pipe = GameManager.control.smokingpipe,
                chalkbag = GameManager.control.chalkBag,
                coffee = GameManager.control.coffee,
                instantiated = true
            };
        }

        [HarmonyPostfix]
        [HarmonyPatch("SavePeakProgress")]
        public static void PostPeakProgessSave(State __state)
        {
            // Incredible warning suppression moment
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable Harmony003 // Harmony non-ref patch parameters modified
            if (!__state.instantiated)
            {
                PeaksOfArchipelago.Logger.LogWarning("state not instantiated somehow");
                return;
            }
            GameManager.control.crampons = __state.crampons;
            GameManager.control.cramponsUpgrade = __state.cramponUpgrade;
            GameManager.control.rope = __state.rope;
            GameManager.control.ropesCollected = __state.ropesCollected;
            GameManager.control.ropesUpgrade = __state.ropeUpgrade;
            GameManager.control.smokingpipe = __state.pipe;
            GameManager.control.chalkBag = __state.chalkbag;
            GameManager.control.coffee = __state.coffee;
#pragma warning restore Harmony003 // Harmony non-ref patch parameters modified
#pragma warning restore IDE0079 // Remove unnecessary suppression
        }
    }

    [HarmonyPatch(typeof(ArtefactOnPeak))]
    internal class ArtefactOnPeakPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("PickUpItem")]
        public static void ArtefactLocationComplete(ArtefactOnPeak __instance)
        {
            int artefactnum = (int) __instance.peakArtefact - 1; // Adjust for None enum
            if (__instance.peakArtefact >= ArtefactOnPeak.Artefacts.Belt) artefactnum -= 3; // Adjust for artefacts not actually in game
            Artefacts artefact = (Artefacts)artefactnum;
            Connection.Instance.CompleteArtefactLocation(artefact);
        }
    }

    [HarmonyPatch(typeof(RopeCollectable))]
    internal class RopeCollectablePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("PickUpRope")]
        public static void RopeLocationComplete(RopeCollectable __instance)
        {
            Ropes rope;
            if (!__instance.isSingleRope)
            {
                rope = (Ropes)(__instance.extraRopeNumber + 4);
            }
            else
            {
                if (__instance.isWaltersCrag) rope = Ropes.WaltersCrag;
                else if (__instance.isWalkersPillar) rope = Ropes.WalkersPillar;
                else if (__instance.isGreatGaol) rope = Ropes.GreatGaol;
                else if (__instance.isStHaelga) rope = Ropes.StHaelga;
                else
                {
                    throw new Exception("Rope brok lol");
                }
            }
            Connection.Instance.CompleteRopeLocation(rope);
            RopeAnchor r = GameObject.FindGameObjectWithTag("Player").GetComponent<RopeAnchor>();
            if (__instance.isSingleRope)
            {
                r.anchorsInBackpack--;
                GameManager.control.ropesCollected--;
            }
            else
            {
                r.anchorsInBackpack -= 2;
                GameManager.control.ropesCollected -= 2;
            }
            r.UpdateRopesCollected();
        }

        [HarmonyPrefix]
        [HarmonyPatch("CheckRope")]
        public static void StartPrefix(out int __state)
        {
            __state = GameManager.control.ropesCollected;
        }
        
        [HarmonyPostfix]
        [HarmonyPatch("CheckRope")]
        public static void StartPostfix(int __state) {
            GameManager.control.ropesCollected = __state;
        }
    }

    [HarmonyPatch(typeof(BirdSeedCollectable))]
    internal class BirdSeedCollectablePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("PickUpBirdSeed")]
        public static void BirdSeedLocationComplete(BirdSeedCollectable __instance)
        {
            BirdSeeds seed = (BirdSeeds)__instance.extraBirdSeedNumber;
            Connection.Instance.CompleteBirdSeedLocation(seed);
            GameManager.control.extraBirdSeedUses--;
        }

        [HarmonyPrefix]
        [HarmonyPatch("CheckBirdSeed")]
        public static void StartPrefix()
        {
            GameManager.control.extraBirdSeedUses = 0;
        }

        [HarmonyPostfix]
        [HarmonyPatch("CheckBirdSeed")]
        public static void StartPostfix()
        {
            GameManager.control.extraBirdSeedUses = Connection.Instance.slotData.GetTotalExtraBirdSeedCount();
        }
    }

    [HarmonyPatch(typeof(TimeAttack))]
    internal class TimeAttackCompletePatch
    {
        public static TimeAttackDefaultData timeAttackDefaultData;

        public struct TimeAttackDefaultData
        {
            public float[] times;
            public int[] ropes;
            public int[] holds;
        }

        [HarmonyPrefix]
        [HarmonyPatch("SetBestTime")]
        public static void BestTimeSet(TimeAttack __instance)
        {
            if (timeAttackDefaultData.ropes == null)
            {
                return;
            }
            Peaks peak = ItemTypes.PeakfromStamper(__instance.summitStamper.peakNames);

            if (__instance.timer < timeAttackDefaultData.times[(int)peak])
            {
                Connection.Instance.CompleteTimePBLocation(peak);
            }
            if (__instance.ropesUsed <= timeAttackDefaultData.ropes[(int)peak])
            {
                Connection.Instance.CompleteRopePBLocation(peak);
            }
            if (__instance.holdsMade < timeAttackDefaultData.holds[(int)peak])
            {
                Connection.Instance.CompleteHoldPBLocation(peak);
            }
        }
    }

    [HarmonyPatch(typeof(TimeAttackSetter))]
    internal class SetDefaultTimes
    {
        [HarmonyPostfix]
        [HarmonyPatch("SetDefaults")]
        public static void LoadTimeAttack(TimeAttackSetter __instance)
        {
            if (TimeAttackCompletePatch.timeAttackDefaultData.times != null)
            {
                return;
            }
            TimeAttackCompletePatch.timeAttackDefaultData.times = [
                .. __instance.category1_defaultTimes, .. __instance.category2_defaultTimes, .. __instance.category3_defaultTimes,
                .. __instance.alps_category1_defaultTimes, .. __instance.alps_category2_defaultTimes, .. __instance.alps_category3_defaultTimes
                ];
            TimeAttackCompletePatch.timeAttackDefaultData.ropes = [
                .. __instance.category1_defaultRopes, .. __instance.category2_defaultRopes, .. __instance.category3_defaultRopes,
                .. __instance.alps_category1_defaultRopes, .. __instance.alps_category2_defaultRopes, .. __instance.alps_category3_defaultRopes
                ];
            TimeAttackCompletePatch.timeAttackDefaultData.holds = [
                .. __instance.category1_defaultHolds, .. __instance.category2_defaultHolds, .. __instance.category3_defaultHolds,
                .. __instance.alps_category1_defaultHolds, .. __instance.alps_category2_defaultHolds, .. __instance.alps_category3_defaultHolds
                ];
        }
    }
}
