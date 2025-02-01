using System;

namespace PeaksOfArchipelago;

class PlayerData
{
    class Checks
    {
        private CheckList<Ropes> ropes = new CheckList<Ropes>();
        private CheckList<Artefacts> artefacts = new();
        private CheckList<Peaks> peaks = new();
    }

    class Items
    {

    }

    Checks checks = new();
    Items items = new();

}

