using Archipelago.MultiClient.Net.Models;
using PeaksOfArchipelago.GameData;
using UnityEngine;

namespace PeaksOfArchipelago.CabinHandlers
{
    internal class ExpertCabinHandler(ISlotData slotData) : CabinHandler(slotData)
    {
        public override bool CollectItems(List<ItemInfo> itemInfos)
        {
            PeaksOfArchipelago.ui.SendNotification($"You have {itemInfos.Count} items awaiting you in your cabin, return there to collect them");
            return false;
        }

        public override void LoadProgress()
        {
            logger.LogInfo("Loading progress for Expert Cabin");

            //bulwark disabling
            Category4CabinLeave cabinLeave = GameObject.FindObjectOfType<Category4CabinLeave>();
            if (cabinLeave == null)
            {
                logger.LogError("Could not find Category4CabinLeave in Alps Cabin :\\");
                return;
            }
            cabinLeave.enabled = slotData.HasPeak(Peaks.GreatBulwark);
        }
    }
}