using Archipelago.MultiClient.Net.Models;
using PeaksOfArchipelago.GameData;

namespace PeaksOfArchipelago.CabinHandlers
{
    internal class AlpsCabinHandler : CabinHandler
    {
        public AlpsCabinHandler(ISlotData slotData) : base(slotData)
        {
        }

        public override void CollectItems(List<ItemInfo> itemInfos)
        {
            throw new NotImplementedException();
        }

        public override void LoadProgress()
        {
            throw new NotImplementedException();
        }
    }
}