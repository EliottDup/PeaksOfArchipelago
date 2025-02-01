using System;
using System.Collections.Generic;

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
    Extra12
}

public enum Artefacts
{
    Hat1,
    Hat2,
    Helmet,
    Shoe,
    Shovel,
    Sleepingbag,
    Backpack,
    Coffebox_1,
    Coffebox_2,
    Chalkbox_1,
    Chalkbox_2,
    ClimberStatue1,
    ClimberStatue2,
    ClimberStatue3,
    Photograph_1,
    Photograph_2,
    Photograph_3,
    Photograph_4,
    PhotographFrame,
    Belt,
    Pants,
    Underwear,
    ClimberStatue0,
}

public enum Peaks
{

}

class CheckList<T> where T : struct, Enum   // I would use enum flags, but they be a bit funny sometimes so no.
{
    private readonly bool[] checks;

    public CheckList()
    {
        checks = new bool[Enum.GetNames(typeof(T)).Length];
    }

    public void SetCheck(T value, bool check)
    {
        checks[GetIndex(value)] = check;
    }

    public bool IsChecked(T value)
    {
        return checks[GetIndex(value)];
    }

    public T[] GetUncheckedValues()
    {
        T[] enumValues = (T[])Enum.GetValues(typeof(T));
        List<T> values = [];
        for (int i = 0; i < checks.Length; i++)
        {
            if (!checks[i])
            {
                values.Add(enumValues[i]);
            }
        }
        return [.. values];
    }

    public T[] GetCheckedValues()
    {
        T[] enumValues = (T[])Enum.GetValues(typeof(T));
        List<T> values = [];
        for (int i = 0; i < checks.Length; i++)
        {
            if (checks[i])
            {
                values.Add(enumValues[i]);
            }
        }
        return [.. values];
    }

    public bool[] GetChecks()
    {
        return checks;
    }

    private int GetIndex(T value)
    {
        T[] values = (T[])Enum.GetValues(typeof(T));
        return Array.IndexOf(values, value);
    }

}

public static class Utils
{
    private static int peakOffset = 0;
    private static int ropeOffset = Enum.GetValues(typeof(Peaks)).Length;
    private static int artefactOffset = ropeOffset + Enum.GetValues(typeof(Ropes)).Length;
    public static Ropes GetRopeFromCollectable(RopeCollectable ropeCollectable)
    {
        if (!ropeCollectable.isSingleRope)
        {
            return (Ropes)ropeCollectable.extraRopeNumber;
        }
        if (ropeCollectable.isWaltersCrag) return Ropes.WaltersCrag;
        if (ropeCollectable.isWalkersPillar) return Ropes.WalkersPillar;
        if (ropeCollectable.isGreatGaol) return Ropes.GreatGaol;
        if (ropeCollectable.isStHaelga) return Ropes.StHaelga;
        throw new Exception("unknown rope");
    }

    public static int GetLocationFromRope(Ropes rope)
    {
        return (int)rope + ropeOffset;
    }

    public static Artefacts GetArtefactFromCollectable(ArtefactOnPeak artefactOnPeak)
    {
        int v = (int)artefactOnPeak.peakArtefact - 1;
        return Artefacts.Hat1;
    }

    public static int GetLocationFromArtefact(Artefacts artefact)
    {
        return (int)artefact + artefactOffset;
    }
}