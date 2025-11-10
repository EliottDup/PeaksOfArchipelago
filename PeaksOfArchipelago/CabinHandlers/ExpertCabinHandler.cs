using PeaksOfArchipelago.GameData;

namespace PeaksOfArchipelago.CabinHandlers
{
    internal class ExpertCabinHandler : CabinHandler
    {
        public ExpertCabinHandler(ISlotData slotData) : base(slotData)
        {
        }

        public override void OnEnterCabin()
        {
            throw new NotImplementedException();
        }
    }
}