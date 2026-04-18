using Archipelago.MultiClient.Net.Models;
using PeaksOfArchipelago.Assets;
using PeaksOfArchipelago.GameData;
using UnityEngine;
using UnityEngine.UI;

namespace PeaksOfArchipelago.CabinHandlers
{
    internal class AlpsCabinHandler(ISlotData slotData) : CabinHandler(slotData)
    {

        bool hasSpawnedInAPLogo = false;

        public override bool CollectItems(List<ItemInfo> itemInfos)
        {
            itemInfos = itemInfos.Where(i => i.ItemName != "Trap").ToList();

            if (itemInfos.Count == 0)
            {
                return true;
            }
            AlpsEvents alpsEvents = GameObject.FindObjectOfType<AlpsEvents>();

            alpsEvents.currentEventName = "Alps_Medal1"; // Hijacking the AllArtefacts event for AP parcel delivery

            alpsEvents.itemDisplay_MedalTitle.text = "- Archipelago Item Delivery -";
            alpsEvents.itemDisplay_MedalDescription.text = $"You have received:\n {itemInfos[0].ItemName}";

            for (int i = 1; i < itemInfos.Count; i++)
            {
                alpsEvents.itemDisplay_MedalDescription.text += $", {itemInfos[i].ItemName}";
            }

            if (!hasSpawnedInAPLogo)
            {
                GameObject apLogo = GameObject.Instantiate(PeaksOfAssets.APLogo);
                apLogo.layer = alpsEvents.alps_itemDisplay_Medal1.layer;
                apLogo.transform.GetChild(0).gameObject.layer = alpsEvents.alps_itemDisplay_Medal1.layer;

                apLogo.transform.SetParent(alpsEvents.alps_itemDisplay_Medal1.transform.parent, false);

                apLogo.transform.localPosition = alpsEvents.alps_itemDisplay_Medal1.transform.localPosition;
                apLogo.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
                apLogo.transform.localRotation = Quaternion.Euler(270, 270, 0);
                alpsEvents.alps_itemDisplay_Medal1 = apLogo;
                alpsEvents.alps_itemDisplay_Medal1.SetActive(false);
                hasSpawnedInAPLogo = true;
            }

            alpsEvents.StartCoroutine("UnpackParcel");
            return true;
        }

        public override bool HandleCompletion()
        {
            return false;
        }

        public override void LoadProgress()
        {
            // Books loading
            AlpsJournal alpsJournal = GameObject.FindObjectOfType<AlpsJournal>();
            if (!alpsJournal)
            {
                logger.LogError("Could not find Alps Journal, progress will not be loaded");
                return;
            }

            // Why dear god
            // for some reason ALL THREE BOOKS in the alps are controlled by A SINGULAR INSTANCE OF A SCRIPT that keeps track of all 3 book.

            // I yearn for blood
            if (!slotData.HasBook(Books.Essentials))
            {
                foreach (Transform c in alpsJournal.transform)
                {
                    c.gameObject.SetActive(false);
                }
                alpsJournal.transform.parent.GetComponent<BoxCollider>().enabled = false;
                alpsJournal.transform.parent.parent.parent.GetChild(1).gameObject.SetActive(false);
            }

            alpsJournal.AlpsJournal2.SetActive(slotData.HasBook(Books.AlpineGreats));
            alpsJournal.AlpsJournal3.SetActive(slotData.HasBook(Books.ArduousArctic));

            AlpsEvents alpsEvents = GameObject.FindObjectOfType<AlpsEvents>();

            alpsEvents.alps_freesolo_medal.SetActive(false);
            alpsEvents.alps_timeattack_medal1.SetActive(false);
            alpsEvents.alps_timeattack_medal2.SetActive(false);
            alpsEvents.alps_timeattack_medal3.SetActive(false);
            alpsEvents.alps_medal1.SetActive(false);
            alpsEvents.alps_medal2.SetActive(false);
            alpsEvents.alps_medal3.SetActive(false);

            // Tool loading
            ArtefactLoaderCabin alc = GameObject.FindObjectOfType<ArtefactLoaderCabin>();


            alpsEvents.phonographMusic?.transform.parent.gameObject.SetActive(slotData.HasTool(Tools.Phonograph));
            alc.alpcabin_ropes.SetActive(slotData.HasTool(Tools.Rope));
            alc.alpcabin_ropesUpgraded.SetActive(slotData.HasTool(Tools.RopeLengthUpgrade));
            alc.alpcabin_crampons.SetActive(slotData.cramponLevel > 0);
            alc.alpcabin_iceaxes.SetActive(slotData.HasTool(Tools.IceAxes));
            alc.alpcabin_barometer.SetActive(slotData.HasTool(Tools.Barometer));
            alc.alpcabin_chalkbag.SetActive(slotData.HasTool(Tools.Chalkbag));
            alc.alpcabin_coffee.SetActive(slotData.HasTool(Tools.Coffee));
            alc.alpcabin_seeds.SetActive(GameManager.control.extraBirdSeedUses > 0);
            
            alc.alpcabin_barometer.transform.parent.gameObject.SetActive(true);

            if (!slotData.HasTool(Tools.Pipe))
            {
                GameManager.control.isUsingPipe = false;
            }
        }

