using System;
using System.Collections.Generic;
using System.Text;

namespace PeaksOfArchipelago.GameData
{
    internal abstract class PeakCollectable : ArchipelagoLocation
    {
        protected PeakCollectable(int archipelagoId) : base(archipelagoId)
        {
        }

    }
}
