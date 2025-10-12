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
        private InputField usernameField;
        private InputField ipField;
        private InputField passwordField;
        private Text statusText;
        private UnityEngine.UI.Button connectButton;
        private GameObject panel;
        Func<string, string, string, Task<bool>> attemptLogin;
        private bool isConnecting;

        public void CreateLoginUI(Func<string, string, string, Task<bool>> attemptLogin)
        {
            GameObject root = GameObject.Find("InGameMenuObj_DisableMe");
            if (root == null) { return; }
            this.attemptLogin = attemptLogin;

            panel = GameObject.Instantiate(Assets.PeaksOfAssets.LoginScreen, root.transform);
            Transform LogInPanel = panel.transform.GetChild(0);
            usernameField = LogInPanel.GetChild(0).GetComponent<InputField>();
            ipField = LogInPanel.GetChild(1).GetComponent<InputField>();
            passwordField = LogInPanel.GetChild(2).GetComponent<InputField>();
            statusText = LogInPanel.GetChild(3).GetChild(0).GetComponent<Text>();
            connectButton = LogInPanel.GetChild(4).GetComponent<UnityEngine.UI.Button>();
            connectButton.onClick.AddListener(() => _ = OnConnectClickedAsync());

        }
        
        private async Task OnConnectClickedAsync()
        {
            if (isConnecting) return;
            statusText.text = "Connecting...";
            isConnecting = true;
            PeaksOfArchipelago.Logger.LogInfo("Button buttonning");
            PeaksOfArchipelago.Logger.LogInfo(attemptLogin.ToString());

            //bool res = attemptLogin.Invoke("a", "b", "c");
            bool res = await attemptLogin(usernameField.text, ipField.text, passwordField.text);

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
