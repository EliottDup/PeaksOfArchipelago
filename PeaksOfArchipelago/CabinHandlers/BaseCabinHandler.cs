using Archipelago.MultiClient.Net.Models;
using PeaksOfArchipelago.Assets;
using PeaksOfArchipelago.GameData;
using PeaksOfArchipelago.Session;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.UI;

namespace PeaksOfArchipelago.CabinHandlers
{
    internal class BaseCabinHandler(ISlotData slotData, SessionSettings settings) : CabinHandler(slotData, settings)
    {
        bool hasSpawnedInAPLogo = false;
        GameObject book1;

        public override bool CollectItems(List<ItemInfo> itemInfos)
        {
            itemInfos = itemInfos.Where(i => i.ItemName != "Trap").ToList();

            if (itemInfos.Count == 0)
            {
                return true;
            }
            NPCEvents npcEvents = GameObject.FindObjectOfType<NPCEvents>();

            npcEvents.eventName = "AllArtefacts"; // Hijacking the AllArtefacts event for AP parcel delivery
            Text textElem = npcEvents.allArtefactsInfo.GetComponentInChildren<Text>();

            textElem.text = $"You have received:\n {itemInfos[0].ItemName}";
            for (int i = 1; i < itemInfos.Count; i++)
            {
                textElem.text += $", {itemInfos[i].ItemName}";
            }

            if (!hasSpawnedInAPLogo)
            {
                GameObject apLogo = GameObject.Instantiate(PeaksOfAssets.APLogo);
                apLogo.layer = npcEvents.packageContentAllArtefacts.layer;
                apLogo.transform.GetChild(0).gameObject.layer = npcEvents.packageContentAllArtefacts.layer;
                apLogo.transform.SetParent(npcEvents.packageContentAllArtefacts.transform.parent, false);
                apLogo.transform.localPosition = npcEvents.packageContentAllArtefacts.transform.localPosition;
                apLogo.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                apLogo.transform.rotation = npcEvents.packageContentAllArtefacts.transform.rotation;
                npcEvents.packageContentAllArtefacts = apLogo;
                npcEvents.packageContentAllArtefacts.SetActive(false);
                hasSpawnedInAPLogo = true;
            }

            npcEvents.runningEvent = true;
            npcEvents.npcParcelDeliverySystem.StartCoroutine("FadeScreenAndStartUnpackEvent");
            // trust that the patches will correctly reverse any effects brought by this :P
            return true;
        }

        public override bool HandleCompletion()
        {
            // NPCEvents: GameEndingBase
            NPCEvents npcEvents = GameObject.FindObjectOfType<NPCEvents>();
            if (npcEvents == null)
            {
                logger.LogError("Couldn't find NPCEvents to start win check");
                return false;
            }
            npcEvents.StartCoroutine("GameEndingBase");
            return true;
        }

        private struct Artefact
        {
            public Artefact(GameObject cleanObject, GameObject dirtyObject, bool isDirty, bool hasArtefact)
            {
                this.cleanObject = cleanObject;
                this.dirtyObject = dirtyObject;
                this.isDirty = isDirty;
                this.hasArtefact = hasArtefact;
            }

            public GameObject cleanObject;
            public GameObject dirtyObject;
            public bool isDirty;
            public bool hasArtefact;
        }

