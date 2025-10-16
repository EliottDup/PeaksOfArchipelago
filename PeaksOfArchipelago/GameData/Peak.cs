using System;
using System.Collections.Generic;
using System.Text;

namespace PeaksOfArchipelago.GameData
{
    internal class Peak : ArchipelagoLocation
    {
        public StamperPeakSummit.PeakNames PeakName { get; private set; }   
        public bool unlocked = false;
        public bool summited = false;
        public IPeakCollectable[] Collectables { get; private set; }


        public Peak(StamperPeakSummit.PeakNames peak, IPeakCollectable[] collectables) : base((int)peak + Offsets.PeakIDOffset)
        {
            PeakName = peak;
            Collectables = collectables;

        }

    }
}
