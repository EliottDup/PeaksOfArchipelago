using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using UnityEngine;

namespace PeaksOfArchipelago;

class POASession(PlayerData playerData)
{
    ArchipelagoSession session;
    DeathLinkService deathLinkService;
    bool playerKilled = false;
    Dictionary<long, ScoutedItemInfo> scoutedItems;
    public readonly PlayerData playerData = playerData;

    private int itemcount = 0;
    public List<SimpleItemInfo> uncollectedItems = [];
    public string currentScene;
    public GameObject fundamentalsBook;

    public async Task<bool> Connect(string uri, string SlotName, string Password)
    {
        Debug.Log("Connecting to " + uri);
        session = ArchipelagoSessionFactory.CreateSession(uri);
        Debug.Log("Created session!");
        LoginResult result = session.TryConnectAndLogin("Peaks of Yore", SlotName, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, password: Password);
        Debug.Log("Login result: " + result.Successful);

        if (!result.Successful)
        {
            Debug.Log("unsuccessful connect, aborting");
            return false;
        }
        session.Items.ItemReceived += (helper) =>
        {
            Debug.Log("Recieved item");
        };

        session.SetClientState(ArchipelagoClientState.ClientConnected);

        // session.Items.ItemReceived += (helper) =>
        // {
        //     ItemInfo info = helper.PeekItem();
        //     Debug.Log("Recieved item " + info.ItemName);
        //     recievedItems.AddItem(new SimpleItemInfo
        //     {
        //         playerName = info.Player.Name,
        //         itemName = info.ItemName,
        //         id = info.ItemId
        //     });
        //     helper.DequeueItem();
        // };

        deathLinkService = session.CreateDeathLinkService();
        deathLinkService.EnableDeathLink();


        deathLinkService.OnDeathLinkReceived += (deathLinkObject) =>
        {
            Debug.Log(deathLinkObject.Source + deathLinkObject.Cause);
            KillPlayer();
        };
        await LoadLocationDetails();

        UpdateRecievedItems();
        // TODO: implement loading of data: rope count etc
        //! WARNING: THIS SHOULD BE DONE WHEN ENTERING THE CABIN SCENE, DOING SO EARLIER THAN THAT *WILL* FUCK UP SAVES
        return result.Successful;
    }

    public void UpdateRecievedItems()
    {
        if (session == null) return;
        if (session.Items.AllItemsReceived.Count == itemcount) return;
        List<SimpleItemInfo> newRecievedItems = [.. session.Items.AllItemsReceived.Select(item => new SimpleItemInfo() { playerName = item.Player.Name, id = item.ItemId, itemName = item.ItemName })];
        Debug.Log($"total item count {newRecievedItems.Count}");
        Debug.Log($"old item count {itemcount}");
        uncollectedItems = [.. uncollectedItems.Concat(newRecievedItems.Skip(itemcount))];
        Debug.Log($"Recieved {uncollectedItems.Count} items " + uncollectedItems.ToString());
        itemcount = newRecievedItems.Count;

    }

