using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;

class UIHandler : MonoBehaviour
{
    public static UIHandler instance;
    public static ManualLogSource logger;
    public static Font poyFont;
    Transform chatBoxTransform;
    GameObject canvas;

    Queue<string> messages = new Queue<string>();

    void Awake()
    {
        poyFont = FindObjectOfType<Text>().font;
        logger.LogInfo(poyFont);
        canvas = FindObjectOfType<Canvas>()?.gameObject;
        if (canvas == null)
        {
            logger.LogError("Couldn't find Canvas");
            this.enabled = false;
        }
        instance = this;
    }

    void Start()
    {
        CreateUI();
    }

    void Update()
    {
        if (messages.Count != 0)
        {
            CreateChatMessage(messages.Dequeue());
        }
    }

    void CreateUI()
    {
        chatBoxTransform = CreateVerticalPanel("panel", new Color(0, 0, 0, 0), canvas.transform, new Vector2(-12.5f, 0), 1f, 0.75f, 1f, 0f, TextAnchor.LowerRight).transform;
        chatBoxTransform.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = false;
        chatBoxTransform.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;
    }

    public void AddChatMessage(string text)
    {
        messages.Enqueue(text);
    }

    private void CreateChatMessage(string text)
    {
        GameObject p = CreateVerticalPanel("chatmessagepanel", new Color(0, 0, 0, 0.8f), chatBoxTransform, new Vector2(0, 0), 0, 1, 1, 0, TextAnchor.LowerRight);
        CanvasGroup cg = p.AddComponent<CanvasGroup>();
        Text t = CreateTextElem("chatmessageText", 20, p.transform, alignment: TextAnchor.LowerRight);
        t.text = text;
        StartCoroutine(FadeAndShowText(cg, .5f, 4f, .5f));
    }

    public void ShowText(string text, float fadeInTime = 0.5f, float stayTime = 2f, float fadeOutTime = 0.5f)
    {
        GameObject p = CreateVerticalPanel("textmoment", new Color(0, 0, 0, 0.8f), canvas.transform, new Vector2(0, 0), .5f, 0.75f);
        CanvasGroup cg = p.AddComponent<CanvasGroup>();
        Text t = CreateTextElem("Textmomento", 32, p.transform);
        t.text = text;
        StartCoroutine(FadeAndShowText(cg, fadeInTime, stayTime, fadeOutTime));
    }

    IEnumerator FadeAndShowText(CanvasGroup cg, float fadeInTime, float stayTime, float fadeOutTime)
    {
        yield return new WaitForEndOfFrame();
        cg.alpha = 0;

        float timer = 0;
        while (timer < 1f)
        {
            timer += Time.deltaTime / fadeInTime;
            cg.alpha = timer;
            yield return null;
        }
        cg.alpha = 1;
        yield return new WaitForSeconds(stayTime);
        timer = 0;
        while (timer < 1f)
        {
            timer += Time.deltaTime * fadeOutTime;
            cg.alpha = 1 - timer;
            yield return null;
        }
        cg.alpha = 0;
        yield return null;
        Destroy(cg.gameObject);
    }

    static GameObject CreateVerticalPanel(string name, Color color, Transform parent, Vector2 position, float anchorx = 0.5f, float anchory = 0.5f, float pivotx = -1, float pivoty = -1, TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        GameObject p = new GameObject(name);
        p.transform.SetParent(parent, false);
        Image pImage = p.AddComponent<Image>();
        pImage.color = color;

        RectTransform pRect = p.GetComponent<RectTransform>() ?? p.AddComponent<RectTransform>();


        Vector2 anchorPosition = new Vector2(anchorx, anchory);
        Vector2 pivotPosition = (pivotx == -1) ? anchorPosition : new Vector2(pivotx, pivoty);
        pRect.anchorMax = anchorPosition;
        pRect.anchorMin = anchorPosition;
        pRect.anchoredPosition = position;

        pRect.pivot = pivotPosition;

        VerticalLayoutGroup vlg = p.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = alignment;
        vlg.spacing = 25;
        vlg.padding.bottom = 30;
        vlg.padding.right = 10;
        vlg.padding.left = 10;
        vlg.padding.top = 10;

        ContentSizeFitter csf = p.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return p;
    }

    static Text CreateTextElem(string name, int fontSize, Transform parent, TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        GameObject t = new GameObject(name);
        t.transform.SetParent(parent, false);

        Text tText = t.AddComponent<Text>();
        if (poyFont != null)
        {
            tText.font = poyFont;
        }
        else
        {
            logger.LogError("No font found :(");
            tText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        tText.alignment = alignment;
        tText.fontSize = fontSize;
        tText.color = Color.white;
        tText.lineSpacing = 3;
        tText.supportRichText = true;

        RectTransform tRect = t.GetComponent<RectTransform>() ?? t.AddComponent<RectTransform>();

        ContentSizeFitter csf = t.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return tText;
    }
}