using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using BepInEx.Logging;
using PeaksOfArchipelago.CabinHandlers;
using PeaksOfArchipelago.GameData;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeaksOfArchipelago.Session
{
    internal class Connection
    {
        ArchipelagoSession session;
        ManualLogSource logger;
        private Dictionary<string, object> slotOptions;
        private ISlotData slotData;
        private List<ItemInfo> uncollectedItems;
        private List<ItemInfo> instantCollectItems;

        private int itemCount = 0;

        public Connection() {
            logger = PeaksOfArchipelago.Logger;
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

            itemCount = (int)session.DataStorage["ItemCount"];
            instantCollectItems = [.. session.Items.AllItemsReceived.Take(itemCount).Where(item => item.ItemName != "Trap")];
            uncollectedItems = [.. session.Items.AllItemsReceived.Skip(itemCount).Where(item => item.ItemName != "Trap")];

            logger.LogInfo($"Loaded {instantCollectItems.Count} old items and {uncollectedItems.Count} new items");
        }

        public void OnEnterCabin(Cabins cabin)
        {
            if (instantCollectItems.Count > 0)
            {
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
            handler.OnEnterCabin();
        }

        private void OnItemReceived(ItemInfo item)
        {
            uncollectedItems.Add(item);
            // TODO: Notify player ??
        }

        private void UnlockItem(ItemInfo item)
        {
            ArchipelagoItem APItem = ArchipelagoItem.Create(item);
            APItem.Unlock(slotData);
        }
    }
}
