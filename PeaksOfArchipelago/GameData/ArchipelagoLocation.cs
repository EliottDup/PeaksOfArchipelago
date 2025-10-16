using System;
using System.Collections.Generic;
using System.Text;

namespace PeaksOfArchipelago.GameData
{
    internal abstract class ArchipelagoLocation
    {
        public int ArchipelagoID { get; private set; }
        public bool IsCompleted { get; private set; } = false;
        public ArchipelagoLocation(int archipelagoId)
        {
            ArchipelagoID = archipelagoId;
        }

        public void Complete()
        {
            IsCompleted = true;
        }
    }
}
