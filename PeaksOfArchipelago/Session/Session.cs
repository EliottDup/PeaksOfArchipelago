using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using BepInEx.Logging;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeaksOfArchipelago.Session
{
    internal class PeaksSession
    {
        ArchipelagoSession session;
        ManualLogSource logger;
        private LoginSuccessful login;

        public PeaksSession() {
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
            login = (LoginSuccessful)result;

            return true;
        }
    }
}
