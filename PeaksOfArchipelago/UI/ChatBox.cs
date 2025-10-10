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
        }

        void CreateChatBox()
        {
            if (canvas == null)
            {
                logger.LogError("Canvas not found!");
                return;
            }
            chatBoxRoot = new GameObject("ChatPanel");
            Image i = chatBoxRoot.AddComponent<Image>();
            i.color = new Color(0, 0, 0, 0.5f);
            chatBoxRoot.transform.SetParent(canvas.transform, false);
            RectTransform rt = chatBoxRoot.GetComponent<RectTransform>();
            rt.pivot = new Vector2(1, 0.5f);
            rt.anchorMin = new Vector2(1, 0.6f);
            rt.anchorMax = new Vector2(1, 0.6f);
            rt.sizeDelta = new Vector2(Screen.width * 400 / 1920, Screen.height * 300 / 1080);
            rt.anchoredPosition = new Vector2(-10, 0);

            GameObject holder = new GameObject("MessageHolder");
            holder.transform.SetParent(chatBoxRoot.transform, false);

            RectTransform mhRT = holder.GetComponent<RectTransform>() ?? holder.AddComponent<RectTransform>();
            mhRT.pivot = new Vector2(0.5f, 0);
            mhRT.anchorMin = new Vector2(0, 0);
            mhRT.anchorMax = new Vector2(1, 1);
            mhRT.position = new Vector2(0, 0);
            mhRT.anchoredPosition = new Vector2(0, 0);
            mhRT.offsetMax = new Vector2(0, 0);
            mhRT.offsetMin = new Vector2(0, 0);

            VerticalLayoutGroup vlt = holder.AddComponent<VerticalLayoutGroup>();
            vlt.childAlignment = TextAnchor.LowerRight;
            vlt.childForceExpandWidth = true;
            vlt.childControlWidth = true;

            vlt.childForceExpandHeight = false;
            vlt.childControlHeight = false;
            // padding
            vlt.padding = new RectOffset(5, 10, 5, 20);
            vlt.spacing = 25;

            ContentSizeFitter csf = holder.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            holder.AddComponent<LayoutElement>().ignoreLayout = true;

            messageHolder = holder.transform;
        }

        public void AddChatMessage(string message)
        {
            logger.LogInfo("AddChatMessage: " + message);
            if (messageHolder == null) return;
            GameObject go = new GameObject("Text");
            go.transform.SetParent(messageHolder, false);
            Text t = go.AddComponent<Text>();
            t.font = UIManager.GetFont();
            t.fontSize = 25;
            t.text = message;
            t.alignment = TextAnchor.LowerLeft;
            t.lineSpacing = 2.5f;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            ContentSizeFitter csf = go.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }
}
