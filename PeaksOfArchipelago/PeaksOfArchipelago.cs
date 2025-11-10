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
        public static PlayerState playerState { get; private set; } = PlayerState.InMainMenu;

        private Harmony harmony;
        private Connection connection;

        public enum PlayerState
        {
            InMainMenu,
            InCabin,
            InPeak
        }

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
            // Scene buildIndex:
            // 0 = Main Menu
            // 1 = Cabin
            // 37 = Cabin4
            // 67 = Alpine Express
            if (connection == null) throw new Exception("How tf did you enter the game without connecting??");
            switch (scene.buildIndex)
            {
                case 0:
                    playerState = PlayerState.InMainMenu;
                    break;
                case 1:
                    playerState = PlayerState.InCabin;
                    connection.OnEnterCabin(GameData.Cabins.Cabin);
                    Logger.LogInfo("In Cabin");
                    break;
                case 37:
                    playerState = PlayerState.InCabin;
                    connection.OnEnterCabin(GameData.Cabins.CabinExpert);
                    Logger.LogInfo("In Cabin");
                    break;
                case 67:
                    playerState = PlayerState.InPeak;
                    connection.OnEnterCabin(GameData.Cabins.CabinAlps);
                    Logger.LogInfo("In Peak");
                    break;
                default:
                    playerState = PlayerState.InPeak;
                    break;
            }
        }

        private async Task<bool> AttemptLogin(string username, string ip, string password)
        {
            Logger.LogInfo("creating session");
            connection = new();
            Logger.LogInfo("Session Created");
            return await connection.ConnectAndLogin(username, ip, password);
        }
    }
}