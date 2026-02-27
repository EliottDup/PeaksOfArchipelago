using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;
using BepInEx.Logging;
using PeaksOfArchipelago.CabinHandlers;
using PeaksOfArchipelago.GameData;
using PeaksOfArchipelago.UI;
using UnityEngine;
using static PeaksOfArchipelago.GameData.LocationIDs;
using Color = UnityEngine.Color;

namespace PeaksOfArchipelago.Session
{
    internal class Connection
    {
        public static Connection Instance { get; private set; }

        ArchipelagoSession session;
        readonly ManualLogSource logger;
        private Dictionary<string, object> slotOptions;
        public ISlotData slotData;
        private List<ItemInfo> uncollectedItems;
        private List<ItemInfo> instantCollectItems;
        internal SessionSettings settings;

        private Dictionary<long, ScoutedItemInfo> scoutedItems;

        private int itemCount = 0;


        public Connection()
        {
            logger = PeaksOfArchipelago.Logger;
            PeaksOfArchipelago.Instance.OnEnterCabin += OnEnterCabin;
            Instance = this;
        }

        public async Task<bool> ConnectAndLogin(string username, string uri, string password)
        {

            session = ArchipelagoSessionFactory.CreateSession(uri);

            session.Socket.ErrorReceived += (Exception e, string message) =>
            {
                logger.LogInfo("Socket error incoming lmao");
                logger.LogError(e);
            };

            logger.LogInfo($"connecting to {session.Socket.Uri}");

            Task connectTask = session.ConnectAsync();

            if (await Task.WhenAny(connectTask, Task.Delay(4000)) != connectTask)
            {
                logger.LogError("Connection timed out");
                logger.LogInfo("Pushing to chat");
                PeaksOfArchipelago.ui.SendChatMessage("<color=red>Connection timed out.</color>");
                return false;
            }

            logger.LogInfo($"Connected to {session.Socket.Uri}");

            logger.LogInfo($"Logging in as {username}");
            LoginResult result = await session.LoginAsync("Peaks of Yore", username, ItemsHandlingFlags.AllItems, password: password);
            if (!result.Successful)
            {
                logger.LogInfo($"Couldn't log in");
                //TODO: add UI notifications
                foreach (string error in ((LoginFailure)result).Errors)
                {
                    logger.LogError(error);
                }
                foreach (ConnectionRefusedError error in ((LoginFailure)result).ErrorCodes)
                {
                    // TODO: show error to user
                    logger.LogError(error);
                }
                return false;
            }

            PeaksOfArchipelago.Instance.SaveUserCredentials(username, uri, password);

            logger.LogInfo($"Logged in as {username}");
            session.SetClientState(ArchipelagoClientState.ClientConnected);
            slotOptions = ((LoginSuccessful)result).SlotData;
            session.DataStorage["ItemCount"].Initialize(0);

            LoadData();

            // event listeners
            session.Items.ItemReceived += (ReceivedItemsHelper h) => OnItemReceived(h.AllItemsReceived.Last());

            session.MessageLog.OnMessageReceived += (logMessage) =>
            {
                string msg = "";
                foreach (MessagePart part in logMessage.Parts)
                {
                    Color col = new Color(part.Color.R, part.Color.G, part.Color.B);
                    msg += $"<color=#{ColorUtility.ToHtmlStringRGB(col)}>{part.Text}</color>";
                }
                PeaksOfArchipelago.ui.SendChatMessage(msg);
            };

            _ = ScoutLocations();

            return true;
        }

        public async Task ScoutLocations()
        {
            logger.LogInfo("Scouting items");
            scoutedItems = await session.Locations.ScoutLocationsAsync(HintCreationPolicy.None, [.. session.Locations.AllLocations]);
            logger.LogInfo("Items scouted");
        }

