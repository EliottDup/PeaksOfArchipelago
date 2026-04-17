using Archipelago.MultiClient.Net.Models;
using PeaksOfArchipelago.GameData;

namespace PeaksOfArchipelago.CabinHandlers
{
    internal class AlpsCabinHandler(ISlotData slotData, SessionSettings settings) : CabinHandler(slotData, settings)
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
            // rember to do phonograph, crampons and sum more as well
            PeaksOfArchipelago.ui.SendNotification($"Alps not implemented, please return to the base cabin");
        }

        internal override void LoadArtefacts()
        {
            throw new NotImplementedException();
        }
    }
}