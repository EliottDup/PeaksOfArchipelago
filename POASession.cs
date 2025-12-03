using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;
using BepInEx.Logging;
using UnityEngine;
using Color = UnityEngine.Color;

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
    public static ManualLogSource logger;

    public async Task<bool> Connect(string uri, string SlotName, string Password)
    {
        loginSuccessful = null;
        uncollectedItems = [];
        deathLinkService = null;
        // Reset some values to allow reconnection without restarting

        logger.LogInfo("Connecting to " + uri);
        session = ArchipelagoSessionFactory.CreateSession(uri);
        LoginResult result = session.TryConnectAndLogin("Peaks of Yore", SlotName, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, password: Password);
        if (!result.Successful)
        {
            logger.LogError("unsuccessful connect, aborting");
            logger.LogError("Something went wrong and you are not connected");
            UIHandler.instance.Notify("<color=#FF0000>Connection failed, see the console for more details</color>");
            foreach (string error in ((LoginFailure)result).Errors)
            {
                logger.LogError(error);
            }
            foreach (ConnectionRefusedError error in ((LoginFailure)result).ErrorCodes)
            {
                string errorMsg = error switch
                {
                    ConnectionRefusedError.InvalidSlot => "Slot name invalid",
                    ConnectionRefusedError.InvalidGame => "Developer made a big oopsie lol",
                    ConnectionRefusedError.SlotAlreadyTaken => "Player already connected",
                    ConnectionRefusedError.IncompatibleVersion => "Version incompatible",
                    ConnectionRefusedError.InvalidPassword => "Password is incorrect",
                    ConnectionRefusedError.InvalidItemsHandling => "Developer made a big oopsie lol",
                    _ => "Error so weird even the code doesn't know whats going on",
                };
                UIHandler.instance.AddChatMessage($"<color=#FF0000>Error: {errorMsg}</color>");
            }
            return false;
        }

        UIHandler.instance.Notify("<color=#00FF00>Connection Successful!</color>", 0.5f, 4, 0.5f);

        session.SetClientState(ArchipelagoClientState.ClientConnected);
        loginSuccessful = (LoginSuccessful)result;

        bool deathLinkTraps = false;
        if (loginSuccessful.SlotData.TryGetValue("deathLinkTraps", out var tmp))
        {
            deathLinkTraps = Convert.ToInt32(tmp) == 1;
        }


        if (loginSuccessful.SlotData.TryGetValue("deathLink", out var enableDeathLink))
        {
            if (Convert.ToInt32(enableDeathLink) == 1)
            {
                deathLinkService = session.CreateDeathLinkService();
                deathLinkService.EnableDeathLink();
                logger.LogInfo("Enabling Death Link");
                if (deathLinkTraps)
                {
                    deathLinkService.OnDeathLinkReceived += (deathLinkObject) =>
                    {
                        logger.LogInfo(deathLinkObject.Source + deathLinkObject.Cause);
                        Traps.instance?.StartTrap();
                    };
                }
                else
                {
                    deathLinkService.OnDeathLinkReceived += (deathLinkObject) =>
                    {
                        logger.LogInfo(deathLinkObject.Source + deathLinkObject.Cause);
                        KillPlayer();
                    };
                }
            }
        }
        if (loginSuccessful.SlotData.TryGetValue("ropeUnlockMode", out var ropeUnlockMode))
        {
            instantRope = Convert.ToInt32(ropeUnlockMode) == 0;
        }

        if (loginSuccessful.SlotData.TryGetValue("gameMode", out var gameMode))
        {
            logger.LogInfo($"gamemode: {Convert.ToInt32(gameMode)}");
            if (Convert.ToInt32(gameMode) == 0)
            {
                foreach (Peaks peak in Enum.GetValues(typeof(Peaks)))
                {
                    playerData.items.peaks.SetCheck(peak, true);
                }
            }
            else
            {
                foreach (Books book in Enum.GetValues(typeof(Books)))
                {
                    playerData.items.books.SetCheck(book, true);
                }
            }
        }

        await LoadLocationDetails();

        session.MessageLog.OnMessageReceived += (logMessage) =>
        {
            string message = "";
            foreach (MessagePart part in logMessage.Parts)
            {
                Color col = new Color(part.Color.R, part.Color.G, part.Color.B);
                message += $"<color=#{ColorUtility.ToHtmlStringRGB(col)}>" + part.Text + "</color>";
            }
            UIHandler.instance.AddChatMessage(message);
        };

        session.Items.ItemReceived += (receivedItemsHelper) =>
        {
            ItemInfo item = receivedItemsHelper.AllItemsReceived.Last();
            logger.LogInfo($"Receiving Item: {item.ItemName}"); // This doesn't seem to work for some reason, so I just check for new items when entering the cabin
            if (item.ItemId == Utils.ExtraItemToId(ExtraItems.Trap))
            {
                logger.LogInfo("Activating Trap!");
                Traps.instance?.StartTrap();
            }
        };

        firstLogin = true;
        session.DataStorage["ItemCount"].Initialize(0);
        logger.LogInfo("Login result: " + result.Successful);
        return result.Successful;
    }

    public void UpdateReceivedItems()
    {
        if (session == null) return;
        if (firstLogin)
        {
            firstLogin = false;
            logger.LogInfo("Getting previously unlocked items");
            itemcount = session.DataStorage["ItemCount"];
            logger.LogInfo($"found {itemcount} already unlocked items");
            List<SimpleItemInfo> oldReceivedItems = [.. session.Items.AllItemsReceived.Where(item => item.ItemName != "Trap").Take(itemcount).Select(item =>
            new SimpleItemInfo() { playerName = item.Player.Name, id = item.ItemId, itemName = item.ItemName })];
            foreach (SimpleItemInfo oldReceivedItem in oldReceivedItems)
            {
                UnlockById(oldReceivedItem.id);
            }
        }
        if (session.Items.AllItemsReceived.Count == itemcount) return;
        List<SimpleItemInfo> newReceivedItems = [.. session.Items.AllItemsReceived.Select(item =>
            new SimpleItemInfo() { playerName = item.Player.Name, id = item.ItemId, itemName = item.ItemName }).Where( item => item.itemName != "Trap")]; // slight affront to god to convert to custom item class & filter out traps

        uncollectedItems = [.. uncollectedItems.Concat(newReceivedItems.Skip(itemcount))];
        logger.LogInfo($"Received {uncollectedItems.Count} items");
        itemcount = newReceivedItems.Count;
        session.DataStorage["ItemCount"] = itemcount;
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
        logger.LogInfo("Sending Deathlink");
        if (session == null) return;
        deathLinkService?.SendDeathLink(new DeathLink(session.Players.GetPlayerAliasAndName(session.ConnectionInfo.Slot), "Fell off."));
    }

    public void CheckWin()
    {
        if (session == null) return;
        if (loginSuccessful.SlotData.TryGetValue("goal", out var goal))
        {
            string winCondition = goal.ToString();
            bool checkPeaks = winCondition == "0" || winCondition == "2";       // 0 or 2
            bool checkArtefacts = winCondition == "1" || winCondition == "2";   // 1 or 2
            bool checkTimeAttack = winCondition == "3";
            bool checkAll = winCondition == "4";
            ReadOnlyCollection<long> missing = session.Locations.AllMissingLocations;
            if (checkAll)
            {
                if (missing.Count() == 0)
                {
                    logger.LogInfo("Win condition achieved, unlocking items!");
                    UIHandler.instance.Notify("Win condition achieved!");
                    session.SetClientState(ArchipelagoClientState.ClientGoal);
                    session.SetGoalAchieved();
                    finished = true;
                }
            }
            else
            {
                foreach (long id in missing)
                {
                    CollectibleType type = Utils.GetTypeById(id);
                    if ((type == CollectibleType.Peak && checkPeaks) ||
                        (type == CollectibleType.Artefact && checkArtefacts) ||
                        ((type == CollectibleType.TimeParPeak ||
                          type == CollectibleType.RopeParPeak ||
                          type == CollectibleType.HoldParPeak) && checkTimeAttack))
                    {
                        return;
                    }
                }
                UIHandler.instance.Notify("Win condition achieved!");
                logger.LogInfo("Win condition achieved, unlocking items!");
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
            logger.LogInfo("killing player");
            playerKilled = true;
            method.Invoke(fallingEvent, null);
        }
        Climbing[] climbingScripts = GameObject.FindObjectsOfType<Climbing>();
        foreach (Climbing climbing in climbingScripts)
        {
            climbing.ReleaseLHand(true);
            climbing.ReleaseRHand(true);
        }
    }

    public SimpleItemInfo GetLocationDetails(long loc)
    {
        if (scoutedItems == null || !session.Locations.AllLocations.Contains(loc))
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

    public void NotifyCollection(long id, bool hideIfAlreadyUnlocked = true)
    {
        if (!session.Locations.AllLocationsChecked.Contains(id) && session.Locations.AllLocations.Contains(id))
        {
            SimpleItemInfo itemInfo = GetLocationDetails(id);
            UIHandler.instance.Notify("Found " + itemInfo.playerName + "'s " + itemInfo.itemName);
        }
    }

    public Ropes CompleteRopeCheck(Ropes rope)
    {
        playerData.locations.ropes.SetCheck(rope, true);                            // save rope check
        logger.LogInfo("Completing rope " + rope.ToString());

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

    public void CompleteArtefactCheck(Artefacts artefact)
    {
        logger.LogInfo("Completing artefact " + artefact.ToString());
        playerData.locations.artefacts.SetCheck(artefact, true);

        if (session == null) return;
        session.Locations.CompleteLocationChecks(Utils.ArtefactToId(artefact));
    }

    public void CompleteSeedCheck(BirdSeeds seed)
    {
        logger.LogInfo("Completing seed " + seed.ToString());
        playerData.locations.seeds.SetCheck(seed, true);

        if (session == null) return;
        session.Locations.CompleteLocationChecks(Utils.BirdSeedToId(seed));
    }

    public void CompletePeakCheck(Peaks peak)
    {
        if (session == null) return;

        session.Locations.CompleteLocationChecks(Utils.PeakToId(peak));

        playerData.locations.peaks.SetCheck(peak, true);

        logger.LogInfo("Completing peak " + peak.ToString());
        // DONE!    // I don't know why I placed this comment here lol
    }

    public void CompleteFSPeakCheck(Peaks peak)
    {
        if (session == null) return;

        session.Locations.CompleteLocationChecks(Utils.FSPeakToId(peak));

        playerData.locations.fsPeaks.SetCheck(peak, true);

        logger.LogInfo("Completing free solo peak " + peak.ToString());
        // DONE!    // I don't know why I placed this comment here lol
        return;
    }

    internal void CompleteTimePBCheck(Peaks peak)
    {
        if (session == null) return;

        session.Locations.CompleteLocationChecks((long)peak + Utils.timePBPeakOffset);
        playerData.locations.timePBs.SetCheck(peak, true);
    }

    internal void CompleteHoldsPBCheck(Peaks peak)
    {
        if (session == null) return;

        session.Locations.CompleteLocationChecks((long)peak + Utils.holdPBPeakOffset);
        playerData.locations.holdsPBs.SetCheck(peak, true);
    }

    internal void CompleteRopesPBCheck(Peaks peak)
    {
        if (session == null) return;

        session.Locations.CompleteLocationChecks((long)peak + Utils.ropePBPeakOffset);
        playerData.locations.ropesPBs.SetCheck(peak, true);
    }

    internal string UnlockById(long id)
    {
        if (id < Utils.ropeOffset)
        {
            Peaks peak = Utils.IdtoPeak(id);
            UnlockPeak(peak);
            return peak.ToString();
        }
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

    private void UnlockPeak(Peaks peak)
    {
        playerData.items.peaks.SetCheck(peak, true);
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
            case Tools.RightHand:
                {
                    playerData.items.rightHand = true;
                    break;
                }
            case Tools.leftHand:
                {
                    playerData.items.leftHand = true;
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