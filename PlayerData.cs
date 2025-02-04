namespace PeaksOfArchipelago;

class PlayerData
{
    public class Locations
    {
        public CheckList<Ropes> ropes = new();
        public CheckList<Artefacts> artefacts = new();
        public CheckList<Peaks> peaks = new();
        public CheckList<BirdSeeds> seeds = new();
        public int progressiveCrampons;
        public bool pipe;
        public bool ropeLengthUpgrade;
        public bool barometer;
        public bool monocular;
        public bool phonograph;
        public bool pocketwatch;
        public bool chalkbag;
        public bool rope;
    }

    public class Items
    {
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
    }

    public Locations locations = new();
    public Items items = new();
}

