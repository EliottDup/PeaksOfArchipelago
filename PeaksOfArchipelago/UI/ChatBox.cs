using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PeaksOfArchipelago.UI
{
    internal class ChatBox : MonoBehaviour
    {
        private ManualLogSource logger;
        private Canvas canvas;
        private GameObject chatBoxRoot;
        private Transform messageHolder;

        public void Awake()
        {
            logger = PeaksOfArchipelago.Logger;
            canvas = GameObject.FindObjectOfType<Canvas>();
        }

        private void Start()
        {
            CreateChatBox();
            AddChatMessage("test Message 1");
            AddChatMessage("test Message 2\n2lines now :)");
            AddChatMessage("test Message 3: very long line that should overflow and not explode everything (hopefully), if it does I will be sad");
            AddChatMessage("test Message 3: very long line that should overflow and not explode everything (hopefully), if it does I will be sad");
            AddChatMessage("test Message 3: very long line that should overflow and not explode everything (hopefully), if it does I will be sad");
            AddChatMessage("test Message 3: very long line that should overflow and not explode everything (hopefully), if it does I will be sad");
            AddChatMessage("test Message 3: very long line that should overflow and not explode everything (hopefully), if it does I will be sad");
            AddChatMessage("test Message 3: very long line that should overflow and not explode everything (hopefully), if it does I will be sad");
            AddChatMessage("cool message now with <color=#FF0000>C</color><color=#FFFF00>o</color><color=#00FF00>l</color><color=#00FEFF>o</color><color=#0000FF>r</color><color=#FF00FE>s</color>");
        }

        void CreateChatBox()
        {
            if (canvas == null)
            {
                logger.LogError("Canvas not found!");
                return;
            }
            chatBoxRoot = Instantiate(Assets.PeaksOfAssets.ChatBoxPrefab, canvas.transform, false);
            messageHolder = chatBoxRoot.transform.GetChild(0);
        }

        public void AddChatMessage(string message)
        {
            if (messageHolder == null) return;
            logger.LogInfo("AddChatMessage: " + message);
            GameObject messageObject = Instantiate(Assets.PeaksOfAssets.ChatMessagePrefab, messageHolder);
            Text t = messageObject.GetComponentInChildren<Text>();
            t.text = message;
        }
    }
}
