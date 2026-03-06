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
            // TODO: Transpiler to stop the text from being overridden again
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

            // Idol Loading
            ArtefactLoaderCabin alc = GameObject.FindObjectOfType<ArtefactLoaderCabin>();
            Dictionary<Artefacts, GameObject> idolsToPartsGameObjects = new()
            { // no partial loading of idol parts
                {Artefacts.Alps_Idol_crimpsPt1, alc.alps_clean_statue_crimps_pt1},
                {Artefacts.Alps_Idol_crimpsPt2, alc.alps_clean_statue_crimps_pt2},
                {Artefacts.Alps_Idol_slopersPt1, alc.alps_clean_statue_slopers_pt1},
                {Artefacts.Alps_Idol_slopersPt2, alc.alps_clean_statue_slopers_pt2},
                {Artefacts.Alps_Idol_feathersPt1, alc.alps_clean_statue_feathers_pt1},
                {Artefacts.Alps_Idol_feathersPt2, alc.alps_clean_statue_feathers_pt2},
                {Artefacts.Alps_Idol_pitchesPt1, alc.alps_clean_statue_pitches_pt1},
                {Artefacts.Alps_Idol_pitchesPt2, alc.alps_clean_statue_pitches_pt2},
                {Artefacts.Alps_Idol_icePt1, alc.alps_clean_statue_ice_pt1},
                {Artefacts.Alps_Idol_icePt2, alc.alps_clean_statue_ice_pt2},
                {Artefacts.Alps_Idol_pinchesPt1, alc.alps_clean_statue_pinches_pt1},
                {Artefacts.Alps_Idol_pinchesPt2, alc.alps_clean_statue_pinches_pt2},
                {Artefacts.Alps_Idol_greaterbalancePt1, alc.alps_clean_statue_greaterbalance_pt1},
                {Artefacts.Alps_Idol_greaterbalancePt2, alc.alps_clean_statue_greaterbalance_pt2},
                {Artefacts.Alps_Idol_sundownPt1, alc.alps_clean_statue_sundown_pt1},
                {Artefacts.Alps_Idol_sundownPt2, alc.alps_clean_statue_sundown_pt2},
                {Artefacts.Alps_Idol_seedsPt1, alc.alps_clean_statue_seeds_pt1},
                {Artefacts.Alps_Idol_seedsPt2, alc.alps_clean_statue_seeds_pt2},
                {Artefacts.Alps_Idol_gravityPt1, alc.alps_clean_statue_gravity_pt1},
                {Artefacts.Alps_Idol_gravityPt2, alc.alps_clean_statue_gravity_pt2},
            };
            Dictionary<Artefacts, GameObject> idolsToCompleteGameObjects = new()
            { // no partial loading of idol parts
                {Artefacts.Alps_Idol_crimpsPt1, alc.alps_clean_statue_crimps_complete},
                {Artefacts.Alps_Idol_slopersPt1, alc.alps_clean_statue_slopers_complete},
                {Artefacts.Alps_Idol_feathersPt1, alc.alps_clean_statue_feathers_complete},
                {Artefacts.Alps_Idol_pitchesPt1, alc.alps_clean_statue_pitches_complete},
                {Artefacts.Alps_Idol_icePt1, alc.alps_clean_statue_ice_complete},
                {Artefacts.Alps_Idol_pinchesPt1, alc.alps_clean_statue_pinches_complete},
                {Artefacts.Alps_Idol_greaterbalancePt1, alc.alps_clean_statue_greaterbalance_complete},
                {Artefacts.Alps_Idol_sundownPt1, alc.alps_clean_statue_sundown_complete},
                {Artefacts.Alps_Idol_seedsPt1, alc.alps_clean_statue_seeds_complete},
                {Artefacts.Alps_Idol_gravityPt1, alc.alps_clean_statue_gravity_complete},
            };

            foreach (Artefacts idol in idolsToCompleteGameObjects.Keys)
            {
                bool p1 = slotData.HasArtefact(idol);
                bool p2 = slotData.HasArtefact(idol+1);
                if (p1 && p2)
                {
                    idolsToCompleteGameObjects[idol].SetActive(true);
                    idolsToPartsGameObjects[idol].SetActive(false);
                    idolsToPartsGameObjects[idol+1].SetActive(false);
                }
                else
                {
                    idolsToCompleteGameObjects[idol].SetActive(false);
                    idolsToPartsGameObjects[idol].SetActive(p1);
                    idolsToPartsGameObjects[idol+1].SetActive(p2);
                }
            }

            // Gentiana loading
            List<Artefacts> gentianas = new() { Artefacts.Alps_Gentiana1, Artefacts.Alps_Gentiana2, 
                                                Artefacts.Alps_Gentiana3, Artefacts.Alps_Gentiana4, 
                                                Artefacts.Alps_Gentiana5, Artefacts.Alps_Gentiana6, 
                                                Artefacts.Alps_Gentiana7 };

            alc.alpsflowerpot_gentiana.SetActive(false);

            for (int i = 0; i < gentianas.Count; i++)
            {
                if (slotData.HasArtefact(gentianas[i]))
                {
                    alc.alpsflowerpot_gentiana.SetActive(true);
                    alc.alpsflower_gentiana[i].SetActive(true);
                }
                else
                {
                    alc.alpsflower_gentiana[i].SetActive(false);
                }
            }

            // Edelweiss loading
            List<Artefacts> edelweiss = new() { Artefacts.Alps_Edelweiss1, Artefacts.Alps_Edelweiss2,
                                                Artefacts.Alps_Edelweiss3, Artefacts.Alps_Edelweiss4,
                                                Artefacts.Alps_Edelweiss5, Artefacts.Alps_Edelweiss6,
                                                Artefacts.Alps_Edelweiss7 };

            alc.alpsflowerpot_edelweiss.SetActive(false);

            for (int i = 0; i < edelweiss.Count; i++)
            {
                if (slotData.HasArtefact(edelweiss[i]))
                {
                    alc.alpsflowerpot_edelweiss.SetActive(true);
                    alc.alpsflower_edelweiss[i].SetActive(true);
                }
                else
                {
                    alc.alpsflower_edelweiss[i].SetActive(false);
                }
            }

            AlpsEvents alpsEvents = GameObject.FindObjectOfType<AlpsEvents>();

            alpsEvents.alps_freesolo_medal.SetActive(false);
            alpsEvents.alps_timeattack_medal1.SetActive(false);
            alpsEvents.alps_timeattack_medal2.SetActive(false);
            alpsEvents.alps_timeattack_medal3.SetActive(false);
            alpsEvents.alps_medal1.SetActive(false);
            alpsEvents.alps_medal2.SetActive(false);
            alpsEvents.alps_medal3.SetActive(false);

            // Tool loading
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
    }
}