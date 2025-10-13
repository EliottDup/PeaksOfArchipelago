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
