using Archipelago.MultiClient.Net.Models;
using BepInEx.Logging;
using PeaksOfArchipelago.GameData;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;

namespace PeaksOfArchipelago.CabinHandlers
{
    internal abstract class CabinHandler
    {
        public abstract void LoadProgress();
        public abstract bool CollectItems(List<ItemInfo> itemInfos);
        public abstract bool HandleCompletion();

        protected ManualLogSource logger;

        protected ISlotData slotData;

        public CabinHandler(ISlotData slotData)
        {
            this.slotData = slotData;
            logger = PeaksOfArchipelago.Logger;
            this.settings = settings;
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

        internal abstract void LoadArtefacts();
    }
}
