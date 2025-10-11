using BepInEx;
using BepInEx.Logging;
using PeaksOfArchipelago.Session;
using PeaksOfArchipelago.UI;
using UnityEngine;
using HarmonyLib;
using UnityEngine.SceneManagement;
using System.Reflection;

namespace PeaksOfArchipelago
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class PeaksOfArchipelago : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        public static UIManager ui;

        private Harmony harmony;

        public void Awake()
        { 
            Logger = base.Logger;
            Assets.PeaksOfAssets.LoadAssets();
            this.harmony = new(MyPluginInfo.PLUGIN_GUID + "_Patcher");
            try
            {
                Logger.LogInfo("Patching...");
                this.harmony.PatchAll(typeof(Patches.AchievementBlockPatches));
                Logger.LogInfo("Patch complete!");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during patching: {ex}");
            }

            ui = new UIManager();
            SceneManager.sceneLoaded += OnSceneLoad;
            Logger.LogMessage($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
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