using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using HarmonyLib;
using UnityEngine;

namespace PeaksOfArchipelago;

class POASession(PlayerData playerData)
{
    ArchipelagoSession session;
    DeathLinkService deathLinkService;
    bool playerKilled = false;
    Dictionary<long, ScoutedItemInfo> scoutedItems;
    readonly PlayerData playerData = playerData;

    public long[] recievedItems = [];
    public string currentScene;

    public bool Connect(string uri, string SlotName, string Password)
    {
        session = ArchipelagoSessionFactory.CreateSession(uri);
        LoginResult result = session.TryConnectAndLogin("Peaks Of Yore", SlotName, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, password: Password);
        if (session.DataStorage["version"] != ModInfo.MOD_VERSION)
        {
            Debug.Log("Version Incompatible");
            return false;
        }

        session.Items.ItemReceived += (helper) =>
        {
            recievedItems.AddItem(helper.PeekItem().ItemId);
        };

        deathLinkService = session.CreateDeathLinkService();
        deathLinkService.EnableDeathLink();


        deathLinkService.OnDeathLinkReceived += (deathLinkObject) =>
        {
            Debug.Log(deathLinkObject.Source + deathLinkObject.Cause);
            KillPlayer();
        };

        // TODO: implement loading of data: rope count etc
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

        Ropes rope = Utils.GetRopeFromCollectable(ropeCollectable);

        playerData.locations.ropes.SetCheck(rope, true);                            // save rope check
        Debug.Log("Completing rope " + rope.ToString());

        if (session == null) return (Ropes)(-1);
        session.Locations.CompleteLocationChecks(Utils.RopeToId(rope));  // send check complete to multiworld
        return rope;
    }

    public Artefacts CompleteArtefactCheck(ArtefactOnPeak artefactOnPeak)
    {
        Artefacts artefact = Utils.GetArtefactFromCollectable(artefactOnPeak);
        Debug.Log("Completing artefact " + artefact.ToString());
        playerData.locations.artefacts.SetCheck(artefact, true);

        if (session == null) return (Artefacts)(-1);
        session.Locations.CompleteLocationChecks(Utils.ArtefactToId(artefact));

        return artefact;
    }

    public Peaks CompletePeakCheck(StamperPeakSummit peakStamper)
    {
        if (session == null) return (Peaks)(-1);

        Peaks peak = Utils.GetPeakFromCollectable(peakStamper);
        session.Locations.CompleteLocationChecks(Utils.PeakToId(peak));

        playerData.locations.peaks.SetCheck(peak, true);

        Debug.Log("Completing peak " + peak.ToString());
        // DONE!
        return peak;
    }

    internal void LoadArtefacts(ArtefactLoaderCabin instance)
    {
        CheckList<Artefacts> savestate = new();
        foreach (Artefacts artefact in Enum.GetValues(typeof(Artefacts)))
        {
            savestate.SetCheck(artefact, UnityUtils.GetGameManagerArtefactCollected(artefact));
            UnityUtils.SetGameManagerArtefactCollected(artefact, playerData.items.artefacts.IsChecked(artefact));
            UnityUtils.SetGameManagerArtefactDirty(artefact, false);
        }
        Debug.Log("loading Artefacts");
        instance.LoadArtefacts();
        Debug.Log("loaded Artefacts");

        foreach (Artefacts artefact in Enum.GetValues(typeof(Artefacts)))
        {
            UnityUtils.SetGameManagerArtefactCollected(artefact, savestate.IsChecked(artefact));    // reset gamemanager to default state
        }
    }

    internal string UnlockById(long id)
    {
        if (id < Utils.artefactOffset)
        {
            Ropes rope = Utils.IdtoRope(id);
            UnlockRope(rope);
            return rope.ToString();
        }
        if (id < Utils.bookOffset)
        {
            Artefacts artefact = Utils.IdtoArtefact(id);
            UnlockArtefact(artefact);
            return artefact.ToString();
        }
        if (id < Utils.extraItemOffset)
        {
            Books book = Utils.IdToBook(id);
            UnlockBook(book);
            return book.ToString();
        }
        ExtraItems extraItem = Utils.IdToExtraItem(id);
        UnlockExtraItem(extraItem);
        return extraItem.ToString();
    }

    private void UnlockRope(Ropes rope)
    {
        playerData.items.ropes.SetCheck(rope, true);
        GameManager.control.rope = true;
        if (rope < Ropes.ExtraFirst)
        {
            GameManager.control.ropesCollected++;
        }
        else
        {
            GameManager.control.ropesCollected += 2;
        }
        GameManager.control.Save();
    }

    private void UnlockArtefact(Artefacts artefact)
    {
        playerData.items.artefacts.SetCheck(artefact, true);
        UnityUtils.SetGameManagerArtefactCollected(artefact, true);
        // UnityUtils.SetGameManagerArtefactDirty(artefact, true);
        GameObject.FindObjectOfType<ArtefactLoaderCabin>()?.LoadArtefacts();
        GameManager.control.Save();
    }

    private void UnlockBook(Books book)
    {
        playerData.items.books.SetCheck(book, true);
        NPCEvents npcEvents = GameObject.FindObjectOfType<NPCEvents>();
        switch (book)
        {
            case Books.Intermediate:
                GameManager.control.category_2_unlocked = true;
                npcEvents.cabin_Category2.SetActive(true);
                break;
            case Books.Advanced:
                GameManager.control.category_3_unlocked = true;
                npcEvents.cabin_Category3.SetActive(true);
                break;
            case Books.Expert:
                GameManager.control.category_4_unlocked = true;
                npcEvents.cabinIceaxes.SetActive(true);
                GameManager.control.iceAxes = true;
                npcEvents.category4Ticket.StartCoroutine("GlowTicket");
                break;
        }
        GameManager.control.Save();
    }

    private void UnlockExtraItem(ExtraItems extraItem)
    {
        switch (extraItem)
        {
            case ExtraItems.ExtraRope:
                playerData.items.extraropeItemCount++;
                GameManager.control.ropesCollected++;
                break;
            case ExtraItems.ExtraCoffee:
                playerData.items.extraCoffeeItemCount++;
                GameManager.control.extraCoffeeUses++;
                break;
            case ExtraItems.ExtraChalk:
                playerData.items.extraChalkItemCount++;
                GameManager.control.extraChalkUses++;
                break;
            case ExtraItems.ExtraSeed:
                playerData.items.extraSeedItemCount++;
                GameManager.control.extraBirdSeedUses++;
                break;
        }
        GameManager.control.Save();
    }
}