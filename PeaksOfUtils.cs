using System;
using Archipelago.MultiClient.Net.Packets;
using UnityEngine;

namespace PeaksOfArchipelago;

public enum Ropes
    {
        WaltersCrag,
        WalkersPillar,
        GreatGaol,
        StHaelga,
        ExtraFirst,
		ExtraSecond,
		ExtraThird,
		ExtraFourth,
		Extra5,
		Extra6,
		Extra7,
		Extra8,
		Extra9,
		Extra10,
		Extra11,
		Extra12,
        None
    }

public enum Checks {

}

class Utils{
    static public Ropes GetRopeCollectable(RopeCollectable ropeCollectable){
        if (!ropeCollectable.isSingleRope){
            return (Ropes)ropeCollectable.extraRopeNumber;
        }
        if (ropeCollectable.isWaltersCrag) return Ropes.WaltersCrag;
        if (ropeCollectable.isWalkersPillar) return Ropes.WalkersPillar;
        if (ropeCollectable.isGreatGaol) return Ropes.GreatGaol;
        if (ropeCollectable.isStHaelga) return Ropes.StHaelga;
        return Ropes.None;
    }
}