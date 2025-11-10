using PeaksOfArchipelago.GameData;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;

namespace PeaksOfArchipelago.CabinHandlers
{
    internal abstract class CabinHandler
    {
        public abstract void OnEnterCabin();

        protected ISlotData slotData;

        public CabinHandler(ISlotData slotData)
        {
            this.slotData = slotData;
        }
        public static CabinHandler New(Cabins cabin, ISlotData slotData)
        {
            return cabin switch
            {
                Cabins.Cabin => new BaseCabinHandler(slotData),
                Cabins.CabinExpert => new ExpertCabinHandler(slotData),
                Cabins.CabinAlps => new AlpsCabinHandler(slotData),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
