using System;
using System.Collections.Generic;
using System.Text;

namespace PeaksOfArchipelago.GameData
{
    internal class SlotData : ISlotData
    {
        // Needs: Some way to access the peaks
        // Some way to access artefacts
        //
        public SessionSettings.RopeUnlockMode ropeUnlockMode { get; set; }
        public int cramponLevel { get; set; }
        public bool hasLamp { get; set; }
        public bool rightHand { get; set; }
        public bool leftHand { get; set; }

        public SlotData(SessionSettings settings)
        {
            ropeUnlockMode = settings.ropeUnlockMode;
            return;
        }

        public void UnlockedArtefact(Artefacts artefact)
        {
            
        }

        public void UnlockedBirdSeed(BirdSeeds birdSeed)
        {
            
        }

        public void UnlockedBook(Books book)
        {
            
        }

        public void UnlockedPeak(Peaks peak)
        {
            
        }

        public void UnlockedRope(Ropes rope)
        {
            
        }

        public void UnlockedTool(Tools tool)
        {
            
        }
    }
}
