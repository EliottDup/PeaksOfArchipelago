using Archipelago.MultiClient.Net.Models;
using BepInEx.Logging;
using PeaksOfArchipelago.GameData;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
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

        protected struct ArtefactInfo
        {
            public ArtefactInfo(Artefacts artefact, GameObject cleanObject, GameObject dirtyObject, bool isDirty, bool hasArtefact)
            {
                this.artefact = artefact;
                this.cleanObject = cleanObject;
                this.dirtyObject = dirtyObject;
                this.isDirty = isDirty;
                this.hasArtefact = hasArtefact;
            }

            public Artefacts artefact;
            public GameObject cleanObject;
            public GameObject dirtyObject;
            public bool isDirty;
            public bool hasArtefact;
        }

        public CabinHandler(ISlotData slotData)
        {
            this.slotData = slotData;
            logger = PeaksOfArchipelago.Logger;
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
