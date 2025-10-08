using UnityEngine;
using UnityEngine.UI;

namespace PeaksOfArchipelago.UI
{
    internal class UIElementFactory
    {
        public struct PanelData
        {
            public GameObject gameObject;
            public Image image;
            public RectTransform rectTransform;
        }

        public static PanelData CreatePanel(Transform parent, string name, Color? color = null)
        {
            GameObject go = new GameObject(name, typeof(Image), typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Image img = go.GetComponent<Image>();
            img.color = color ?? new Color(0, 0, 0, 0.6f);
            RectTransform rt = go.GetComponent<RectTransform>();
            
            return new PanelData() { gameObject = go, image = img, rectTransform = rt };
        }

        public struct InputFieldData
        {
            public InputField inputField;
        }

        public static InputFieldData createInputField(Transform parent, string name, string placeholder, bool isPassword = false) {
            var go = new GameObject(name, typeof(Image), typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = Color.white;
            RectTransform rt = go.GetComponent<RectTransform>();

            InputField input = go.AddComponent<InputField>();

            // Text Component
            input.textComponent = CreateText(go.transform, "Text", "", 25, TextAnchor.MiddleLeft, scale2parent: true, padding: 0.1f);

            // Placeholder Text
            input.placeholder = CreateText(go.transform, "Placeholder", placeholder, 25, TextAnchor.MiddleLeft, new Color(0.5f, 0.5f, 0.5f), true, 0.1f);

            if (isPassword)
                input.contentType = InputField.ContentType.Password;

            return new InputFieldData() { inputField = input };
        }

        public struct LayoutData
        {
            public GameObject gameObject;
            public RectTransform rectTransform;
            public HorizontalOrVerticalLayoutGroup layoutGroup;
        }

        public static LayoutData CreateLayoutObject(Transform parent, string name, bool vertical, float spacing = 10, TextAnchor childAlignment = TextAnchor.MiddleCenter)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            HorizontalOrVerticalLayoutGroup layout;
            if (vertical)
            {
                layout = go.AddComponent<VerticalLayoutGroup>();
            }
            else
            {
                layout = go.AddComponent<HorizontalLayoutGroup>();
            }
            layout.childAlignment = childAlignment;
            layout.spacing = 10;

            return new LayoutData() { gameObject = go, rectTransform = go.GetComponent<RectTransform>(), layoutGroup = layout };
        }




        public static UnityEngine.UI.Button CreateButton(Transform parent, string label, System.Action onClick)
        {
            GameObject go = new GameObject($"{label}button", typeof(Image), typeof(UnityEngine.UI.Button));
            go.transform.SetParent(parent, false);
            go.GetComponent <Image>().color = Color.white;

            Text text = CreateText(go.transform, "Text", label, 25, scale2parent: true);
            text.horizontalOverflow = HorizontalWrapMode.Overflow;

            UnityEngine.UI.Button button = go.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() => onClick?.Invoke());

            return button;
        }

        public static Text CreateText(Transform parent, string name, string message, int fontsize = 15, TextAnchor alignment = TextAnchor.MiddleCenter, Color? color = null, bool scale2parent = false, float padding = 0)
        {
            GameObject go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = UIManager.GetFont();
            text.fontSize = fontsize;
            text.text = message;
            text.alignment = alignment;
            text.color = color ?? Color.black;
            if (scale2parent) {
                go.GetComponent<RectTransform>().anchorMin = Vector2.zero + Vector2.one * padding;
                go.GetComponent<RectTransform>().anchorMax = Vector2.one - Vector2.one * padding;
                go.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                go.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            }
            return text;
        }

    }
}
