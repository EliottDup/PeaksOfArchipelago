using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
    ClimberStatue0
}

public enum BirdSeeds
{
    ExtraSeed1,
    ExtraSeed2,
    ExtraSeed3,
    ExtraSeed4,
    ExtraSeed5
}

public enum Peaks
{
    GreenhornsTop,
    PaltryPeak,
    OldMill,
    GrayGully,
    Lighthouse,
    OldManOfSjor,
    GiantsShelf,
    EvergreensEnd,
    TheTwins,
    OldGrovesSkelf,
    LandsEnd,
    HangmansLeap,
    OldLangr,
    AldrGrotto,
    ThreeBrothers,
    WaltersCrag,
    GreatCrevice,
    OldHagger,
    UgsomeStorr,
    WutheringCrest,
    PortersBoulders,
    JotunnsThumb,
    OldSkerry,
    HamarrStone,
    GiantsNose,
    WaltersBoulder,
    SunderedSons,
    OldWealdsBoulder,
    LeaningSpire,
    Cromlech,
    WalkersPillar,
    Eldenhorn,
    GreatGaol,
    StHaelga,
    YmirsShadow,
    GreatBulwark,
    SolemnTempest
}

public enum Books
{
    Fundamentals,
    Intermediate,
    Advanced,
    Expert
}

public enum Tools
{
    Pipe,
    RopeLengthUpgrade,
    Barometer,
    ProgressiveCrampons,
    Monocular,
    Phonograph,
    Pocketwatch,
    Chalkbag,
    Rope,
    Coffee,
    Lamp,
    leftHand,
    RightHand
}

public enum ExtraItems
{
    ExtraRope,
    ExtraChalk,
    ExtraCoffee,
    ExtraSeed
}

struct SimpleItemInfo
{
    public string playerName;
    public string itemName;
    public long id;
}

class CheckList<T> where T : struct, Enum   // I would use enum flags, but they be a bit funny sometimes so no.
{
    private readonly Dictionary<T, bool> checks;

    public CheckList(bool value = false)
    {
        checks = [];
        foreach (T entry in Enum.GetValues(typeof(T)))
        {
            checks.Add(entry, value);
        }
    }

    public void SetCheck(T value, bool check)
    {
        checks[value] = check;
    }

    public bool IsChecked(T value)
    {
        return checks[value];
    }

    public T[] GetUncheckedValues()
    {
        List<T> values = [];
        foreach (T entry in Enum.GetValues(typeof(T)))
        {
            if (!checks[entry])
            {
                values.Add(entry);
            }
        }
        return [.. values];
    }

    public T[] GetCheckedValues()
    {
        List<T> values = [];
        foreach (T entry in Enum.GetValues(typeof(T)))
        {
            if (checks[entry])
            {
                values.Add(entry);
            }
        }
        return [.. values];
    }

    public Dictionary<T, bool> GetChecks()
    {
        return checks;
    }
}

public static class Utils
{
    //id offsets (surely 100 is enough)
    public const int peakOffset = 1;
    public const int ropeOffset = 1000;
    public const int artefactOffset = 2000;
    public const int bookOffset = 3000;
    public const int birdSeedOffset = 4000;
    public const int toolOffset = 5000;
    public const int extraItemOffset = 6000;

    public readonly static Dictionary<Artefacts, string> artefactToVariableName = new()
    {
        {Artefacts.Hat1, "Hat1"},
        {Artefacts.Hat2, "Hat2"},
        {Artefacts.Helmet, "Helmet"},
        {Artefacts.Shoe, "Shoe"},
        {Artefacts.Shovel, "Shovel"},
        {Artefacts.Sleepingbag, "Sleepingbag"},
        {Artefacts.Backpack, "Backpack"},
        {Artefacts.Coffebox_1, "Coffeebox1"},
        {Artefacts.Coffebox_2, "Coffeebox2"},
        {Artefacts.Chalkbox_1, "Chalkbox1"},
        {Artefacts.Chalkbox_2, "Chalkbox2"},
        {Artefacts.ClimberStatue1, "Statue1"},
        {Artefacts.ClimberStatue2, "Statue2"},
        {Artefacts.ClimberStatue3, "Statue3"},
        {Artefacts.Photograph_1, "Photograph1"},
        {Artefacts.Photograph_2, "Photograph2"},
        {Artefacts.Photograph_3, "Photograph3"},
        {Artefacts.Photograph_4, "Photograph4"},
        {Artefacts.PhotographFrame, "PhotographFrame"},
        {Artefacts.ClimberStatue0, "Statue0"}
    };

    public static Peaks GetPeakFromCollectable(StamperPeakSummit peakStamper)
    {
        int p = (int)peakStamper.peakNames - 1;
        if (p >= (int)StamperPeakSummit.PeakNames.IceWaterFallDemo)
        {
            p -= 1;
        }
        return (Peaks)(p);
    }

    public static Ropes GetRopeFromCollectable(RopeCollectable ropeCollectable)
    {
        if (!ropeCollectable.isSingleRope)
        {
            return (Ropes)(ropeCollectable.extraRopeNumber + 4);
        }
        if (ropeCollectable.isWaltersCrag) return Ropes.WaltersCrag;
        if (ropeCollectable.isWalkersPillar) return Ropes.WalkersPillar;
        if (ropeCollectable.isGreatGaol) return Ropes.GreatGaol;
        if (ropeCollectable.isStHaelga) return Ropes.StHaelga;
        throw new Exception("unknown rope");
    }

