using Archipelago.MultiClient.Net.Models;
using PeaksOfArchipelago.Assets;
using PeaksOfArchipelago.GameData;
using PeaksOfArchipelago.Session;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.UI;

namespace PeaksOfArchipelago.CabinHandlers
{
    internal class BaseCabinHandler(ISlotData slotData) : CabinHandler(slotData)
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

            List<ArtefactInfo> artefacts = new List<ArtefactInfo>() {
                new ArtefactInfo(Artefacts.Hat1, alc.clean_hat1, alc.dirty_hat1, GameManager.control.artefact_Hat1_IsDirty, GameManager.control.hasArtefact_Hat1),
                new ArtefactInfo(Artefacts.Hat2, alc.clean_hat2, alc.dirty_hat2, GameManager.control.artefact_Hat2_IsDirty, GameManager.control.hasArtefact_Hat2),
                new ArtefactInfo(Artefacts.Helmet, alc.clean_helmet, alc.dirty_helmet, GameManager.control.artefact_Helmet_IsDirty, GameManager.control.hasArtefact_Helmet),
                new ArtefactInfo(Artefacts.Shoe, alc.clean_shoe, alc.dirty_shoe, GameManager.control.artefact_Shoe_IsDirty, GameManager.control.hasArtefact_Shoe),
                new ArtefactInfo(Artefacts.Shovel, alc.clean_shovel, alc.dirty_shovel, GameManager.control.artefact_Shovel_IsDirty, GameManager.control.hasArtefact_Shovel),
                new ArtefactInfo(Artefacts.Sleepingbag, alc.clean_sleepingbag, alc.dirty_sleepingbag, GameManager.control.artefact_Sleepingbag_IsDirty, GameManager.control.hasArtefact_Sleepingbag),
                new ArtefactInfo(Artefacts.Backpack, alc.clean_backpack, alc.dirty_backpack, GameManager.control.artefact_Backpack_IsDirty, GameManager.control.hasArtefact_Backpack),
                new ArtefactInfo(Artefacts.Coffebox_1, alc.clean_coffeebox1, alc.dirty_coffeebox1, GameManager.control.artefact_Coffeebox1_IsDirty, GameManager.control.hasArtefact_Coffeebox1),
                new ArtefactInfo(Artefacts.Coffebox_2, alc.clean_coffeebox2, alc.dirty_coffeebox2, GameManager.control.artefact_Coffeebox2_IsDirty, GameManager.control.hasArtefact_Coffeebox2),
                new ArtefactInfo(Artefacts.Chalkbox_1, alc.clean_chalkbox1, alc.dirty_chalkbox1, GameManager.control.artefact_Chalkbox1_IsDirty, GameManager.control.hasArtefact_Chalkbox1),
                new ArtefactInfo(Artefacts.Chalkbox_2, alc.clean_chalkbox2, alc.dirty_chalkbox2, GameManager.control.artefact_Chalkbox2_IsDirty, GameManager.control.hasArtefact_Chalkbox2),
                new ArtefactInfo(Artefacts.ClimberStatue1, alc.clean_statue1, alc.dirty_statue1, GameManager.control.artefact_Statue1_IsDirty, GameManager.control.hasArtefact_Statue1),
                new ArtefactInfo(Artefacts.ClimberStatue2, alc.clean_statue2, alc.dirty_statue2, GameManager.control.artefact_Statue2_IsDirty, GameManager.control.hasArtefact_Statue2),
                new ArtefactInfo(Artefacts.ClimberStatue3, alc.clean_statue3, alc.dirty_statue3, GameManager.control.artefact_Statue3_IsDirty, GameManager.control.hasArtefact_Statue3),
                new ArtefactInfo(Artefacts.Photograph_1, alc.clean_photograph1, alc.dirty_photograph1, GameManager.control.artefact_Photograph1_IsDirty, GameManager.control.hasArtefact_Photograph1),
                new ArtefactInfo(Artefacts.Photograph_2, alc.clean_photograph2, alc.dirty_photograph2, GameManager.control.artefact_Photograph2_IsDirty, GameManager.control.hasArtefact_Photograph2),
                new ArtefactInfo(Artefacts.Photograph_3, alc.clean_photograph3, alc.dirty_photograph3, GameManager.control.artefact_Photograph3_IsDirty, GameManager.control.hasArtefact_Photograph3),
                new ArtefactInfo(Artefacts.Photograph_4, alc.clean_photograph4, alc.dirty_photograph4, GameManager.control.artefact_Photograph4_IsDirty, GameManager.control.hasArtefact_Photograph4),
                new ArtefactInfo(Artefacts.PhotographFrame, alc.clean_photographframe, alc.dirty_photographframe, GameManager.control.artefact_PhotographFrame_IsDirty, GameManager.control.hasArtefact_PhotographFrame),
                new ArtefactInfo(Artefacts.ClimberStatue0, alc.clean_statue0, alc.dirty_statue0, GameManager.control.artefact_Statue0_IsDirty, GameManager.control.hasArtefact_Statue0)
            };

            // artefact loading
            // TODO: idols

            foreach (ArtefactInfo art in artefacts)
            {
                if (art.hasArtefact)
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

            Dictionary<Idols, GameObject> idolsToGameObjects = new()
                { // no partial loading of idol parts in normal cabin
                    {Idols.Crimps, alc.alps_clean_statue_crimps_complete},
                    {Idols.Slopers, alc.alps_clean_statue_slopers_complete},
                    {Idols.Feathers, alc.alps_clean_statue_feathers_complete},
                    {Idols.Pitches, alc.alps_clean_statue_pitches_complete},
                    {Idols.Ice, alc.alps_clean_statue_ice_complete},
                    {Idols.Pinches, alc.alps_clean_statue_pinches_complete},
                    {Idols.GreaterBalance, alc.alps_clean_statue_greaterbalance_complete},
                    {Idols.Sundown, alc.alps_clean_statue_sundown_complete},
                    {Idols.Seeds, alc.alps_clean_statue_seeds_complete},
                    {Idols.Gravity, alc.alps_clean_statue_gravity_complete},
                };

            foreach (Idols idol in idolsToGameObjects.Keys)
            {
                idolsToGameObjects[idol].SetActive(slotData.HasIdol(idol));
            }
        }
    }
}