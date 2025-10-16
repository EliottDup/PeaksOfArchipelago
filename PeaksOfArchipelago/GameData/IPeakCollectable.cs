using System;
using System.Collections.Generic;
using System.Text;

namespace PeaksOfArchipelago.GameData
{
    internal interface IPeakCollectable
    {
        public int ArchipelagoID { get; }
        public string Name { get; }
        public bool IsCollected { get; }
        public void Collect();
    }
}
