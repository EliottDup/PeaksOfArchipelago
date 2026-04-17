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
        protected SessionSettings settings;

        public CabinHandler(ISlotData slotData, SessionSettings settings)
        {
            this.slotData = slotData;
            logger = PeaksOfArchipelago.Logger;
            this.settings = settings;
        }

        public static CabinHandler New(Cabins cabin, ISlotData slotData, SessionSettings settings)
        {
            return cabin switch
            {
                Cabins.Cabin => new BaseCabinHandler(slotData, settings),
                Cabins.CabinExpert => new ExpertCabinHandler(slotData, settings),
                Cabins.CabinAlps => new AlpsCabinHandler(slotData, settings),
                _ => throw new NotImplementedException(),
            };
        }

        internal abstract void LoadArtefacts();
    }
}