        public override void LoadProgress()
        {
            logger.LogInfo("Loading Base Cabin Progress...");


            ArtefactLoaderCabin alc = GameObject.FindObjectOfType<ArtefactLoaderCabin>();

            // If not alps ticket, disable ticket & suitcase
            if (!slotData.HasTool(Tools.AlpsTicket))
            {
                // Disable Alps Bed
                // Enable Normal Bed
                logger.LogInfo("Alps disabled, disabling ticket (if existing)");
                GameObject.Find("bed_original")?.SetActive(true);

                GameObject alps_bed = GameObject.Find("AlpsGateway");
                if (alps_bed != null)
                {
                    alps_bed.SetActive(false);
                }
                else
                {
                    logger.LogWarning("Alps Gateway not found!");
                }
            }
            else
            {
                Dictionary<Artefacts, GameObject> idolsToGameObjects = new()
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

                foreach (Artefacts idol in idolsToGameObjects.Keys)
                {
                    idolsToGameObjects[idol].SetActive(slotData.HasArtefact(idol) && slotData.HasArtefact(idol+1));
                }
            }

            // book loading
            if (book1 == null)
            {
                book1 = GameObject.Find("MAPJOURNAL");
                if (book1 == null)
                    logger.LogWarning("Warning: Could Not Find Fundamentals book");
            }

            NPCEvents npcEvents = GameObject.FindObjectOfType<NPCEvents>();

            if (npcEvents == null)
            {
                logger.LogWarning("npc events is null, aborting");
                return;
            }

            Transform ticketObject = npcEvents.cabinIceaxes.transform.Find("Category4_Ticket_Cabin");
            Transform iceAxesObject = npcEvents.cabinIceaxes.transform.Find("iceaxes_hanging");

            npcEvents.iceaxesInfo.SetActive(false);

            book1.SetActive(slotData.ShowBook(Books.Fundamentals));
            npcEvents.cabin_Category2.SetActive(slotData.ShowBook(Books.Intermediate));
            bool adv = slotData.ShowBook(Books.Advanced);
            npcEvents.cabin_Category3.SetActive(adv);
            npcEvents.teaclothteacupObj.SetActive(!adv);
            ticketObject.gameObject.SetActive(slotData.ShowBook(Books.Expert));

            // some general disabling
            npcEvents.cabinMedal_1.SetActive(false);
            npcEvents.cabinMedal_2.SetActive(false);
            npcEvents.cabinMedal_3.SetActive(false);
            npcEvents.cabinallArtefacts.SetActive(false);
            // Tools Showing / loading

            if (npcEvents.cabinRope)
            {
                npcEvents.cabinRope.SetActive(slotData.HasTool(Tools.Rope));
            }
            if (npcEvents.cabinPhonograph)
            {
                npcEvents.cabinPhonograph.SetActive(slotData.HasTool(Tools.Phonograph));
            }
            iceAxesObject.gameObject.SetActive(slotData.HasTool(Tools.IceAxes));
            npcEvents.iceaxesInfo.SetActive(false);
            if (npcEvents.cabinRopesUpgraded)
            {
                npcEvents.cabinRopesUpgraded.SetActive(slotData.HasTool(Tools.RopeLengthUpgrade));
            }
            // I don't know why this is enabled otherwise??

            // load pocketwatch

            if (npcEvents.cabinPocketwatch)
            {
                npcEvents.cabinPocketwatch.SetActive(slotData.HasTool(Tools.Pocketwatch));
                npcEvents.cabinScoreboard.SetActive(slotData.HasTool(Tools.Pocketwatch));
            }

            // Crampon loading...
            if (slotData.cramponLevel > 0)
            {
                npcEvents.cabinCramponsCollider.SetActive(true);
                logger.LogInfo($"crampon level: {slotData.cramponLevel}");
                if (slotData.cramponLevel <= 2)
                {
                    if (PlayerPrefs.GetInt("UseBasicCrampons") == 1)
                    {
                        npcEvents.cramponsEditionTxt.text = "10-";
                        npcEvents.cabinCrampons_10Point.SetActive(true);
                        npcEvents.cabinCrampons_6Point.SetActive(false);
                    }
                    else
                    {
                        npcEvents.cramponsEditionTxt.text = "6-";
                        npcEvents.cabinCrampons_6Point.SetActive(true);
                        npcEvents.cabinCrampons_10Point.SetActive(false);
                    }
                }
                else
                {
                    PlayerPrefs.SetInt("UseBasicCrampons", 1);
                    npcEvents.cramponsEditionTxt.text = "6-";
                    npcEvents.cabinCrampons_6Point.SetActive(true);
                    npcEvents.cabinCrampons_10Point.SetActive(false);
                }

            }
            else
            {
                logger.LogInfo("Showing no crampons");
                npcEvents.cabinCramponsCollider.SetActive(false);
                npcEvents.cabinCrampons_6Point.SetActive(false);
                npcEvents.cabinCrampons_10Point.SetActive(false);
            }

            // load chalk

            if (npcEvents.cabinChalkbag)
            {
                npcEvents.cabinChalkbag.SetActive(slotData.HasTool(Tools.Chalkbag));
            }

            // load pipe
            if (npcEvents.cabinPipe)
            {
                npcEvents.cabinPipe.SetActive(slotData.HasTool(Tools.Pipe));
            }
            if (!slotData.HasTool(Tools.Pipe))
            {
                GameManager.control.isUsingPipe = false;
            }

            // load barometer
            if (npcEvents.cabinBarometer)
            {
                npcEvents.cabinBarometer.SetActive(slotData.HasTool(Tools.Barometer));
            }
        }

