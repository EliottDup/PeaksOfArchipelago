

namespace PeaksOfArchipelago.GameData
{
    internal class Offsets
    {
        public const int PeakIDOffset = 1;
        public const int RopeIDOffset = 1000;
        public const int ArtefactIDOffset = 2000;
        public const int BookIDOffset = 3000;
        public const int BirdSeedIDOffset = 4000;
        public const int ToolIDOffset = 5000;
        public const int ExtraItemIDOffset = 6000;
        public const int FreeSoloPeakIDOffset = 7000;
        public const int TATimeIDOffset = 8000;
        public const int TARopeIDOffset = 9000;
        public const int TAHoldsIDOffset = 10000;
    }

    internal class ItemTypes
    {
        public enum Types
        {
            Peak = 1,
            Rope = 2,
            Artefact = 3,
            Book = 4,
            BirdSeed = 5,
            Tool = 6,
            ExtraItem = 7,
            FreeSoloPeak = 8,
            TATime = 9,
            TARope = 10,
            TAHolds = 11
        }
        
        public static Types GetItemType(long itemID)
        {
            if (itemID >= Offsets.TAHoldsIDOffset)
                return Types.TAHolds;
            if (itemID >= Offsets.TARopeIDOffset)
                return Types.TARope;
            if (itemID >= Offsets.TATimeIDOffset)
                return Types.TATime;
            if (itemID >= Offsets.FreeSoloPeakIDOffset)
                return Types.FreeSoloPeak;
            if (itemID >= Offsets.ExtraItemIDOffset)
                return Types.ExtraItem;
            if (itemID >= Offsets.ToolIDOffset)
                return Types.Tool;
            if (itemID >= Offsets.BirdSeedIDOffset)
                return Types.BirdSeed;
            if (itemID >= Offsets.BookIDOffset)
                return Types.Book;
            if (itemID >= Offsets.ArtefactIDOffset)
                return Types.Artefact;
            if (itemID >= Offsets.RopeIDOffset)
                return Types.Rope;
            return Types.Peak;
        }
    }

    internal class ItemIDs
    {   
        // getXFromID assumes ID is in range (otherwise you can go die in a hole)
        public static Peaks GetPeakFromID(long itemID)
        {
            return (Peaks)(itemID - Offsets.PeakIDOffset);
        }

        public static Ropes GetRopeFromID(long itemID)
        {
            return (Ropes)(itemID - Offsets.RopeIDOffset);
        }

        internal static Tools GetToolFromID(long itemId)
        {
            return (Tools)(itemId - Offsets.ToolIDOffset);
        }
        
        internal static Artefacts GetArtefactFromID(long itemId)
        {
            return (Artefacts)(itemId - Offsets.ArtefactIDOffset);
        }

        internal static Books GetBookFromId(long itemId)
        {
            return (Books)(itemId - Offsets.BookIDOffset);
        }

        internal static BirdSeeds GetBirdSeedFromId(long itemId)
        {
            return (BirdSeeds)(itemId - Offsets.BirdSeedIDOffset);
        }

        internal static ExtraItems GetExtraItemFromId(long itemId)
        {
            return (ExtraItems)(itemId - Offsets.ExtraItemIDOffset);
        }
    }

    public enum Peaks {
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
    public enum Books
    {
        Fundamentals,
        Intermediate,
        Advanced,
        Expert
    }
    public enum BirdSeeds
    {
        ExtraSeed1,
        ExtraSeed2,
        ExtraSeed3,
        ExtraSeed4,
        ExtraSeed5
    }
    public enum ExtraItems
    {
        ExtraRope,
        ExtraChalk,
        ExtraCoffee,
        ExtraSeed,
        Trap
    }

    public enum PeakStatus
    {
        Locked,
        Unlocked,
        Summited,
        Completed
    }

    public enum Cabins
    {
        Cabin,
        CabinExpert,
        CabinAlps
    }
}
