using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PeaksOfArchipelago.GameData
{
    internal interface ISlotData
    {
        SessionSettings.RopeUnlockMode ropeUnlockMode { get; set; }
        SessionSettings.GameMode gameMode { get; set; }
        int cramponLevel { get; set; }

        bool HasBook(Books book);
        bool ShowBook(Books book);

        bool HasTool(Tools tool);

        bool HasPeak(Peaks peak);

        bool HasIdol(Idols idol);

        int GetTotalRopeCount();
        int GetTotalCoffeeCount();

        int GetExtraItemCount(ExtraItems item);
        void RecieveArtefact(Artefacts artefact);
        void RecieveBirdSeed(BirdSeeds birdSeed);
        void RecieveBook(Books book);
        void ExtraItemReceived(ExtraItems item);

        // Needs: Some way to access the peaks
        // Some way to access artefacts
        // 
        void RecievePeak(Peaks peak);
        void RecieveRope(Ropes rope);
        void ReceiveTool(Tools tool);
        bool IsJournalPageUnlocked(int v, Books b);
        Peaks BookPageToPeaks(int page, Books book);
        Color GetJournalPageColor(int v, Books b);
        int GetTotalExtraBirdSeedCount();
        void receiveIdol(Idols idol);
    }
}