        private struct IdolInfo
        {
            public IdolInfo(Idols idol, GameObject fullObject, ArtefactInfo p1, ArtefactInfo p2)
            {
                this.idol = idol;
                this.fullObject = fullObject;
                this.p1 = p1;
                this.p2 = p2;
            }

            public Idols idol;
            public GameObject fullObject;
            public ArtefactInfo p1, p2;
        }

        internal override void LoadArtefacts()
        {

            ArtefactLoaderCabin alc = GameObject.FindObjectOfType<ArtefactLoaderCabin>();

            // Idol loading
            List<IdolInfo> idols = new List<IdolInfo>() { 
                new IdolInfo(Idols.Crimps, alc.alps_clean_statue_crimps_complete,
                    new ArtefactInfo(Artefacts.Alps_Idol_crimpsPt1, alc.alps_clean_statue_crimps_pt1, alc.alps_dirty_statue_crimps_pt1, GameManager.control.alps_statue_crimps_IsDirty_pt1, GameManager.control.alps_hasStatue_crimps_pt1),
                    new ArtefactInfo(Artefacts.Alps_Idol_crimpsPt2, alc.alps_clean_statue_crimps_pt2, alc.alps_dirty_statue_crimps_pt2, GameManager.control.alps_statue_crimps_IsDirty_pt2, GameManager.control.alps_hasStatue_crimps_pt2)
                    ),
                new IdolInfo(Idols.Slopers, alc.alps_clean_statue_slopers_complete,
                    new ArtefactInfo(Artefacts.Alps_Idol_slopersPt1, alc.alps_clean_statue_slopers_pt1, alc.alps_dirty_statue_slopers_pt1, GameManager.control.alps_statue_slopers_IsDirty_pt1, GameManager.control.alps_hasStatue_slopers_pt1),
                    new ArtefactInfo(Artefacts.Alps_Idol_slopersPt2, alc.alps_clean_statue_slopers_pt2, alc.alps_dirty_statue_slopers_pt2, GameManager.control.alps_statue_slopers_IsDirty_pt2, GameManager.control.alps_hasStatue_slopers_pt2)
                    ),
                new IdolInfo(Idols.Feathers, alc.alps_clean_statue_feathers_complete,
                    new ArtefactInfo(Artefacts.Alps_Idol_feathersPt1, alc.alps_clean_statue_feathers_pt1, alc.alps_dirty_statue_feathers_pt1, GameManager.control.alps_statue_feathers_IsDirty_pt1, GameManager.control.alps_hasStatue_feathers_pt1),
                    new ArtefactInfo(Artefacts.Alps_Idol_feathersPt2, alc.alps_clean_statue_feathers_pt2, alc.alps_dirty_statue_feathers_pt2, GameManager.control.alps_statue_feathers_IsDirty_pt2, GameManager.control.alps_hasStatue_feathers_pt2)
                    ),
                new IdolInfo(Idols.Pitches, alc.alps_clean_statue_pitches_complete,
                    new ArtefactInfo(Artefacts.Alps_Idol_pitchesPt1, alc.alps_clean_statue_pitches_pt1, alc.alps_dirty_statue_pitches_pt1, GameManager.control.alps_statue_pitches_IsDirty_pt1, GameManager.control.alps_hasStatue_pitches_pt1),
                    new ArtefactInfo(Artefacts.Alps_Idol_pitchesPt2, alc.alps_clean_statue_pitches_pt2, alc.alps_dirty_statue_pitches_pt2, GameManager.control.alps_statue_pitches_IsDirty_pt2, GameManager.control.alps_hasStatue_pitches_pt2)
                    ),
                new IdolInfo(Idols.Ice, alc.alps_clean_statue_ice_complete,
                    new ArtefactInfo(Artefacts.Alps_Idol_icePt1, alc.alps_clean_statue_ice_pt1, alc.alps_dirty_statue_ice_pt1, GameManager.control.alps_statue_ice_IsDirty_pt1, GameManager.control.alps_hasStatue_ice_pt1),
                    new ArtefactInfo(Artefacts.Alps_Idol_icePt2, alc.alps_clean_statue_ice_pt2, alc.alps_dirty_statue_ice_pt2, GameManager.control.alps_statue_ice_IsDirty_pt2, GameManager.control.alps_hasStatue_ice_pt2)
                    ),
                new IdolInfo(Idols.Pinches, alc.alps_clean_statue_pinches_complete,
                    new ArtefactInfo(Artefacts.Alps_Idol_pinchesPt1, alc.alps_clean_statue_pinches_pt1, alc.alps_dirty_statue_pinches_pt1, GameManager.control.alps_statue_pinches_IsDirty_pt1, GameManager.control.alps_hasStatue_pinches_pt1),
                    new ArtefactInfo(Artefacts.Alps_Idol_pinchesPt2, alc.alps_clean_statue_pinches_pt2, alc.alps_dirty_statue_pinches_pt2, GameManager.control.alps_statue_pinches_IsDirty_pt2, GameManager.control.alps_hasStatue_pinches_pt2)
                    ),
                new IdolInfo(Idols.GreaterBalance, alc.alps_clean_statue_greaterbalance_complete,
                    new ArtefactInfo(Artefacts.Alps_Idol_greaterbalancePt1, alc.alps_clean_statue_greaterbalance_pt1, alc.alps_dirty_statue_greaterbalance_pt1, GameManager.control.alps_statue_greaterbalance_IsDirty_pt1, GameManager.control.alps_hasStatue_greaterbalance_pt1),
                    new ArtefactInfo(Artefacts.Alps_Idol_greaterbalancePt2, alc.alps_clean_statue_greaterbalance_pt2, alc.alps_dirty_statue_greaterbalance_pt2, GameManager.control.alps_statue_greaterbalance_IsDirty_pt2, GameManager.control.alps_hasStatue_greaterbalance_pt2)
                    ),
                new IdolInfo(Idols.Sundown, alc.alps_clean_statue_sundown_complete,
                    new ArtefactInfo(Artefacts.Alps_Idol_sundownPt1, alc.alps_clean_statue_sundown_pt1, alc.alps_dirty_statue_sundown_pt1, GameManager.control.alps_statue_sundown_IsDirty_pt1, GameManager.control.alps_hasStatue_sundown_pt1),
                    new ArtefactInfo(Artefacts.Alps_Idol_sundownPt2, alc.alps_clean_statue_sundown_pt2, alc.alps_dirty_statue_sundown_pt2, GameManager.control.alps_statue_sundown_IsDirty_pt2, GameManager.control.alps_hasStatue_sundown_pt2)
                    ),
                new IdolInfo(Idols.Seeds, alc.alps_clean_statue_seeds_complete,
                    new ArtefactInfo(Artefacts.Alps_Idol_seedsPt1, alc.alps_clean_statue_seeds_pt1, alc.alps_dirty_statue_seeds_pt1, GameManager.control.alps_statue_seeds_IsDirty_pt1, GameManager.control.alps_hasStatue_seeds_pt1),
                    new ArtefactInfo(Artefacts.Alps_Idol_seedsPt2, alc.alps_clean_statue_seeds_pt2, alc.alps_dirty_statue_seeds_pt2, GameManager.control.alps_statue_seeds_IsDirty_pt2, GameManager.control.alps_hasStatue_seeds_pt2)
                    ),
                new IdolInfo(Idols.Gravity, alc.alps_clean_statue_gravity_complete,
                    new ArtefactInfo(Artefacts.Alps_Idol_gravityPt1, alc.alps_clean_statue_gravity_pt1, alc.alps_dirty_statue_gravity_pt1, GameManager.control.alps_statue_gravity_IsDirty_pt1, GameManager.control.alps_hasStatue_gravity_pt1),
                    new ArtefactInfo(Artefacts.Alps_Idol_gravityPt2, alc.alps_clean_statue_gravity_pt2, alc.alps_dirty_statue_gravity_pt2, GameManager.control.alps_statue_gravity_IsDirty_pt2, GameManager.control.alps_hasStatue_gravity_pt2)
                    ),
            };
            
            foreach (IdolInfo i in idols)
            {
                i.fullObject.SetActive(false);

                i.p1.cleanObject.SetActive(false);
                i.p1.dirtyObject.SetActive(false);


                i.p2.cleanObject.SetActive(false);
                i.p2.dirtyObject.SetActive(false);


                if (i.p1.hasArtefact)
                {
                    i.p1.dirtyObject.SetActive(i.p1.isDirty);
                    i.p1.cleanObject.SetActive(!i.p1.isDirty);
                }

                if (i.p2.hasArtefact)
                {
                    i.p2.dirtyObject.SetActive(i.p2.isDirty);
                    i.p2.cleanObject.SetActive(!i.p2.isDirty);
                }

                if (slotData.HasIdol(i.idol))
                {
                    i.fullObject.SetActive(true);

                    i.p1.cleanObject.SetActive(false);
                    i.p2.cleanObject.SetActive(false);
                }
            }

            Dictionary<Artefacts, bool> gentianas = new Dictionary<Artefacts, bool>() {
                {Artefacts.Alps_Gentiana1, GameManager.control.alps_gentiana1 },
                {Artefacts.Alps_Gentiana2, GameManager.control.alps_gentiana2 },
                {Artefacts.Alps_Gentiana3, GameManager.control.alps_gentiana3 },
                {Artefacts.Alps_Gentiana4, GameManager.control.alps_gentiana4 },
                {Artefacts.Alps_Gentiana5, GameManager.control.alps_gentiana5 },
                {Artefacts.Alps_Gentiana6, GameManager.control.alps_gentiana6 },
                {Artefacts.Alps_Gentiana7, GameManager.control.alps_gentiana7 },
            };

            for (int i = 0; i < gentianas.Count; i++)
            {
                bool hasGentiana = gentianas[(Artefacts)(i + Artefacts.Alps_Gentiana1)];
                if (hasGentiana)
                {
                    alc.alpsflowerpot_gentiana.SetActive(true);
                }
                alc.alpsflower_gentiana[i].SetActive(hasGentiana);
            }

            Dictionary<Artefacts, bool> edelweisses = new Dictionary<Artefacts, bool>() {
                {Artefacts.Alps_Edelweiss1, GameManager.control.alps_edelweiss1 },
                {Artefacts.Alps_Edelweiss2, GameManager.control.alps_edelweiss2 },
                {Artefacts.Alps_Edelweiss3, GameManager.control.alps_edelweiss3 },
                {Artefacts.Alps_Edelweiss4, GameManager.control.alps_edelweiss4 },
                {Artefacts.Alps_Edelweiss5, GameManager.control.alps_edelweiss5 },
                {Artefacts.Alps_Edelweiss6, GameManager.control.alps_edelweiss6 },
                {Artefacts.Alps_Edelweiss7, GameManager.control.alps_edelweiss7 },
            };

            for (int i = 0; i < edelweisses.Count; i++)
            {
                bool hasEdeweiss = edelweisses[(Artefacts)(i + Artefacts.Alps_Edelweiss1)];
                if (hasEdeweiss)
                {
                    alc.alpsflowerpot_edelweiss.SetActive(true);
                }
                alc.alpsflower_edelweiss[i].SetActive(hasEdeweiss);
            }
        }
    }
}