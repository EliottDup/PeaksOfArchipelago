using PeaksOfArchipelago.GameData;

namespace PeaksOfArchipelago.CabinHandlers
{
    internal class BaseCabinHandler : CabinHandler
    {

        public BaseCabinHandler(ISlotData slotData): base(slotData)
        {
        }

        public override void OnEnterCabin()
        {
            // If not alps, disable ticket & suitcase

            throw new NotImplementedException();
        }
    }
}