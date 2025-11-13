using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using BepInEx.Logging;
using PeaksOfArchipelago.CabinHandlers;
using PeaksOfArchipelago.GameData;

namespace PeaksOfArchipelago.Session
{
    internal class Connection
    {
        public static Connection Instance { get; private set; }

        ArchipelagoSession session;
        ManualLogSource logger;
        private Dictionary<string, object> slotOptions;
        private ISlotData slotData;
        private List<ItemInfo> uncollectedItems;
        private List<ItemInfo> instantCollectItems;

        private int itemCount = 0;


        public Connection() {
            logger = PeaksOfArchipelago.Logger;
            PeaksOfArchipelago.Instance.OnEnterCabin += OnEnterCabin;
            Instance = this;
        }

        public async Task<bool> ConnectAndLogin(string username, string uri, string password) {

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
            LoginResult result = await session.LoginAsync("Peaks of Yore", username, ItemsHandlingFlags.AllItems, password:  password);
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

            return true;
        }

        private void LoadData()
        {
            // init this.slotData
            // This does not actually apply the loaded data to the GameManager (because that would break save 1 for some reason I think)
            this.slotData = new SlotData(new SessionSettings(slotOptions));

            itemCount = (int)session.DataStorage["ItemCount"];
            instantCollectItems = [.. session.Items.AllItemsReceived.Take(itemCount).Where(item => item.ItemName != "Trap")];
            uncollectedItems = [.. session.Items.AllItemsReceived.Skip(itemCount).Where(item => item.ItemName != "Trap")];

            logger.LogInfo($"Loaded {instantCollectItems.Count} old items and {uncollectedItems.Count} new items");
        }

        public void OnEnterCabin(object sender, Cabins cabin)
        {
            if (instantCollectItems.Count > 0)
            {
                logger.LogInfo("Collecting previously unlocked items...");
                foreach (ItemInfo item in instantCollectItems)
                {
                    UnlockItem(item);
                }
                itemCount += instantCollectItems.Count;
                session.DataStorage["ItemCount"] = itemCount;
                instantCollectItems.Clear();
            }
            // TODO: Collect Items
            CabinHandler handler = CabinHandler.New(cabin, slotData);
            if (uncollectedItems.Count > 0)
            {
                logger.LogInfo("Collecting new items...");
                handler.CollectItems(uncollectedItems);
                foreach (ItemInfo item in uncollectedItems)
                {
                    UnlockItem(item);
                }
                itemCount += uncollectedItems.Count;
                session.DataStorage["ItemCount"] = itemCount;
            }
            handler.LoadProgress();
        }

        private void OnItemReceived(ItemInfo item)
        {
            uncollectedItems.Add(item);
            // TODO: Notify player
        }

        private void UnlockItem(ItemInfo item)
        {
            ArchipelagoItem APItem = ArchipelagoItem.Create(item);
            APItem.Unlock(slotData);
        }
    }
}
