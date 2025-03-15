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
    public GameObject canvas;

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
        // Chat messages
        chatBoxTransform = CreatePanel("panel", new Color(0, 0, 0, 0), canvas.transform, new Vector2(-12.5f, 0), 1f, 0.75f, 1f, 0f, TextAnchor.LowerRight).transform;
        chatBoxTransform.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = false;
        chatBoxTransform.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;
    }

    public void AddChatMessage(string text)
    {
        messages.Enqueue(text);
    }

    private void CreateChatMessage(string text)
    {
        GameObject p = CreatePanel("chatmessagepanel", new Color(0, 0, 0, 0.8f), chatBoxTransform, new Vector2(0, 0), 0, 1, 1, 0, TextAnchor.LowerRight);
        CanvasGroup cg = p.AddComponent<CanvasGroup>();
        Text t = CreateTextElem("chatmessageText", 20, p.transform, alignment: TextAnchor.LowerRight);
        t.text = text;
        StartCoroutine(FadeAndShowText(cg, .5f, 4f, .5f));
    }

    public void ShowText(string text, float fadeInTime = 0.5f, float stayTime = 2f, float fadeOutTime = 0.5f)
    {
        GameObject p = CreatePanel("textmoment", new Color(0, 0, 0, 0.8f), canvas.transform, new Vector2(0, 0), .5f, 0.75f);
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

    public static GameObject CreatePanel(string name, Color color, Transform parent, Vector2 position, float anchorx = 0.5f, float anchory = 0.5f, float pivotx = -1, float pivoty = -1, TextAnchor alignment = TextAnchor.MiddleCenter, bool vertical = true)
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

        HorizontalOrVerticalLayoutGroup vlg = (vertical) ? p.AddComponent<VerticalLayoutGroup>() : p.AddComponent<HorizontalLayoutGroup>();
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

    public static GameObject CreateClock(string name, Color color, Transform parent, Vector2 position, float size, float anchorx = 0.5f, float anchory = 0.5f, float pivotx = -1, float pivoty = -1)
    {
        GameObject p = new GameObject(name);
        p.transform.SetParent(parent, false);

        Image pImage = p.AddComponent<Image>();
        pImage.color = color;
        pImage.sprite = GenerateCircleSprite(Mathf.RoundToInt(size));
        pImage.type = Image.Type.Filled;
        pImage.fillMethod = Image.FillMethod.Radial360;
        pImage.fillOrigin = (int)Image.Origin360.Top;

        RectTransform pRect = p.GetComponent<RectTransform>() ?? p.AddComponent<RectTransform>();

        Vector2 anchorPosition = new Vector2(anchorx, anchory);
        Vector2 pivotPosition = (pivotx == -1) ? anchorPosition : new Vector2(pivotx, pivoty);
        pRect.anchorMax = anchorPosition;
        pRect.anchorMin = anchorPosition;
        pRect.pivot = pivotPosition;

        pRect.anchoredPosition = position;

        pRect.sizeDelta = Vector2.one * size;

        return p;
    }

    public static Text CreateTextElem(string name, int fontSize, Transform parent, TextAnchor alignment = TextAnchor.MiddleCenter)
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

    static Sprite GenerateCircleSprite(int size)    //reminder: This is STUPID and should be changed as soon as I know how
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(size / 2, size / 2);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = (dist <= radius) ? Color.white : new Color(0, 0, 0, 0);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        Rect rect = new Rect(0, 0, size, size);
        return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
    }
}