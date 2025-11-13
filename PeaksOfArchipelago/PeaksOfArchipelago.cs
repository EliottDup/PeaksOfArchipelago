using BepInEx;
using BepInEx.Logging;
using PeaksOfArchipelago.Session;
using PeaksOfArchipelago.UI;
using UnityEngine;
using HarmonyLib;
using UnityEngine.SceneManagement;
using System.Reflection;
using PeaksOfArchipelago.GameData;
using BepInEx.Configuration;

namespace PeaksOfArchipelago
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class PeaksOfArchipelago : BaseUnityPlugin
    {
        public static PeaksOfArchipelago Instance { get; private set; }

        internal static new ManualLogSource Logger;
        public static UIManager ui;
        public static PlayerState playerState { get; private set; } = PlayerState.InMainMenu;

        public event EventHandler<GameData.Cabins> OnEnterCabin;

        private Harmony harmony;
        private Connection connection;

        public ConfigEntry<string> Username;
        public ConfigEntry<string> ServerIP;
        public ConfigEntry<string> Password;

        public enum PlayerState
        {
            InMainMenu,
            InCabin,
            InPeak
        }

        public void Awake()
        {
            Username = Config.Bind("Session", "Username", "", "The last username used to connect to an Archipelago session.");
            ServerIP = Config.Bind("Session", "ServerIP", "", "The last server IP used to connect to an Archipelago session.");
            Password = Config.Bind("Session", "Password", "", "The last password used to connect to an Archipelago session.");

            Instance = this;

            Logger = base.Logger;
            Assets.PeaksOfAssets.LoadAssets();

            this.harmony = new(MyPluginInfo.PLUGIN_GUID + "_Patcher");
            try
            {
                Logger.LogInfo("Patching...");
                this.harmony.PatchAll(typeof(Patches.AchievementBlockPatches));
                this.harmony.PatchAll(typeof(Patches.NPCEventsPatches));
                this.harmony.PatchAll(typeof(Patches.ArtefactLoaderCabinPatches));
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
            if ( connection == null && scene.buildIndex == 0)
            {
                UIManager.CreateLoginUI(AttemptLogin);
            }

            ui.OnSceneLoaded();
            Logger.LogInfo($"Loaded scene index: {scene.buildIndex}");
            // Scene buildIndex:
            // 0 = Main Menu
            // 1 = Cabin
            // 37 = Cabin4
            // 67 = Alpine Express/alps cabin
            if (connection == null && scene.buildIndex != 0) throw new Exception("How tf did you enter the game without connecting??");
            switch (scene.buildIndex)
            {
                case 0:
                    playerState = PlayerState.InMainMenu;
                    break;

                case 1:
                    playerState = PlayerState.InCabin;
                    Logger.LogInfo("In Cabin");
                    OnEnterCabin?.Invoke(this, GameData.Cabins.Cabin);
                    break;

                case 37:
                    playerState = PlayerState.InCabin;

                    Logger.LogInfo("In Cabin");
                    OnEnterCabin?.Invoke(this, GameData.Cabins.CabinExpert);
                    break;

                case 67:
                    playerState = PlayerState.InCabin;

                    Logger.LogInfo("In Cabin");
                    OnEnterCabin?.Invoke(this, GameData.Cabins.CabinAlps);
                    break;

                default:
                    playerState = PlayerState.InPeak;
                    break;
            }
        }

        public void SaveUserCredentials(string username, string uri, string password)
        {
            Username.Value = username;
            ServerIP.Value = uri;
            Password.Value = password;
            Config.Save();
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