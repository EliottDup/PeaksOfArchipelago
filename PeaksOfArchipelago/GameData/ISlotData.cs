using System;
using System.Collections.Generic;
using System.Text;

namespace PeaksOfArchipelago.GameData
{
    internal interface ISlotData
    {
        SessionSettings.RopeUnlockMode ropeUnlockMode { get; set; }
        int cramponLevel { get; set; }
        bool hasLamp { get; set; }
        bool rightHand { get; set; }
        bool leftHand { get; set; }

        void UnlockedArtefact(Artefacts artefact);
        void UnlockedBirdSeed(BirdSeeds birdSeed);
        void UnlockedBook(Books book);

        // Needs: Some way to access the peaks
        // Some way to access artefacts
        // 
        void UnlockedPeak(Peaks peak);
        void UnlockedRope(Ropes rope);
        void UnlockedTool(Tools tool);
    }
}
