namespace PeaksOfArchipelago;

class PlayerData
{
    public class Locations
    {
        public CheckList<Ropes> ropes = new();
        public CheckList<Artefacts> artefacts = new();
        public CheckList<Peaks> peaks = new();
        public CheckList<Peaks> fsPeaks = new();
        public CheckList<Peaks> timePBs = new();
        public CheckList<Peaks> holdsPBs = new();
        public CheckList<Peaks> ropesPBs = new();
        public CheckList<BirdSeeds> seeds = new();
    }

    public class Items
    {
        public CheckList<Peaks> peaks = new();
        public CheckList<Ropes> ropes = new();
        public CheckList<Artefacts> artefacts = new();
        public CheckList<Books> books = new();
        public CheckList<BirdSeeds> seeds = new();

        public int extraropeItemCount = 0;
        public int extraChalkItemCount = 0;
        public int extraCoffeeItemCount = 0;
        public int extraSeedItemCount = 0;

        public int progressiveCrampons;
        public bool pipe;
        public bool ropeLengthUpgrade;
        public bool barometer;
        public bool monocular;
        public bool phonograph;
        public bool pocketwatch;
        public bool chalkbag;
        public bool rope;
        public bool coffee;
        public bool lamp;
        public bool rightHand;
        public bool leftHand;
    }

    public Locations locations = new();
    public Items items = new();
}

