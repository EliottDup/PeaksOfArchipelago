using System;
using System.Collections.Generic;
using System.Text;

namespace PeaksOfArchipelago.Session
{
    internal class Player
    {
        public enum PlayerState
        {
            InMainMenu,
            InCabin,
            InPeak
        }

        public PlayerState state;
    }
}