        internal override void LoadArtefacts()
        {
            // ISSUE TODO: AAAA IsDirty is not set correctly by the game
            ArtefactLoaderCabin alc = GameObject.FindObjectOfType<ArtefactLoaderCabin>();

            Dictionary<Artefacts, Artefact> artefactsToGameObjects = new()
                {   // Can't really blame Andos for doing it this way, but I hate it :D
                    // It could have been done with a Hashmap or something (he even has a whole enum with all the artefacts)
                    // but noooo, we have 20123 individual fields in GameManager.
                    // INSTEAD OF 3 HASHMAPS
                    {Artefacts.Hat1, new Artefact(alc.clean_hat1, alc.dirty_hat1, GameManager.control.artefact_Hat1_IsDirty, GameManager.control.hasArtefact_Hat1)},
                    {Artefacts.Hat2, new Artefact(alc.clean_hat2, alc.dirty_hat2, GameManager.control.artefact_Hat2_IsDirty, GameManager.control.hasArtefact_Hat2)},
                    {Artefacts.Helmet, new Artefact(alc.clean_helmet, alc.dirty_helmet, GameManager.control.artefact_Helmet_IsDirty, GameManager.control.hasArtefact_Helmet)},
                    {Artefacts.Shoe, new Artefact(alc.clean_shoe, alc.dirty_shoe, GameManager.control.artefact_Shoe_IsDirty, GameManager.control.hasArtefact_Shoe)},
                    {Artefacts.Shovel, new Artefact(alc.clean_shovel, alc.dirty_shovel, GameManager.control.artefact_Shovel_IsDirty, GameManager.control.hasArtefact_Shovel)},
                    {Artefacts.Sleepingbag, new Artefact(alc.clean_sleepingbag, alc.dirty_sleepingbag, GameManager.control.artefact_Sleepingbag_IsDirty, GameManager.control.hasArtefact_Sleepingbag)},
                    {Artefacts.Backpack, new Artefact(alc.clean_backpack, alc.dirty_backpack, GameManager.control.artefact_Backpack_IsDirty, GameManager.control.hasArtefact_Backpack)},
                    {Artefacts.Coffebox_1, new Artefact(alc.clean_coffeebox1, alc.dirty_coffeebox1, GameManager.control.artefact_Coffeebox1_IsDirty, GameManager.control.hasArtefact_Coffeebox1)},
                    {Artefacts.Coffebox_2, new Artefact(alc.clean_coffeebox2, alc.dirty_coffeebox2, GameManager.control.artefact_Coffeebox2_IsDirty, GameManager.control.hasArtefact_Coffeebox2)},
                    {Artefacts.Chalkbox_1, new Artefact(alc.clean_chalkbox1, alc.dirty_chalkbox1, GameManager.control.artefact_Chalkbox1_IsDirty, GameManager.control.hasArtefact_Chalkbox1)},
                    {Artefacts.Chalkbox_2, new Artefact(alc.clean_chalkbox2, alc.dirty_chalkbox2, GameManager.control.artefact_Chalkbox2_IsDirty, GameManager.control.hasArtefact_Chalkbox2)},
                    {Artefacts.ClimberStatue1, new Artefact(alc.clean_statue1, alc.dirty_statue1, GameManager.control.artefact_Statue1_IsDirty, GameManager.control.hasArtefact_Statue1)},
                    {Artefacts.ClimberStatue2, new Artefact(alc.clean_statue2, alc.dirty_statue2, GameManager.control.artefact_Statue2_IsDirty, GameManager.control.hasArtefact_Statue2)},
                    {Artefacts.ClimberStatue3, new Artefact(alc.clean_statue3, alc.dirty_statue3, GameManager.control.artefact_Statue3_IsDirty, GameManager.control.hasArtefact_Statue3)},
                    {Artefacts.Photograph_1, new Artefact(alc.clean_photograph1, alc.dirty_photograph1, GameManager.control.artefact_Photograph1_IsDirty, GameManager.control.hasArtefact_Photograph1)},
                    {Artefacts.Photograph_2, new Artefact(alc.clean_photograph2, alc.dirty_photograph2, GameManager.control.artefact_Photograph2_IsDirty, GameManager.control.hasArtefact_Photograph2)},
                    {Artefacts.Photograph_3, new Artefact(alc.clean_photograph3, alc.dirty_photograph3, GameManager.control.artefact_Photograph3_IsDirty, GameManager.control.hasArtefact_Photograph3)},
                    {Artefacts.Photograph_4, new Artefact(alc.clean_photograph4, alc.dirty_photograph4, GameManager.control.artefact_Photograph4_IsDirty, GameManager.control.hasArtefact_Photograph4)},
                    {Artefacts.PhotographFrame, new Artefact(alc.clean_photographframe, alc.dirty_photographframe, GameManager.control.artefact_PhotographFrame_IsDirty, GameManager.control.hasArtefact_PhotographFrame)},
                    {Artefacts.ClimberStatue0, new Artefact(alc.clean_statue0, alc.dirty_statue0, GameManager.control.artefact_Statue0_IsDirty, GameManager.control.hasArtefact_Statue0)}
                };

            // artefact loading

            foreach (Artefacts a in artefactsToGameObjects.Keys)
            {
                Artefact art = artefactsToGameObjects[a];
                if ((settings.artefactItems && slotData.HasArtefact(a)) || (!settings.artefactItems && art.hasArtefact))
                {
                    if (art.isDirty)
                    {
                        art.cleanObject.SetActive(false);
                        art.dirtyObject.SetActive(true);
                    }
                    else
                    {
                        art.cleanObject.SetActive(true);
                        art.dirtyObject.SetActive(false);
                    }
                }
                else
                {
                    art.cleanObject.SetActive(false);
                    art.dirtyObject.SetActive(false);
                }
            }
        }
    }
}