using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using UnityEngine;
using UnityEngine.UIElements;

namespace PeaksOfArchipelago;

class POASession
{
    ArchipelagoSession session;
    DeathLinkService deathLinkService;
    bool playerKilled = false;
    Dictionary<long, ScoutedItemInfo> scoutedItems;
    PlayerData playerData;

    public POASession(PlayerData playerData)
    {
        this.playerData = playerData;
    }

    public bool Connect(string uri, string SlotName, string Password)
    {
        session = ArchipelagoSessionFactory.CreateSession(uri);
        LoginResult result = session.TryConnectAndLogin("Peaks Of Yore", SlotName, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, password: Password);
        if (session.DataStorage["version"] != ModInfo.MOD_VERSION)
        {
            Debug.Log("Version Incompatible");
            return false;
        }

        session.Items.ItemReceived += (item) =>
        {
            // ITEM LOGIC
        };

        deathLinkService = session.CreateDeathLinkService();
        deathLinkService.EnableDeathLink();


        deathLinkService.OnDeathLinkReceived += (deathLinkObject) =>
        {
            Debug.Log(deathLinkObject.Source + deathLinkObject.Cause);
            KillPlayer();
        };

        //TODO: implement loading of data: rope count etc
        //! WARNING: THIS SHOULD BE DONE WHEN ENTERING THE CABIN SCENE, DOING SO EARLIER THAN THAT *WILL* FUCK UP SAVES
        return result.Successful;
    }

    public async Task LoadLocationDetails()
    {
        scoutedItems = await session.Locations.ScoutLocationsAsync(HintCreationPolicy.None, [.. session.Locations.AllLocations]);
    }

    public void HandleDeath()
    {
        if (playerKilled)
        {
            playerKilled = false;   // This is done to prevent players killed by deathlink to send out deathlink ticks again
            return;
        }
        Debug.Log("Sending Deathlink");
        if (session == null) return;
        deathLinkService.SendDeathLink(new DeathLink(session.Players.GetPlayerAliasAndName(session.ConnectionInfo.Slot), "Fell off."));
    }

    public void KillPlayer()
    {
        FallingEvent[] events = GameObject.FindObjectsOfType<FallingEvent>();
        Debug.Log("found " + events.Length + " fallingEvents");
        foreach (FallingEvent fallingEvent in events)
        {
            MethodInfo method = typeof(FallingEvent).GetMethod("FellToDeath", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                return;
            }
            Debug.Log("killing player");
            playerKilled = true;
            method.Invoke(fallingEvent, null);
        }
    }

    public SimpleItemInfo GetLocationDetails(long loc)
    {
        if (scoutedItems == null)
        {
            return new SimpleItemInfo
            {
                playerName = "NoPlayer",
                itemName = "NoItem"
            };
        }
        return new SimpleItemInfo
        {
            playerName = scoutedItems[loc].Player.Name,
            itemName = scoutedItems[loc].ItemDisplayName
        };
    }

    public Ropes CompleteRopeCheck(RopeCollectable ropeCollectable)
    {
        if (session == null) return (Ropes)(-1);

        Ropes rope = Utils.GetRopeFromCollectable(ropeCollectable);
        session.Locations.CompleteLocationChecks(Utils.GetLocationFromRope(rope));  // send check complete to multiworld

        playerData.locations.ropes.SetCheck(rope, true);                            // save rope check

        Debug.Log("Completing rope " + rope.ToString());    // TODO: BLOCK UNLOCKING OF ROPE IN-GAME
        Debug.Log("TODO: Block rope pick up in-game");
        return rope;
    }

    public Artefacts CompleteArtefactCheck(ArtefactOnPeak artefactOnPeak)
    {
        if (session == null) return (Artefacts)(-1);

        Artefacts artefact = Utils.GetArtefactFromCollectable(artefactOnPeak);
        session.Locations.CompleteLocationChecks(Utils.GetLocationFromArtefact(artefact));

        playerData.locations.artefacts.SetCheck(artefact, true);

        Debug.Log("Completing artefact " + artefact.ToString());    // TODO: BLOCK UNLOCKING OF ARTEFACT IN-GAME
        Debug.Log("TODO: Block artefact pick up in-game");
        return artefact;
    }

    public Peaks CompletePeakCheck(StamperPeakSummit peakStamper)
    {
        if (session == null) return (Peaks)(-1);

        Peaks peak = Utils.GetPeakFromCollectable(peakStamper);
        session.Locations.CompleteLocationChecks(Utils.GetLocationFromPeak(peak));

        playerData.locations.peaks.SetCheck(peak, true);

        Debug.Log("Completing peak " + peak.ToString());
        // DONE!
        return peak;
    }
}