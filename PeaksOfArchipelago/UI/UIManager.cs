using BepInEx.Logging;
using PeaksOfArchipelago.Assets;
using PeaksOfArchipelago.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Font = UnityEngine.Font;

namespace PeaksOfArchipelago.UI
{
    public class UIManager
    {
        private static Font _gameFont;

        private Canvas canvas;
        private GameObject chatBoxObject;
        private ManualLogSource logger;
        private ChatBox chat;
        private GameObject notificationSystemObject;
        private Notificator notificationSystem;

        public UIManager()
        {
            logger = PeaksOfArchipelago.Logger;
        }

        private void MakeUI()
        {
            canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                logger.LogError("Canvas not found!");
                return;
            }

            // Make ChatBox
            chatBoxObject = new GameObject("UIManager_ScriptHolder");
            chat = chatBoxObject.AddComponent<ChatBox>();
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
