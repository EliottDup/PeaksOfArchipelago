using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using System.Reflection;
using PeaksOfArchipelago.Session;
using PeaksOfArchipelago.GameData;
using UnityEngine;
using UnityEngine.UIElements;

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

    [HarmonyPatch(typeof(PeakSelection))]
    [HarmonyPatch(typeof(IntermediatePeakSelection))]
    [HarmonyPatch(typeof(AdvancedPeakSelection))]
    [HarmonyPatch(typeof(PeakSelection))]
    internal class PeakSelectionPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnMouseSpriteDown")]
        static bool DisableClick(object __instance)
        {
            Type type = __instance.GetType();
            var journal = type.GetField("peakJournal", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            // type: (Intermediate/Advanced/none)Journal
            PeaksOfArchipelago.Logger.LogInfo($"Journal Type: {journal.GetType().FullName}");
            
            int currentPage = (int)journal.GetType().GetField("currentPage", BindingFlags.Public | BindingFlags.Instance).GetValue(journal);
            bool leftPage = (bool)type.GetField("leftPage", BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);

            Books b = GetMatchingBook(type);

            if ((leftPage && !Connection.Instance.slotData.IsJournalPageUnlocked(currentPage, b)) || 
                (!leftPage && !Connection.Instance.slotData.IsJournalPageUnlocked(currentPage+1, b)))
            {
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnMouseSpriteOver")]
        public static void ColorHighLight(object __instance) {
            Type type = __instance.GetType();
            var journal = type.GetField("peakJournal", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            
            int currentPage = (int)journal.GetType().GetField("currentPage", BindingFlags.Public | BindingFlags.Instance).GetValue(journal);
            bool leftPage = (bool)type.GetField("leftPage", BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);

            MeshRenderer leftRenderer = ((GameObject)type.GetField("rightSelectOutlineOBJ", 
                BindingFlags.Public | BindingFlags.Instance).GetValue(__instance)).GetComponent<MeshRenderer>();
            MeshRenderer rightRenderer = ((GameObject) type.GetField("leftSelectOutlineOBJ", 
                BindingFlags.Public | BindingFlags.Instance).GetValue(__instance)).GetComponent<MeshRenderer>();

            if (leftPage && !Connection.Instance.slotData.IsJournalPageUnlocked(currentPage, GetMatchingBook(type)))
                leftRenderer.material.color = new Color(2, 0, 0);
            else 
                leftRenderer.material.color = new Color(1, 1, 1, 0.349f);
            
            if (!leftPage && !Connection.Instance.slotData.IsJournalPageUnlocked(currentPage + 1, GetMatchingBook(type)))
                rightRenderer.material.color = new Color(2, 0, 0);
            else
                rightRenderer.material.color = new Color(1, 1, 1, 0.349f);
        }

        static Books GetMatchingBook(Type peakSelectionType)
        {
            if (peakSelectionType == typeof(PeakSelection))             return Books.Fundamentals;
            if (peakSelectionType == typeof(IntermediatePeakSelection)) return Books.Intermediate;
            if (peakSelectionType == typeof(AdvancedPeakSelection))     return Books.Advanced;
            throw new ArgumentException("Unknown PeakSelection type");
        }
    }


    // TODO: PeakJournal next
    [HarmonyPatch(typeof(PeakJournal))]
    [HarmonyPatch(typeof(IntermediateJournal))]
    [HarmonyPatch(typeof(AdvancedJournal))]
    internal class PageColorationPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("PageTurnSound")]
        public static void PageTurnColoration(object __instance)
        {
            Type type = __instance.GetType();
            Animation anim = (Animation)type.GetField("journalPageAnim", BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);
            AnimationClip left_anim = (AnimationClip)type.GetField("journalPage_TurnLeft", BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);
            
            Material leftPage = (Material)type.GetField("leftPage", BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);
            Material rightPage = (Material)type.GetField("rightPage", BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);
            Material frontPage = (Material)type.GetField("frontPage", BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);
            Material backPage = (Material)type.GetField("backPage", BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);

            int currentPage = (int)type.GetField("currentPage", BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);

            Books book = GetMatchingBook(type);

            if (anim == left_anim)
            {
                backPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 0, book);
                frontPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage - 1, book);
                rightPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 1, book);
                leftPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage - 2, book);
            }
            else
            {
                backPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 2, book);
                frontPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 1, book);
                rightPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 3, book);
                leftPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 0, book);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("UpdatePage")]
        public static void PageOpeningColor(object __instance)
        {
            Type type = __instance.GetType();

            Material leftPage = (Material)type.GetField("leftPage", BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);
            Material rightPage = (Material)type.GetField("rightPage", BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);
            Material frontPage = (Material)type.GetField("frontPage", BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);
            Material backPage = (Material)type.GetField("backPage", BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);

            int currentPage = (int)type.GetField("currentPage", BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);

            Books book = GetMatchingBook(type);

            backPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 2, book);
            frontPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 1, book);
            rightPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 3, book);
            leftPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 0, book);
        }

        static Books GetMatchingBook(Type peakJournalType)
        {
            if (peakJournalType == typeof(PeakJournal)) return Books.Fundamentals;
            if (peakJournalType == typeof(IntermediateJournal)) return Books.Intermediate;
            if (peakJournalType == typeof(AdvancedJournal)) return Books.Advanced;
            throw new ArgumentException("Unknown PeakSelection type");
        }
    }
}
