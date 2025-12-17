using HarmonyLib;
using System.Reflection;
using PeaksOfArchipelago.Session;
using PeaksOfArchipelago.GameData;
using UnityEngine;

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

    // Fundamentals blocking

    [HarmonyPatch(typeof(PeakSelection))]
    internal class PeakSelectionPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnMouseSpriteDown")]
        static bool DisableClick(PeakSelection __instance)
        {
            Type type = __instance.GetType();

            PeakJournal journal =  (PeakJournal)type.GetField("peakJournal", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            if ((__instance.leftPage && !Connection.Instance.slotData.IsJournalPageUnlocked(journal.currentPage, Books.Fundamentals)) || 
                (!__instance.leftPage && !Connection.Instance.slotData.IsJournalPageUnlocked(journal.currentPage + 1, Books.Fundamentals)))
            {
                PeaksOfArchipelago.Logger.LogInfo($"LeftPage: {__instance.leftPage}");
                PeaksOfArchipelago.Logger.LogInfo($"Page: {journal.currentPage}, Peak: {Connection.Instance.slotData.BookPageToPeaks(journal.currentPage, Books.Fundamentals)}");
                PeaksOfArchipelago.Logger.LogInfo($"Page: {journal.currentPage + 1}, Peak: {Connection.Instance.slotData.BookPageToPeaks(journal.currentPage + 1, Books.Fundamentals)}");
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnMouseSpriteOver")]
        public static void ColorHighLight(PeakSelection __instance)
        {
            Type type = __instance.GetType();


            PeakJournal journal = (PeakJournal)type.GetField("peakJournal", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            MeshRenderer leftRenderer = journal.leftSelectOutlineOBJ.GetComponent<MeshRenderer>();
            MeshRenderer rightRenderer = journal.rightSelectOutlineOBJ.GetComponent<MeshRenderer>();

            if (__instance.leftPage && !Connection.Instance.slotData.IsJournalPageUnlocked(journal.currentPage, Books.Fundamentals))
                leftRenderer.material.color = new Color(2, 0, 0);
            else 
                leftRenderer.material.color = new Color(1, 1, 1, 0.349f);
            
            if (!__instance.leftPage && !Connection.Instance.slotData.IsJournalPageUnlocked(journal.currentPage + 1, Books.Fundamentals))
                rightRenderer.material.color = new Color(2, 0, 0);
            else
                rightRenderer.material.color = new Color(1, 1, 1, 0.349f);
        }
    }


    [HarmonyPatch(typeof(PeakJournal))]
    internal class PageColorationPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("PageTurnSound")]
        public static void PageTurnColoration(PeakJournal __instance)
        {
            Animation anim = __instance.journalPageAnim;
            AnimationClip left_anim = __instance.journalPage_TurnLeft;
            
            Material leftPage = __instance.leftPage;
            Material rightPage = __instance.rightPage;
            Material frontPage = __instance.frontPage;
            Material backPage = __instance.backPage;

            int currentPage = __instance.currentPage;

            Books book = Books.Fundamentals;

            if (anim.clip == left_anim)
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
        public static void PageOpeningColor(PeakJournal __instance)
        {
            Type type = __instance.GetType();

            Material leftPage = __instance.leftPage;
            Material rightPage = __instance.rightPage;
            Material frontPage = __instance.frontPage;
            Material backPage = __instance.backPage;

            int currentPage = __instance.currentPage;

            Books book = Books.Fundamentals;

            backPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 2, book);
            frontPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 1, book);
            rightPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 3, book);
            leftPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 0, book);
        }
    }

    // Intermediate blocking

    [HarmonyPatch(typeof(IntermediatePeakSelection))]
    internal class IntermediatePeakSelectionPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnMouseSpriteDown")]
        static bool DisableClick(IntermediatePeakSelection __instance)
        {
            Type type = __instance.GetType();

            IntermediateJournal journal = __instance.peakJournal;

            Books book = Books.Intermediate;

            if ((__instance.leftPage && !Connection.Instance.slotData.IsJournalPageUnlocked(journal.currentPage, book)) ||
                (!__instance.leftPage && !Connection.Instance.slotData.IsJournalPageUnlocked(journal.currentPage + 1, book)))
            {
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnMouseSpriteOver")]
        public static void ColorHighLight(IntermediatePeakSelection __instance)
        {
            Type type = __instance.GetType();

            IntermediateJournal journal = __instance.peakJournal;

            MeshRenderer leftRenderer = journal.leftSelectOutlineOBJ.GetComponent<MeshRenderer>();
            MeshRenderer rightRenderer = journal.rightSelectOutlineOBJ.GetComponent<MeshRenderer>();

            Books book = Books.Intermediate;

            if (__instance.leftPage && !Connection.Instance.slotData.IsJournalPageUnlocked(journal.currentPage, book))
                leftRenderer.material.color = new Color(2, 0, 0);
            else
                leftRenderer.material.color = new Color(1, 1, 1, 0.349f);

            if (!__instance.leftPage && !Connection.Instance.slotData.IsJournalPageUnlocked(journal.currentPage + 1, book))
                rightRenderer.material.color = new Color(2, 0, 0);
            else
                rightRenderer.material.color = new Color(1, 1, 1, 0.349f);
        }
    }


    [HarmonyPatch(typeof(IntermediateJournal))]
    internal class IntermediatePageColorationPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("PageTurnSound")]
        public static void PageTurnColoration(IntermediateJournal __instance)
        {
            Animation anim = __instance.journalPageAnim;
            AnimationClip left_anim = __instance.journalPage_TurnLeft;

            Material leftPage = __instance.leftPage;
            Material rightPage = __instance.rightPage;
            Material frontPage = __instance.frontPage;
            Material backPage = __instance.backPage;

            int currentPage = __instance.currentPage;

            Books book = Books.Intermediate;


            if (anim.clip == left_anim)
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
        public static void PageOpeningColor(IntermediateJournal __instance)
        {
            Material leftPage = __instance.leftPage;
            Material rightPage = __instance.rightPage;
            Material frontPage = __instance.frontPage;
            Material backPage = __instance.backPage;

            int currentPage = __instance.currentPage;

            Books book = Books.Intermediate;

            backPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 2, book);
            frontPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 1, book);
            rightPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 3, book);
            leftPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 0, book);
        }
    }

    // Advanced blocking

    [HarmonyPatch(typeof(AdvancedPeakSelection))]
    internal class AdvancedPeakSelectionPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnMouseSpriteDown")]
        static bool DisableClick(AdvancedPeakSelection __instance)
        {
            Type type = __instance.GetType();

            AdvancedJournal journal = __instance.peakJournal;

            Books book = Books.Advanced;

            if ((__instance.leftPage && !Connection.Instance.slotData.IsJournalPageUnlocked(journal.currentPage, book)) ||
                (!__instance.leftPage && !Connection.Instance.slotData.IsJournalPageUnlocked(journal.currentPage + 1, book)))
            {
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnMouseSpriteOver")]
        public static void ColorHighLight(AdvancedPeakSelection __instance)
        {
            Type type = __instance.GetType();

            AdvancedJournal journal = __instance.peakJournal;

            MeshRenderer leftRenderer = journal.leftSelectOutlineOBJ.GetComponent<MeshRenderer>();
            MeshRenderer rightRenderer = journal.rightSelectOutlineOBJ.GetComponent<MeshRenderer>();

            Books book = Books.Advanced;

            if (__instance.leftPage && !Connection.Instance.slotData.IsJournalPageUnlocked(journal.currentPage, book))
                leftRenderer.material.color = new Color(2, 0, 0);
            else
                leftRenderer.material.color = new Color(1, 1, 1, 0.349f);

            if (!__instance.leftPage && !Connection.Instance.slotData.IsJournalPageUnlocked(journal.currentPage + 1, book))
                rightRenderer.material.color = new Color(2, 0, 0);
            else
                rightRenderer.material.color = new Color(1, 1, 1, 0.349f);
        }
    }


    [HarmonyPatch(typeof(AdvancedJournal))]
    internal class AdvancedPageColorationPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("PageTurnSound")]
        public static void PageTurnColoration(AdvancedJournal __instance)
        {
            Animation anim = __instance.journalPageAnim;
            AnimationClip left_anim = __instance.journalPage_TurnLeft;

            Material leftPage = __instance.leftPage;
            Material rightPage = __instance.rightPage;
            Material frontPage = __instance.frontPage;
            Material backPage = __instance.backPage;

            int currentPage = __instance.currentPage;

            Books book = Books.Advanced;


            if (anim.clip == left_anim)
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
        public static void PageOpeningColor(AdvancedJournal __instance)
        {
            Material leftPage = __instance.leftPage;
            Material rightPage = __instance.rightPage;
            Material frontPage = __instance.frontPage;
            Material backPage = __instance.backPage;

            int currentPage = __instance.currentPage;

            Books book = Books.Advanced;

            backPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 2, book);
            frontPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 1, book);
            rightPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 3, book);
            leftPage.color = Connection.Instance.slotData.GetJournalPageColor(currentPage + 0, book);
        }
    }
}
