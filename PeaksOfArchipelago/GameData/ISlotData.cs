using System;
using System.Collections.Generic;
using System.Text;

namespace PeaksOfArchipelago.GameData
{
    internal interface ISlotData
    {
        SessionSettings.RopeUnlockMode ropeUnlockMode { get; set; }
        int cramponLevel { get; set; }

        bool HasBook(Books book);

        bool HasArtefact(Artefacts artefact);

        bool HasTool(Tools tool);

        int GetRopeCount();

        void UnlockArtefact(Artefacts artefact);
        void UnlockBirdSeed(BirdSeeds birdSeed);
        void UnlockBook(Books book);
        void ExtraItemReceived(ExtraItems item);

        // Needs: Some way to access the peaks
        // Some way to access artefacts
        // 
        void UnlockPeak(Peaks peak);
        void UnlockRope(Ropes rope);
        void UnlockTool(Tools tool);
    }
}
