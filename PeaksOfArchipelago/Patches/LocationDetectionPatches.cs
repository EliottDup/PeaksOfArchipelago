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

            int p = (int)__instance.peakNames;
            if (p >= (int)StamperPeakSummit.PeakNames.IceWaterFallDemo)
            {
                p--;
            }

            HashSet<Peaks> FSPeaks = new HashSet<Peaks>()
            {
                Peaks.WalkersPillar,
                Peaks.Eldenhorn,
                Peaks.GreatGaol,
                Peaks.StHaelga,
                Peaks.YmirsShadow,
                Peaks.GreatBulwark,
                Peaks.SolemnTempest,
                Peaks.EinvaldFalls,
                Peaks.AlmattrDam,
                Peaks.Dunderhorn,
                Peaks.MhorDruim,
                Peaks.WelkinPass,
                Peaks.SeigrCraeg,
                Peaks.UllrsChasm,
                Peaks.GreatSilf,
                Peaks.ToweringVisir,
                Peaks.EldrisWall,
                Peaks.MountMhorgorm
            };

            Peaks peak = (Peaks)p;

            Connection.Instance.CompletePeakLocation(peak);

            if (FSPeaks.Contains(peak) && GameObject.FindGameObjectWithTag("Player").GetComponent<RopeAnchor>().ropesPlacedDuringMap == 0)
            {
                Connection.Instance.completeFSPeakLocation(peak);
            }
        }
    }
}
