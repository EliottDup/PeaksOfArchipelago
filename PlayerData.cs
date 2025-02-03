namespace PeaksOfArchipelago;

class PlayerData
{
    public class Locations
    {
        public CheckList<Ropes> ropes = new();
        public CheckList<Artefacts> artefacts = new();
        public CheckList<Peaks> peaks = new();
    }

    public class Items
    {
        public CheckList<Ropes> ropes = new();
        public CheckList<Artefacts> artefacts = new();
        public CheckList<Books> books = new();

        public int extraropeItemCount = 0;
        public int extraChalkItemCount = 0;
        public int extraCoffeeItemCount = 0;
        public int extraSeedItemCount = 0;
    }

    public Locations locations = new();
    public Items items = new();
}

