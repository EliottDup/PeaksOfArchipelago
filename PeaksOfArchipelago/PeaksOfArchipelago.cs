using BepInEx;
using BepInEx.Logging;
using PeaksOfArchipelago.Session;
using PeaksOfArchipelago.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeaksOfArchipelago
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class PeaksOfArchipelago : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        private UIManager ui;
        public void Awake()
        {
            ui = new UIManager();
            Logger = base.Logger;
            Logger.LogMessage($"Plugin c0der23.PeaksOfArchipelago is loaded!");
            SceneManager.sceneLoaded += OnSceneLoad;
        }

        public void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
        {
            UIManager.CreateLoginUI(AttemptLogin);
            ui.OnSceneLoaded();
            Logger.LogInfo($"Loaded scene index: {scene.buildIndex}");
        }

        private async Task<bool> AttemptLogin(string username, string ip, string password)
        {
            Logger.LogInfo("creating session");
            PeaksSession session = new();
            Logger.LogInfo("Session Created");
            return await session.ConnectAndLogin(username, ip, password);
        }
    }
}