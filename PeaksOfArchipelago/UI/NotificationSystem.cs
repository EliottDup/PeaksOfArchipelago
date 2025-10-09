using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeaksOfArchipelago.UI
{
    internal class NotificationSystem
    {
        ManualLogSource logger;
        public void Awake()
        {
            logger = PeaksOfArchipelago.Logger;

        }

        public void Notify(string message)
        {

        }
    }
}
