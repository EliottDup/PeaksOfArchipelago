using Archipelago.MultiClient.Net.Models;
using PeaksOfArchipelago.GameData;
using UnityEngine;

namespace PeaksOfArchipelago.CabinHandlers
{
    internal class ExpertCabinHandler(ISlotData slotData) : CabinHandler(slotData)
    {
        public override bool CollectItems(List<ItemInfo> itemInfos)
        {
            PeaksOfArchipelago.ui.SendNotification($"You have {itemInfos.Count} items awaiting you in your cabin, return there to collect them");
            return false;
        }

        public override bool HandleCompletion()
        {
            PeaksOfArchipelago.ui.SendNotification("You have completed your goal, return to your cabin to free your items.");
            return false;
        }

        public override void LoadProgress()
        {
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

            logger.LogInfo("Loading progress for Expert Cabin");

            if (!slotData.HasTool(Tools.IceAxes))
            {
                PeaksOfArchipelago.ui.SendNotification("You have not unlocked the Ice Axes, you will not be able to progress here");
            }

            //bulwark disabling
            Category4CabinLeave cabinLeave = GameObject.FindObjectOfType<Category4CabinLeave>();
            if (cabinLeave == null)
            {
                logger.LogError("Could not find Category4CabinLeave in Alps Cabin :\\");
                return;
            }
            cabinLeave.enabled = slotData.HasPeak(Peaks.GreatBulwark);
        }

        internal override void LoadArtefacts()
        {
            ArtefactLoaderCabin alc = GameObject.FindObjectOfType<ArtefactLoaderCabin>();

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