using HarmonyLib;
using PeaksOfArchipelago.Session;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

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
            Transform yfydButton = __instance.yfydButton.transform;
            foreach (Transform t in buttonLayout)
            {
                if (t.name != normalButton.name && t.name != yfydButton.name)
                {
                    t.gameObject.SetActive(false);
                }
            }

            normalButton.GetComponentInChildren<Text>().text = "Archipelago";
            SlotType slotType = Connection.Instance == null ? SlotType.None : Connection.Instance.GetSlotType();
            normalButton.GetComponent<UnityEngine.UI.Button>().interactable = slotType == SlotType.None || slotType == SlotType.Normal;
            yfydButton.GetComponent<UnityEngine.UI.Button>().interactable = slotType == SlotType.None || slotType == SlotType.YFYD;
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
            SlotType slotType = Connection.Instance.GetSlotType();

            UnityEngine.UI.Button[] slotButtons = __instance.slotButtons;

            bool slotCheck = MenuSaveManager.modeSelect == 0 && slotType == SlotType.Normal ||
                MenuSaveManager.modeSelect == 1 && slotType == SlotType.YFYD || 
                slotType == SlotType.None;

            for (int i = 0; i < 3; i++)
            {
                bool slotIsEmpty = slotButtons[i].GetComponentInChildren<Text>().text.ToUpper().Equals("EMPTY");
                if (slotIsEmpty) PeaksOfArchipelago.Logger.LogInfo($"Slot {i} is empty");
                if (saveSlot == -1)
                {
                    slotButtons[i].interactable = slotIsEmpty && slotCheck;
                }
                else
                {
                    slotButtons[i].interactable = (i == saveSlot) && slotCheck;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(MenuSaveManager.PlaySlot))]
        public static void PlaySlotPostfix(MenuSaveManager __instance, int slotToUse)
        {
            Connection.Instance.SetSaveSlot(slotToUse);
            switch (MenuSaveManager.modeSelect)
            {
                case 0:
                    Connection.Instance.SetSlotType(SlotType.Normal);
                    break;
                case 1:
                    Connection.Instance.SetSlotType(SlotType.YFYD);
                    break;
                default:
                    Connection.Instance.SetSlotType(SlotType.None);
                    break;
            }
        }
    }
}
