using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace PeaksOfArchipelago.UI
{
    internal class LoginUI
    {
        private UIElementFactory.InputFieldData usernameField;
        private UIElementFactory.InputFieldData ipField;
        private UIElementFactory.InputFieldData passwordField;
        private Text statusText;
        private UnityEngine.UI.Button connectButton;
        private UIElementFactory.PanelData panel;
        Func<string, string, string, Task<bool>> attemptLogin;
        private bool isConnecting;

        public void CreateLoginUI(Func<string, string, string, Task<bool>> attemptLogin)
        {
            GameObject root = GameObject.Find("InGameMenuObj_DisableMe");
            if (root == null) { return; }

            this.attemptLogin = attemptLogin;

            // UI
            panel = UIElementFactory.CreatePanel(root.transform, "LoginPanel");

            panel.rectTransform.sizeDelta = new Vector2(100000, 100000);

            UIElementFactory.LayoutData layout = UIElementFactory.CreateLayoutObject(panel.gameObject.transform, "Layout", true);
            layout.rectTransform.sizeDelta = new Vector2(300, 300);


            //Elements
            usernameField = UIElementFactory.createInputField(layout.gameObject.transform, "UsernameField", "username");
            ipField = UIElementFactory.createInputField(layout.gameObject.transform, "IpField", "adress (255.255.255.0:12345)");
            passwordField = UIElementFactory.createInputField(layout.gameObject.transform, "PasswordField", "password", true);
            statusText = UIElementFactory.CreateText(layout.gameObject.transform, "Status", "", fontsize: 25);
            statusText.supportRichText = true;
            statusText.horizontalOverflow = HorizontalWrapMode.Overflow;
            connectButton = UIElementFactory.CreateButton(layout.gameObject.transform, "Connect", () => _ = OnConnectClickedAsync());
        }
        
        private async Task OnConnectClickedAsync()
        {
            if (isConnecting) return;
            statusText.text = "Connecting...";
            isConnecting = true;
            PeaksOfArchipelago.Logger.LogInfo("Button buttonning");
            PeaksOfArchipelago.Logger.LogInfo(attemptLogin.ToString());

            //bool res = attemptLogin.Invoke("a", "b", "c");
            bool res = await attemptLogin(usernameField.inputField.text, ipField.inputField.text, passwordField.inputField.text);

            if (res)
            {
                GameObject.Destroy(panel.gameObject);
            }
            else {
                statusText.text = "<color=red>Error, please check the console and try again</color>";
            }
            isConnecting = false;
        }
    }
}
