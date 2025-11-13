using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace PeaksOfArchipelago.Patches
{
    [HarmonyPatch(typeof(NPCEvents))]
    internal class NPCEventsPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("CheckProgress")]
        public static bool CabinEventDisabler()
        {
            PeaksOfArchipelago.Logger.LogInfo("Disabled Cabin CheckProgress!");
            return false;
        }
    }

    [HarmonyPatch(typeof(ArtefactLoaderCabin))]
    internal class ArtefactLoaderCabinPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("LoadArtefacts")]
        public static bool ArtefactLoaderCabinDisabler()
        {
            PeaksOfArchipelago.Logger.LogInfo("Disabled artefact loading!");
            return false;
        }
    }
}