    public static Artefacts GetArtefactFromCollectable(ArtefactOnPeak artefactOnPeak)
    {
        int v = (int)artefactOnPeak.peakArtefact - 1;
        if (artefactOnPeak.peakArtefact >= ArtefactOnPeak.Artefacts.Belt) v -= 3;
        return (Artefacts)v;
    }

    //Converters Type -> ID
    public static int ArtefactToId(Artefacts artefact)
    {
        return (int)artefact + artefactOffset;
    }

    public static int RopeToId(Ropes rope)
    {
        return (int)rope + ropeOffset;
    }

    public static int PeakToId(Peaks peak)
    {
        return (int)peak + peakOffset;
    }

    public static int BookToId(Books book)
    {
        return (int)book + bookOffset;
    }

    public static int BirdSeedToId(BirdSeeds birdSeed)
    {
        return (int)birdSeed + birdSeedOffset;
    }

    public static int ToolToId(Tools tool)
    {
        return (int)tool + toolOffset;
    }

    public static int ExtraItemToId(ExtraItems extraItem)
    {
        return (int)extraItem + extraItemOffset;
    }

    public static Peaks IdtoPeak(long id)
    {
        return (Peaks)(id - peakOffset);
    }

    public static Ropes IdtoRope(long id)
    {
        return (Ropes)(id - ropeOffset);
    }

    public static Artefacts IdtoArtefact(long id)
    {
        return (Artefacts)(id - artefactOffset);
    }

    public static Books IdToBook(long id)
    {
        return (Books)(id - bookOffset);
    }

    public static BirdSeeds IdToBirdSeed(long id)
    {
        return (BirdSeeds)(id - birdSeedOffset);
    }

    public static Tools IdToTool(long id)
    {
        return (Tools)(id - toolOffset);
    }

    public static ExtraItems IdToExtraItem(long id)
    {
        return (ExtraItems)(id - extraItemOffset);
    }

    public static string GetNameById(long id)
    {
        if (id < Utils.artefactOffset)
        {
            return IdtoRope(id).ToString();
        }
        if (id < Utils.bookOffset)
        {
            return IdtoArtefact(id).ToString();
        }
        if (id < Utils.extraItemOffset)
        {
            return IdToBook(id).ToString();
        }
        return IdToExtraItem(id).ToString();
    }

    struct Location
    {
        public string name;
        public string type;
        public int id;
        public int region;
    }

    struct Item
    {
        public string name;
        public string type;
        public int id;
    }

    struct Data
    {
        public Location[] locations;
        public Item[] items;

    }

    public static string GetDataAsJson()
    {
        Data data;
        data.locations = [];
        data.items = [];
        List<Location> locations = new List<Location>();
        List<Item> items = new List<Item>();
        foreach (Peaks peak in Enum.GetValues(typeof(Peaks)))
        {
            int id = Utils.PeakToId(peak);
            string name = peak.ToString();
            string type = "Peak";

            Location location = new() { name = name, type = type, id = id, region = 0 };

            locations.Add(location);
        }

        foreach (Ropes rope in Enum.GetValues(typeof(Ropes)))
        {
            int id = Utils.RopeToId(rope);
            string name = rope.ToString();
            string type = "Rope";

            Location location = new() { name = name, type = type, id = id, region = 0 };
            Item item = new() { name = name, type = type, id = id };

            locations.Add(location);
            items.Add(item);
        }

        foreach (Artefacts artefact in Enum.GetValues(typeof(Artefacts)))
        {
            int id = Utils.ArtefactToId(artefact);
            string name = artefact.ToString();
            string type = "Artefact";

            Location location = new() { name = name, type = type, id = id, region = 0 };
            Item item = new() { name = name, type = type, id = id };

            locations.Add(location);
            items.Add(item);
        }

        foreach (Books book in Enum.GetValues(typeof(Books)))
        {
            int id = Utils.BookToId(book);
            string name = book.ToString();
            string type = "Book";

            Item item = new() { name = name, type = type, id = id };

            items.Add(item);
        }

        foreach (BirdSeeds birdSeed in Enum.GetValues(typeof(BirdSeeds)))
        {
            int id = Utils.BirdSeedToId(birdSeed);
            string name = birdSeed.ToString();
            string type = "Bird Seed";

            Location location = new() { name = name, type = type, id = id, region = 0 };
            Item item = new() { name = name, type = type, id = id };

            locations.Add(location);
            items.Add(item);
        }

        foreach (Tools tool in Enum.GetValues(typeof(Tools)))
        {
            int id = Utils.ToolToId(tool);
            string name = tool.ToString();
            string type = "Tool";

            Item item = new() { name = name, type = type, id = id };

            items.Add(item);
        }

        foreach (ExtraItems extraItem in Enum.GetValues(typeof(ExtraItems)))
        {
            int id = Utils.ExtraItemToId(extraItem);
            string name = extraItem.ToString();
            string type = "Extra Item";

            Item item = new() { name = name, type = type, id = id };

            items.Add(item);
        }
        data.locations = [.. locations];
        data.items = [.. items];

        string json = JsonConvert.SerializeObject(data);
        return json;
    }

    public static Type GetTypeById(long id)
    {
        if (id >= extraItemOffset) return typeof(ExtraItems);
        if (id >= toolOffset) return typeof(Tools);
        if (id >= birdSeedOffset) return typeof(BirdSeeds);
        if (id >= bookOffset) return typeof(Books);
        if (id >= artefactOffset) return typeof(Artefacts);
        if (id >= ropeOffset) return typeof(Ropes);
        if (id >= peakOffset) return typeof(Peaks);
        return null;
    }

    public static BirdSeeds GetSeedFromCollectable(BirdSeedCollectable seedCollectable)
    {
        return (BirdSeeds)seedCollectable.extraBirdSeedNumber;
    }
}