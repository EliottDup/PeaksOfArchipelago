using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PeaksOfArchipelago.GameData
{
    internal class SlotData : ISlotData
    {
        public SessionSettings.RopeUnlockMode ropeUnlockMode { get; set; }
        public SessionSettings.GameMode gameMode { get; set; }
        public int cramponLevel { get; set; }

        private readonly HashSet<Books> unlockedBooks = [];
        private readonly HashSet<Artefacts> unlockedArtefacts = [];
        private readonly HashSet<BirdSeeds> unlockedBirdSeeds = [];
        private readonly HashSet<Peaks> unlockedPeaks = [];
        private readonly HashSet<Ropes> unlockedRopes = [];
        private readonly HashSet<Tools> unlockedTools = [];
        private readonly Dictionary<ExtraItems, int> extraItemCounts = [];

        public SlotData(SessionSettings settings)
        {
            ropeUnlockMode = settings.ropeUnlockMode;
            gameMode = settings.gameMode;
            extraItemCounts = [];

            for (int i = 0; i < Enum.GetValues(typeof(ExtraItems)).Length; i++)
            {
                extraItemCounts[(ExtraItems)i] = 0;
            }

            return;
        }

        public void RecieveArtefact(Artefacts artefact)
        {
            unlockedArtefacts.Add(artefact);
        }

        public void RecieveBirdSeed(BirdSeeds birdSeed)
        {
            unlockedBirdSeeds.Add(birdSeed);
        }

        public void RecieveBook(Books book)
        {
            PeaksOfArchipelago.Logger.LogInfo($"Unlocked book: {book}");
            unlockedBooks.Add(book);
        }

        public void RecievePeak(Peaks peak)
        {
            unlockedPeaks.Add(peak);
        }

        public void RecieveRope(Ropes rope)
        {
            unlockedRopes.Add(rope);
        }

        public void ReceiveTool(Tools tool)
        {
            if (tool == Tools.ProgressiveCrampons)
            {
                cramponLevel++;
            }
            unlockedTools.Add(tool);
        }

        public void ExtraItemReceived(ExtraItems item)
        {
            extraItemCounts[item]++;
        }

        public bool HasBook(Books book)
        {
            return unlockedBooks.Contains(book) || gameMode == SessionSettings.GameMode.PEAK_UNLOCK;
        }

        public bool HasArtefact(Artefacts artefact)
        {
            return unlockedArtefacts.Contains(artefact);
        }

        public int GetRopeCount()
        {
            int ropeCount = 0;
            for (int i = (int)Ropes.WaltersCrag; i <= (int)Ropes.StHaelga; i++)
            {
                Ropes rope = (Ropes)i;
                if (unlockedRopes.Contains(rope))
                {
                    ropeCount++;
                }
            }
            
            for (int i = (int)Ropes.ExtraFirst; i <= (int)Ropes.Extra12; i++)
            {
                Ropes rope = (Ropes)i;
                if (unlockedRopes.Contains(rope))
                {
                    ropeCount += 2;
                }
            }
            return ropeCount;
        }

        public bool HasTool(Tools tool)
        {
            return unlockedTools.Contains(tool);
        }

        public bool HasPeak(Peaks peak)
        {
            return unlockedPeaks.Contains(peak) || gameMode == SessionSettings.GameMode.BOOK_UNLOCK;
        }

        public bool IsJournalPageUnlocked(int page, Books book)
        {
            return HasPeak(BookPageToPeaks(page, book));
        }

        public Peaks BookPageToPeaks(int page, Books book)
        {
            page--;
            Peaks peak;
            if (page < 0) return (Peaks)(-1);
            switch (book)
            {
                case Books.Fundamentals:
                    peak = (Peaks)page;
                    if (peak == Peaks.HangmansLeap) peak = Peaks.LandsEnd;
                    else if (peak == Peaks.LandsEnd) peak = Peaks.HangmansLeap; // the funny swap that makes me want to kill myself
                    return peak;
                case Books.Intermediate:
                    return (Peaks)(page + 20);
                case Books.Advanced:
                    peak = (Peaks)(page + 30);
                    if (peak == Peaks.Eldenhorn) peak = Peaks.GreatGaol;
                    else if (peak == Peaks.GreatGaol) peak = Peaks.Eldenhorn;
                    return peak;
            }
            return (Peaks)(-2);
        }

        public Color GetJournalPageColor(int v, Books b)
        {
            return IsJournalPageUnlocked(v, b) ? Color.white : Color.red;
        }
    }
}
