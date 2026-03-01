using Archipelago.MultiClient.Net.Models;
using PeaksOfArchipelago.GameData;

namespace PeaksOfArchipelago.CabinHandlers
{
    internal class AlpsCabinHandler(ISlotData slotData) : CabinHandler(slotData)
    {
        public override bool CollectItems(List<ItemInfo> itemInfos)
        {
            PeaksOfArchipelago.ui.SendNotification($"Alps not implemented, please return to the base cabin");
            return false;
        }

        public override bool HandleCompletion()
        {
            PeaksOfArchipelago.ui.SendNotification($"Alps not implemented, please return to the base cabin");
            return false;
        }

        public override void LoadProgress()
        {
            PeaksOfArchipelago.ui.SendNotification($"Alps not implemented, please return to the base cabin");
        }
    }
}