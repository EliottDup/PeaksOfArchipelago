using BepInEx;
using BepInEx.Logging;
using PeaksOfArchipelago;
using PeaksOfArchipelago.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeaksOfArchipelago
{
    [BepInPlugin("c0der23.PeaksOfArchipelago", "Peaks of Archipelago", "0.0.1")]
    public class PeaksOfArchipelago : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        public void Awake()
        {
            Logger = base.Logger;
            Logger.LogMessage($"Plugin c0der23.PeaksOfArchipelago is loaded!");
            SceneManager.sceneLoaded += OnSceneLoad;
        }

        public void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
        {
            Logger.LogInfo(scene.buildIndex);
            UIManager.CreateLoginUI(AttemptLogin);
        }

        private async Task<bool> AttemptLogin(string username, string ip, string password)
        {
            Logger.LogInfo("creating session");
            Session session = new();
            Logger.LogInfo("Session Created");
            return await session.ConnectAndLogin(username, ip, password);
        }
    }
}