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

        public override bool HandleCompletion()
        {
            PeaksOfArchipelago.ui.SendNotification("You have completed your goal, return to your cabin to free your items.");
            return false;
        }

        public override void LoadProgress()
        {
            logger.LogInfo("Loading progress for Expert Cabin");

            if (!slotData.HasTool(Tools.IceAxes))
            {
                PeaksOfArchipelago.ui.SendNotification("You have not unlocked the Ice Axes, you will not be able to progress here");
            }

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