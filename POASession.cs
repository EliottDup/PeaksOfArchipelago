using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private bool instantRope;
    bool playerKilled = false;
    Dictionary<long, ScoutedItemInfo> scoutedItems;
    public readonly PlayerData playerData = playerData;
    private int itemcount = 0;
    public List<SimpleItemInfo> uncollectedItems = [];
    public string currentScene;
    public GameObject fundamentalsBook;
    private LoginSuccessful loginSuccessful;
    private bool firstLogin = false;
    public bool finished = false;
    public bool seenFinishedCutScene = false;

    public async Task<bool> Connect(string uri, string SlotName, string Password)
    {
        loginSuccessful = null;
        uncollectedItems = [];
        deathLinkService = null;
        // Reset some values to allow reconnection without restarting

        Debug.Log("Connecting to " + uri);
        session = ArchipelagoSessionFactory.CreateSession(uri);
        LoginResult result = session.TryConnectAndLogin("Peaks of Yore", SlotName, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, password: Password);
        if (!result.Successful)
        {
            Debug.LogError("unsuccessful connect, aborting");
            Debug.LogError("Something went wrong and you are not connected");
            foreach (string error in ((LoginFailure)result).Errors)
            {
                Debug.LogError(error);
            }
            return false;
        }

        session.SetClientState(ArchipelagoClientState.ClientConnected);
        loginSuccessful = (LoginSuccessful)result;

        if (loginSuccessful.SlotData.TryGetValue("deathLink", out var enableDeathLink))
        {
            if (Convert.ToInt32(enableDeathLink) == 1)
            {
                deathLinkService = session.CreateDeathLinkService();
                deathLinkService.EnableDeathLink();
                Debug.Log("Enabling Death Link");

                deathLinkService.OnDeathLinkReceived += (deathLinkObject) =>
                {
                    Debug.Log(deathLinkObject.Source + deathLinkObject.Cause);
                    KillPlayer();
                };
            }
        }
        if (loginSuccessful.SlotData.TryGetValue("ropeUnlockMode", out var ropeUnlockMode))
        {
            instantRope = Convert.ToInt32(ropeUnlockMode) == 0;
        }

        await LoadLocationDetails();

        session.Items.ItemReceived += (receivedItemsHelper) =>
        {
            Debug.Log($"Received Item: {receivedItemsHelper.PeekItem().ItemName}"); // This doesn't seem to work for some reason, so I just check for new items when entering the cabin
        };

        firstLogin = true;
        session.DataStorage["ItemCount"].Initialize(0);
        Debug.Log("Login result: " + result.Successful);
        return result.Successful;
    }

    public void UpdateReceivedItems()
    {
        if (session == null) return;
        if (firstLogin)
        {
            firstLogin = false;
            Debug.Log("Getting previously unlocked items");
            itemcount = session.DataStorage["ItemCount"];
            Debug.Log($"found {itemcount} already unlocked items");
            List<SimpleItemInfo> oldReceivedItems = [.. session.Items.AllItemsReceived.Take(itemcount).Select(item =>
            new SimpleItemInfo() { playerName = item.Player.Name, id = item.ItemId, itemName = item.ItemName })];
            foreach (SimpleItemInfo oldReceivedItem in oldReceivedItems)
            {
                UnlockById(oldReceivedItem.id);
            }
        }
        if (session.Items.AllItemsReceived.Count == itemcount) return;
        List<SimpleItemInfo> newReceivedItems = [.. session.Items.AllItemsReceived.Select(item =>
            new SimpleItemInfo() { playerName = item.Player.Name, id = item.ItemId, itemName = item.ItemName })]; // slight affront to god to convert to custom item class

        uncollectedItems = [.. uncollectedItems.Concat(newReceivedItems.Skip(itemcount))];
        Debug.Log($"Received {uncollectedItems.Count} items");
        itemcount = newReceivedItems.Count;
        session.DataStorage["ItemCount"] = itemcount;
    }

    public async Task LoadLocationDetails()
    {
        scoutedItems = await session.Locations.ScoutLocationsAsync(HintCreationPolicy.None, [.. session.Locations.AllLocations]);
    }

    public void HandleDeath()
    {
        if (deathLinkService == null) return;
        if (playerKilled)
        {
            playerKilled = false;   // This is done to prevent players killed by deathlink to send out deathlink ticks again
            return;
        }
        Debug.Log("Sending Deathlink");
        if (session == null) return;
        deathLinkService.SendDeathLink(new DeathLink(session.Players.GetPlayerAliasAndName(session.ConnectionInfo.Slot), "Fell off."));
    }

    public void CheckWin()
    {
        if (session == null) return;
        if (loginSuccessful.SlotData.TryGetValue("goal", out var goal))
        {
            string winCondition = goal.ToString();
            bool checkPeaks = winCondition == "0" || winCondition == "2";       // 0 or 2
            bool checkArtefacts = winCondition == "1" || winCondition == "2";   // 1 or 2
            bool checkAll = winCondition == "3";
            ReadOnlyCollection<long> missing = session.Locations.AllMissingLocations;
            if (checkAll)
            {
                if (missing.Count() == 0)
                {
                    Debug.Log("Win condition achieved, unlocking items!");
                    session.SetClientState(ArchipelagoClientState.ClientGoal);
                    session.SetGoalAchieved();
                    finished = true;
                }
            }
            else
            {
                foreach (long id in missing)
                {
                    Type type = Utils.GetTypeById(id);
                    if ((type == typeof(Peaks) && checkPeaks) || (type == typeof(Artefacts) && checkArtefacts))
                    {
                        return;
                    }
                }
                Debug.Log("Win condition achieved, unlocking items!");
                session.SetClientState(ArchipelagoClientState.ClientGoal);
                session.SetGoalAchieved();
                finished = true;
            }
        }
    }

    public void KillPlayer()
    {
        FallingEvent[] events = GameObject.FindObjectsOfType<FallingEvent>();
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
        // DONE!    // I don't know why I placed this comment here lol
        return peak;
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
        if (instantRope)
        {
            GameManager.control.rope = true;
            playerData.items.rope = true;
        }

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
        GameManager.control.extraBirdSeedUses++;
        playerData.items.seeds.SetCheck(birdSeed, true);
    }

    private void UnlockTool(Tools tool)
    {
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
                    GameManager.control.chalkBag = true;
                    playerData.items.chalkbag = true;
                    break;
                }
            case Tools.Rope:
                {
                    GameManager.control.rope = true;
                    playerData.items.rope = true;
                    break;
                }
            case Tools.Coffee:
                {
                    GameManager.control.coffee = true;
                    playerData.items.coffee = true;
                    break;
                }
            case Tools.Lamp:
                {
                    playerData.items.lamp = true;
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
                if (instantRope)
                {
                    GameManager.control.rope = true;
                    playerData.items.rope = true;
                }

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