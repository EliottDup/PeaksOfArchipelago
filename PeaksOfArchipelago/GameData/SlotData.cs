using System;
using System.Collections.Generic;
using System.Text;

namespace PeaksOfArchipelago.GameData
{
    internal class SlotData : ISlotData
    {
        public SessionSettings.RopeUnlockMode ropeUnlockMode { get; set; }
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
            extraItemCounts = [];

            for (int i = 0; i < Enum.GetValues(typeof(ExtraItems)).Length; i++)
            {
                extraItemCounts[(ExtraItems)i] = 0;
            }

            return;
        }

        public void UnlockArtefact(Artefacts artefact)
        {
            unlockedArtefacts.Add(artefact);
        }

        public void UnlockBirdSeed(BirdSeeds birdSeed)
        {
            unlockedBirdSeeds.Add(birdSeed);
        }

        public void UnlockBook(Books book)
        {
            PeaksOfArchipelago.Logger.LogInfo($"Unlocked book: {book}");
            unlockedBooks.Add(book);
        }

        public void UnlockPeak(Peaks peak)
        {
            unlockedPeaks.Add(peak);
        }

        public void UnlockRope(Ropes rope)
        {
            unlockedRopes.Add(rope);
        }

        public void UnlockTool(Tools tool)
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
            return unlockedBooks.Contains(book);
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
    }
}
