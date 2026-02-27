using BepInEx.Logging;
using PeaksOfArchipelago.Assets;
using PeaksOfArchipelago.MonoBehaviours;
using PeaksOfArchipelago.Session;
using UnityEngine;
using UnityEngine.UI;
using Font = UnityEngine.Font;

namespace PeaksOfArchipelago.UI
{
    public class UIManager
    {
        private static Font _gameFont;

        private Canvas canvas;
        private GameObject scriptholder;
        private ManualLogSource logger;
        private ChatBox chat;
        private GameObject notificationSystemObject;
        private Notificator notificationSystem;

        private ProgressDisplay progressDisplay;

        public UIManager()
        {
            logger = PeaksOfArchipelago.Logger;
        }

        private void MakeUI()
        {
            Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();
            foreach (Canvas c in canvases)
            {
                if (c.isActiveAndEnabled)
                {
                    canvas = c;
                    logger.LogInfo("Canvas found: " + c.name + "\nParents: " + c.transform.parent?.name);
                    if ((c.transform.parent == null || c.transform.parent.name.ToLower() == "sceneobjects")
                        && canvas.name.ToLower() == "canvas")
                    {
                        logger.LogInfo("root canvas found ^W^");
                        break;
                    }
                }
            }
            if (canvas == null)
            {
                logger.LogError("Canvas not found!");
                return;
            }

             //Make ChatBox
            scriptholder = new GameObject("UIManager_ScriptHolder");
            chat = scriptholder.AddComponent<ChatBox>();
            chat.Initialize(canvas);
            notificationSystemObject = GameObject.Instantiate(PeaksOfAssets.Notificator, canvas.transform);

            // Make Notification System
            notificationSystem = notificationSystemObject.AddComponent<Notificator>();
            notificationSystem.notificationPrefab = PeaksOfAssets.Notification;
        }

        public void SendChatMessage(string message) {
            if (chat == null) return;
            chat.AddChatMessage(message);
        }

        public void SendNotification(string message)
        {
            if (notificationSystem == null) return;
            notificationSystem.CreateNotification(message);
        }

        public void OnSceneLoaded()
        {
            MakeUI();
        }

        internal void OnSceneLoaded(Connection connection)
        {
            MakeUI();
            if (connection != null)
            {
                progressDisplay = scriptholder.AddComponent<ProgressDisplay>();
                progressDisplay.Initialize(canvas, connection);
            }
        }

        public void OnSceneUnloaded()
        {

        }

        public static Font GetFont()
        {
            if (_gameFont == null)
            {
                Text text = null;
                GameObject textHolder = GameObject.Find("BeginMain");
                if (!textHolder)
                {
                    GameObject textObject = GameObject.Find("Text");
                    if (textObject != null)
                    {
                        text = textObject.GetComponent<Text>();
                    }
                    return null;
                }
                else
                {
                    text = textHolder.GetComponentInChildren<Text>();
                }
                _gameFont = text.font;
            }
            return _gameFont;
        }
        
        public static void CreateLoginUI(Func<string, string, string, Task<bool>> attemptLogin)
        {
            LoginUI m = new();
            m.CreateLoginUI(attemptLogin);
        }
    }
}
