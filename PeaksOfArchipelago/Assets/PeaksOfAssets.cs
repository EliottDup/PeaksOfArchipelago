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

        public static void LoadAssets()
        {
            if (loaded) return;
            PeaksOfArchipelago.Logger.LogInfo("Loading Assets...");

            string assetsFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");
            
            assetBundle = AssetBundle.LoadFromFile(Path.Combine(assetsFolder, "peaksofbundle"));
            ChatBoxPrefab = assetBundle.LoadAsset<GameObject>("ChatBox");
            ChatMessagePrefab = assetBundle.LoadAsset<GameObject>("ChatMessage");
            LoginScreen = assetBundle.LoadAsset<GameObject>("LogInPrefab");

            PeaksOfArchipelago.Logger.LogInfo("Assets Loaded");
            loaded = true;
        }

    }
}
