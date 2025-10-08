using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Font = UnityEngine.Font;

namespace PeaksOfArchipelago.UI
{
    internal class UIManager
    {
        private static Font _gameFont;

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
