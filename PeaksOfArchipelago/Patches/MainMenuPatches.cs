using HarmonyLib;
using PeaksOfArchipelago.Session;
using UnityEngine;
using UnityEngine.UI;

namespace PeaksOfArchipelago.Patches
{
    [HarmonyPatch(typeof(MenuSaveManager))]
    internal class MenuSaveManagerPatches
    {
        [HarmonyPostfix]
        //[HarmonyPatch(nameof(MenuSaveManager.GoToModes))]
        [HarmonyPatch(nameof(MenuSaveManager.CheckModesUnlocked))]
        public static void GoToModesPostfix(MenuSaveManager __instance)
        {
            Transform buttonLayout = __instance.modeSelectMenu.transform.Find("ButtonLayout");
            Transform normalButton = buttonLayout.Find("NormalPlaythrough") ?? buttonLayout.GetChild(0);
            foreach (Transform t in buttonLayout)
            {
                if (t.name != normalButton.name)
                {
                    t.gameObject.SetActive(false);
                }
            }

            normalButton.GetComponentInChildren<Text>().text = "Archipelago";
        }
        
        // Problem, entering GoToModes don't always work

        [HarmonyPostfix]
        [HarmonyPatch(nameof(MenuSaveManager.GoToSlots))]
        public static void GoToSlotsPostfix(MenuSaveManager __instance)
        {
            if (Connection.Instance == null)
            {
                PeaksOfArchipelago.Logger.LogFatal("Connection should not be null :(");
                return;
            }
            int saveSlot = Connection.Instance.GetSaveSlot();
            UnityEngine.UI.Button[] slotButtons = __instance.slotButtons;

            for (int i = 0; i < 3; i++)
            {
                bool slotIsEmpty = slotButtons[i].GetComponentInChildren<Text>().text.ToUpper().Equals("EMPTY");
                if (slotIsEmpty) PeaksOfArchipelago.Logger.LogInfo($"Slot {i} is empty");
                if (saveSlot == -1)
                {
                    slotButtons[i].interactable = slotIsEmpty;
                }
                else
                {
                    slotButtons[i].interactable = (i == saveSlot);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(MenuSaveManager.PlaySlot))]
        public static void PlaySlotPostfix(MenuSaveManager __instance, int slotToUse)
        {
            Connection.Instance.SetSaveSlot(slotToUse);
        }
    }
}
