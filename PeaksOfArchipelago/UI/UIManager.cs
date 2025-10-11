using BepInEx.Logging;
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
        private GameObject scriptHolder;
        private ManualLogSource logger;
        private ChatBox chat;

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
            scriptHolder = new GameObject("UIManager_ScriptHolder");
            chat = scriptHolder.AddComponent<ChatBox>();
        }

        public void SendChatMessage(string message) {
            if (chat == null) return;
            chat.AddChatMessage(message);
        }

        public void OnSceneLoaded()
        {
            MakeUI();
            logger.LogInfo("FontS!!");
            logger.LogInfo(_gameFont.name);
            foreach (string s in _gameFont.fontNames)
            {
                logger.LogInfo(s);
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
