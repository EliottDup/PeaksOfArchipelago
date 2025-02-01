using System;
using Archipelago.MultiClient.Net.Packets;
using UnityEngine;

namespace PeaksOfArchipelago;

public enum RopeLocation
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

public enum LocationCheck
{
    Rope_WaltersCrag,
    Rope_WalkersPillar,
    Rope_GreatGaol,
    Rope_StHaelga,
    Rope_ExtraFirst,
    Rope_ExtraSecond,
    Rope_ExtraThird,
    Rope_ExtraFourth,
    Rope_Extra5,
    Rope_Extra6,
    Rope_Extra7,
    Rope_Extra8,
    Rope_Extra9,
    Rope_Extra10,
    Rope_Extra11,
    Rope_Extra12,
}


class Utils
{
    private static int ropeOffset = 0;
    public static RopeLocation GetRopeLocation(RopeCollectable ropeCollectable)
    {
        if (!ropeCollectable.isSingleRope)
        {
            return (RopeLocation)ropeCollectable.extraRopeNumber;
        }
        if (ropeCollectable.isWaltersCrag) return RopeLocation.WaltersCrag;
        if (ropeCollectable.isWalkersPillar) return RopeLocation.WalkersPillar;
        if (ropeCollectable.isGreatGaol) return RopeLocation.GreatGaol;
        if (ropeCollectable.isStHaelga) return RopeLocation.StHaelga;
        return RopeLocation.None;
    }

    public static LocationCheck GetLocationFromRope(RopeCollectable instance)
    {
        RopeLocation rope = GetRopeLocation(instance);
        return (LocationCheck)(rope + ropeOffset);
    }
}