        private void LoadData()
        {
            // init this.slotData
            // This does not actually apply the loaded data to the GameManager (because that would break save 1 for some reason I think)
            settings = new SessionSettings(slotOptions);
            this.slotData = new SlotData(settings);

            itemCount = (int)session.DataStorage["ItemCount"];
            instantCollectItems = [.. session.Items.AllItemsReceived.Where(item => item.ItemName != "Trap").Take(itemCount)];
            uncollectedItems = [.. session.Items.AllItemsReceived.Where(item => item.ItemName != "Trap").Skip(itemCount)];

            logger.LogInfo($"Loaded {instantCollectItems.Count} old items and {uncollectedItems.Count} new items");
        }

        public void OnEnterCabin(object sender, Cabins cabin)
        {
            if (instantCollectItems.Count > 0)
            {
                logger.LogInfo("Collecting previously unlocked items...");
                GameManager.control.ropesCollected = 0;
                GameManager.control.extraChalkUses = 0;
                GameManager.control.extraCoffeeUses = 0;
                GameManager.control.extraBirdSeedUses = 0;
                foreach (ItemInfo item in instantCollectItems)
                {
                    logger.LogInfo($"Unlocking Item: {item.ItemName}");
                    UnlockItem(item);
                }

                if (instantCollectItems.Count != session.DataStorage["ItemCount"])
                {
                    int count = session.DataStorage["ItemCount"];
                    logger.LogInfo($"Warning: Item count mismatch! Expected {count}, " +
                        $"got {instantCollectItems.Count}");
                }
                //itemCount += instantCollectItems.Count;
                session.DataStorage["ItemCount"] = itemCount;
                instantCollectItems.Clear();
            }
            // TODO: Collect Items
            CabinHandler handler = CabinHandler.New(cabin, slotData);
            if (uncollectedItems.Count > 0)
            {
                logger.LogInfo("Collecting new items...");
                if (handler.CollectItems(uncollectedItems)) {
                    foreach (ItemInfo item in uncollectedItems)
                    {
                        UnlockItem(item);
                    }
                    itemCount += uncollectedItems.Count;
                    session.DataStorage["ItemCount"] = itemCount;
                    handler.LoadProgress();
                    uncollectedItems.Clear();
                }
            }
            GameManager.control.Save();
            handler.LoadProgress();
        }

        private void OnItemReceived(ItemInfo item)
        {
            uncollectedItems.Add(item);
            logger.LogInfo($"Recieving Item: {item.ItemName}");
            // TODO: Notify player
        }

        private void UnlockItem(ItemInfo item)
        {
            ArchipelagoItem APItem = ArchipelagoItem.Create(item);
            APItem.Unlock(slotData);
        }

        public void CompletePeakLocation(Peaks peak)
        {
            CompleteCheck(GetPeakLocationID(peak));
        }

        public void CompleteFSPeakLocation(Peaks peak)
        {
            CompleteCheck(GetFSPeakLocationID(peak));
        }

        public void CompleteArtefactLocation(Artefacts artefact)
        {
            CompleteCheck(GetArtefactLocationID(artefact));
        }

        internal void CompleteRopeLocation(Ropes rope)
        {
            CompleteCheck(GetRopeLocationID(rope));
        }
        internal void CompleteBirdSeedLocation(BirdSeeds seed)
        {
            CompleteCheck(GetBirdSeedLocationID(seed));
        }
        internal void CompleteTimePBLocation(Peaks peak)
        {
            CompleteCheck(GetTATimePBLocationID(peak));
        }

        internal void CompleteRopePBLocation(Peaks peak)
        {
            CompleteCheck(GetTARopeLocationID(peak));
        }

        internal void CompleteHoldPBLocation(Peaks peak)
        {
            CompleteCheck(GetTAHoldsLocationID(peak));
        }

        private void CompleteCheck(long locationID)
        {
            if (session == null)
            {
                return;
            }
            if (scoutedItems != null && scoutedItems.ContainsKey(locationID) 
                && session.Locations.AllMissingLocations.Contains(locationID))
            {
                ScoutedItemInfo item = scoutedItems[locationID];
                PeaksOfArchipelago.ui.SendNotification($"Found {item.Player.Name}'s {item.ItemName}");
            }

            session.Locations.CompleteLocationChecks(locationID);
        }

        internal bool HasLocation(long v)
        {
            return session != null && !session.Locations.AllMissingLocations.Contains(v);
        }
    }
}
