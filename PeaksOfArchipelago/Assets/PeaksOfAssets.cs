using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PeaksOfArchipelago.Assets
{
    internal class PeaksOfAssets
    {
        private static bool loaded = false;
        private static AssetBundle assetBundle;
        public static GameObject ChatBoxPrefab { get; private set; }
        public static GameObject ChatMessagePrefab { get; private set; }
        public static GameObject LoginScreen { get; private set; }
        public static GameObject APLogo { get; private set; }
        public static GameObject Notificator { get; private set; }
        public static GameObject Notification { get; private set; }

        public static GameObject ProgressDisplay { get; private set; }
        public static GameObject BookEntryPrefab { get; private set; }
        public static GameObject PeakEntryPrefab { get; private set; }

        public static void LoadAssets()
        {
            if (loaded) return;
            PeaksOfArchipelago.Logger.LogInfo("Loading Assets...");

            string assetsFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");
            
            assetBundle = AssetBundle.LoadFromFile(Path.Combine(assetsFolder, "peaksofbundle"));
            ChatBoxPrefab = assetBundle.LoadAsset<GameObject>("ChatBox");
            ChatMessagePrefab = assetBundle.LoadAsset<GameObject>("ChatMessage");
            LoginScreen = assetBundle.LoadAsset<GameObject>("LogInPrefab");
            APLogo = assetBundle.LoadAsset<GameObject>("APLogo");

            Notificator = assetBundle.LoadAsset<GameObject>("NotificationMaker");
            Notification = assetBundle.LoadAsset<GameObject>("Notification");

            ProgressDisplay = assetBundle.LoadAsset<GameObject>("ProgressDisplay");
            BookEntryPrefab = assetBundle.LoadAsset<GameObject>("BookPanel");
            PeakEntryPrefab = assetBundle.LoadAsset<GameObject>("PeakEntry");

            PeaksOfArchipelago.Logger.LogInfo("Assets Loaded");
            loaded = true;
        }

    }
}