    public async Task LoadLocationDetails()
    {
        Debug.Log("scouting items");
        scoutedItems = await session.Locations.ScoutLocationsAsync(HintCreationPolicy.None, [.. session.Locations.AllLocations]);
        Debug.Log("scouted items!");
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

    public Ropes CompleteRopeCheck(Ropes rope)
    {
        playerData.locations.ropes.SetCheck(rope, true);                            // save rope check
        Debug.Log("Completing rope " + rope.ToString());

        if (session == null) return (Ropes)(-1);
        session.Locations.CompleteLocationChecks(Utils.RopeToId(rope));  // send check complete to multiworld
        return rope;
    }

    public Ropes CompleteRopeCheck(RopeCollectable ropeCollectable)
    {
        Ropes rope = Utils.GetRopeFromCollectable(ropeCollectable);

        CompleteRopeCheck(rope);    // evil fake recursion
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

    public BirdSeeds CompleteSeedCheck(BirdSeedCollectable seedCollectable)
    {
        BirdSeeds seed = Utils.GetSeedFromCollectable(seedCollectable);
        Debug.Log("Completing seed " + seed.ToString());
        playerData.locations.seeds.SetCheck(seed, true);

        if (session == null) return (BirdSeeds)(-1);
        session.Locations.CompleteLocationChecks(Utils.BirdSeedToId(seed));

        return seed;
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
        if (id < Utils.birdSeedOffset)
        {
            Books book = Utils.IdToBook(id);
            UnlockBook(book);
            return book.ToString();
        }
        if (id < Utils.toolOffset)
        {
            BirdSeeds birdSeed = Utils.IdToBirdSeed(id);
            UnlockBirdSeed(birdSeed);
        }
        if (id < Utils.extraItemOffset)
        {
            Tools tool = Utils.IdToTool(id);
            UnlockTool(tool);
        }
        ExtraItems extraItem = Utils.IdToExtraItem(id);
        UnlockExtraItem(extraItem);
        return extraItem.ToString();
    }

    private void UnlockRope(Ropes rope)
    {
        playerData.items.ropes.SetCheck(rope, true);
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
        switch (artefact)
        {
            case Artefacts.Coffebox_1:
            case Artefacts.Coffebox_2:
                {
                    GameManager.control.coffee = true;
                    GameManager.control.extraCoffeeUses += 2;
                    break;
                }
            case Artefacts.Chalkbox_1:
            case Artefacts.Chalkbox_2:
                {
                    GameManager.control.extraChalkUses += 2;
                    break;
                }

        }


        // GameObject.FindObjectOfType<ArtefactLoaderCabin>()?.LoadArtefacts();
        // GameManager.control.Save();
    }

    private void UnlockBook(Books book)
    {
        playerData.items.books.SetCheck(book, true);
        NPCEvents npcEvents = GameObject.FindObjectOfType<NPCEvents>();
        switch (book)
        {
            case Books.Fundamentals:
                SetFundamentalsBookActive(true);

                break;
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

    public void SetFundamentalsBookActive(bool enabled)
    {
        fundamentalsBook.SetActive(enabled);
    }

    private void UnlockBirdSeed(BirdSeeds birdSeed)
    {
        switch (birdSeed)
        {
            case BirdSeeds.ExtraSeed1:
                {
                    GameManager.control.hasExtraSeed1 = true;
                    break;
                }
            case BirdSeeds.ExtraSeed2:
                {
                    GameManager.control.hasExtraSeed2 = true;
                    break;
                }
            case BirdSeeds.ExtraSeed3:
                {
                    GameManager.control.hasExtraSeed3 = true;
                    break;
                }
            case BirdSeeds.ExtraSeed4:
                {
                    GameManager.control.hasExtraSeed4 = true;
                    break;
                }
            case BirdSeeds.ExtraSeed5:
                {
                    GameManager.control.hasExtraSeed5 = true;
                    break;
                }
        }
        playerData.items.seeds.SetCheck(birdSeed, true);
    }

    private void UnlockTool(Tools tool)
    {
        Debug.Log("Unlocking Tool:" + tool.ToString());
        switch (tool)
        {
            case Tools.Pipe:
                {
                    GameManager.control.smokingpipe = true;
                    playerData.items.pipe = true;
                    break;
                }
            case Tools.RopeLengthUpgrade:
                {
                    GameManager.control.ropesUpgrade = true;
                    playerData.items.ropeLengthUpgrade = true;
                    break;
                }
            case Tools.Barometer:
                {
                    GameManager.control.barometer = true;
                    GameManager.control.artefactMap = true;
                    playerData.items.barometer = true;
                    break;
                }
            case Tools.ProgressiveCrampons:
                {
                    if (playerData.items.progressiveCrampons == 0)
                    {
                        playerData.items.progressiveCrampons++;
                        GameManager.control.crampons = true;
                    }
                    else
                    {
                        playerData.items.progressiveCrampons++;
                        GameManager.control.cramponsUpgrade = true;
                    }
                    break;
                }
            case Tools.Monocular:
                {
                    GameManager.control.monocular = true;
                    playerData.items.monocular = true;
                    break;
                }
            case Tools.Phonograph:
                {
                    GameManager.control.phonograph = true;
                    playerData.items.phonograph = true;
                    break;
                }
            case Tools.Pocketwatch:
                {
                    GameManager.control.pocketwatch = true;
                    playerData.items.pocketwatch = true;
                    break;
                }
            case Tools.Chalkbag:
                {
                    GameManager.control.extraChalkUses++;
                    playerData.items.chalkbag = true;
                    break;
                }
            case Tools.Rope:
                {
                    GameManager.control.rope = true;
                    playerData.items.rope = true;
                    break;
                }
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
                GameManager.control.coffee = true;
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