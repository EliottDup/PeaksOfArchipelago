using System.Collections;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;

class UIHandler : MonoBehaviour
{
    public static UIHandler instance;
    public static ManualLogSource logger;
    public static Font poyFont;
    Text chat;
    GameObject canvas;


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

    void CreateUI()
    {
        GameObject lg = CreateVerticalPanel("panel", canvas.transform, new Vector2(-25, 0), 1f, 0.5f, false, 400, 400);
        lg.GetComponent<VerticalLayoutGroup>().childControlWidth = true;
        lg.GetComponent<VerticalLayoutGroup>().childControlHeight = true;
        lg.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = true;
        lg.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = true;
        lg.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.LowerCenter;
        // chatBox = lg;
        chat = CreateTextElem("testText", 32, lg.transform, TextAnchor.LowerLeft);
        chat.GetComponent<ContentSizeFitter>().enabled = false;
        chat.horizontalOverflow = HorizontalWrapMode.Wrap;
        chat.verticalOverflow = VerticalWrapMode.Overflow;
        RectMask2D rm2d = lg.gameObject.AddComponent<RectMask2D>();
        rm2d.AddClippable(chat);
        chat.text = "";
    }

    public void AddChatMessage(string text)
    {
        chat.text += "\n" + text;
    }

    public void ShowText(string text)
    {
        StartCoroutine(FadeAndShowText(text));
    }

    IEnumerator FadeAndShowText(string text)
    {
        yield return new WaitForEndOfFrame();
        GameObject p = CreateVerticalPanel("textmoment", canvas.transform, new Vector2(0, 0), .5f, 0.75f);
        CanvasGroup cv = p.AddComponent<CanvasGroup>();
        Text t = CreateTextElem("Textmomento", 32, p.transform);
        t.text = text;

        float timer = 0;
        while (timer < 1f)
        {
            timer += Time.deltaTime * 2f;
            cv.alpha = timer;
            yield return null;
        }
        cv.alpha = 1;
        yield return new WaitForSeconds(2);
        timer = 0;
        while (timer < 1f)
        {
            timer += Time.deltaTime * 2f;
            cv.alpha = 1 - timer;
            yield return null;
        }
        cv.alpha = 0;
        yield return null;
        Destroy(p);
    }


    static GameObject CreateVerticalPanel(string name, Transform parent, Vector2 position, float pivotx = 0.5f, float pivoty = 0.5f, bool addCSF = true, float sizex = 0, float sizey = 0)
    {
        GameObject p = new GameObject(name);
        p.transform.SetParent(parent, false);
        Image pImage = p.AddComponent<Image>();
        pImage.color = new Color(0, 0, 0, 0.8f);

        RectTransform pRect = p.GetComponent<RectTransform>() ?? p.AddComponent<RectTransform>();

        Vector2 pivotPosition = new Vector2(pivotx, pivoty);
        pRect.anchorMax = pivotPosition;
        pRect.anchorMin = pivotPosition;
        pRect.pivot = pivotPosition;
        pRect.anchoredPosition = position;

        VerticalLayoutGroup vlg = p.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 25;
        vlg.padding.bottom = 30;
        vlg.padding.right = 10;
        vlg.padding.left = 10;
        vlg.padding.top = 10;

        if (addCSF)
        {
            ContentSizeFitter csf = p.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        else
        {
            pRect.sizeDelta = new Vector2(sizex, sizey);
        }

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