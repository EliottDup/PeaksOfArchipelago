using Archipelago.MultiClient.Net.Models;
using BepInEx.Logging;
using PeaksOfArchipelago.GameData;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeaksOfArchipelago.GameData
{
    internal abstract class ArchipelagoItem
    {
        public readonly string name;
        public ArchipelagoItem(ItemInfo info)
        {
            name = info.ItemName;
        }

        public static ArchipelagoItem Create(ItemInfo item)
        {
            ItemTypes.Types t = ItemTypes.GetItemType(item.ItemId);
            switch (t)
            {
                case ItemTypes.Types.Peak:
                    return new PeakItem(item);
                case ItemTypes.Types.Rope:
                    return new RopeItem(item);
                case ItemTypes.Types.Artefact:
                    return new ArtefactItem(item);
                case ItemTypes.Types.Book:
                    return new BookItem(item);
                case ItemTypes.Types.BirdSeed:
                    return new BirdSeedItem(item);
                case ItemTypes.Types.Tool:
                    return new ToolItem(item);
                case ItemTypes.Types.ExtraItem:
                    return new ExtraItem(item);
                default:
                    throw new NotImplementedException($"Item type {t} Should not exist");
            }
        }

        public abstract void Unlock(ISlotData slotData);
    }

    internal class ExtraItem : ArchipelagoItem
    {
        public ExtraItems item;
        public ExtraItem(ItemInfo info) : base(info)
        {
            item = ItemIDs.GetExtraItemFromId(info.ItemId);
        }

        public override void Unlock(ISlotData slotData)
        {
            switch (item)
            {
                case ExtraItems.ExtraRope:
                    GameManager.control.ropesCollected++;
                    break;
                case ExtraItems.ExtraChalk:
                    GameManager.control.extraChalkUses++;
                    break;
                case ExtraItems.ExtraCoffee:
                    GameManager.control.extraCoffeeUses++;
                    break;
                case ExtraItems.ExtraSeed:
                    GameManager.control.extraBirdSeedUses++;
                    break;
                case ExtraItems.Trap:
                    //TODO: implement traps :))))
                    break;
            }
            GameManager.control.Save();
        }
    }

    internal class BirdSeedItem : ArchipelagoItem
    {
        BirdSeeds birdSeed;
        public BirdSeedItem(ItemInfo info) : base(info)
        {
            birdSeed = ItemIDs.GetBirdSeedFromId(info.ItemId);
        }

        public override void Unlock(ISlotData slotData)
        {
            slotData.UnlockBirdSeed(birdSeed);
            GameManager.control.extraBirdSeedUses++;
            GameManager.control.Save();
        }
    }

    internal class BookItem : ArchipelagoItem
    {
        Books book;
        public BookItem(ItemInfo info) : base(info)
        {
            book = ItemIDs.GetBookFromId(info.ItemId);
        }

        public override void Unlock(ISlotData slotData)
        {
            slotData.UnlockBook(book);
        }
    }

    internal class ArtefactItem : ArchipelagoItem
    {
        Artefacts artefact;
        public ArtefactItem(ItemInfo info) : base(info)
        {
            artefact = ItemIDs.GetArtefactFromID(info.ItemId);
        }

        public override void Unlock(ISlotData slotData)
        {
            slotData.UnlockArtefact(artefact);
            switch (artefact)
            {
                case Artefacts.Coffebox_1:
                case Artefacts.Coffebox_2:
                    GameManager.control.extraCoffeeUses += 2;
                    break;
                case Artefacts.Chalkbox_1:
                case Artefacts.Chalkbox_2:
                    GameManager.control.extraChalkUses += 2;
                    break;
            }
            GameManager.control.Save();
        }
    }

    internal class ToolItem : ArchipelagoItem
    {
        Tools tool;
        public ToolItem(ItemInfo info) : base(info)
        {
            tool = ItemIDs.GetToolFromID(info.ItemId);
        }

        public override void Unlock(ISlotData slotData)
        {
            slotData.UnlockTool(tool);
            switch (tool)
            {
                case Tools.Pipe:
                    GameManager.control.smokingpipe = true;
                    break;
                case Tools.RopeLengthUpgrade:
                    GameManager.control.ropesUpgrade = true;
                    break;
                case Tools.Barometer:
                    GameManager.control.barometer = true;
                    GameManager.control.artefactMap = true;
                    break;
                case Tools.ProgressiveCrampons:
                    if (slotData.cramponLevel == 0)
                    {
                        GameManager.control.crampons = true;
                    }
                    else if (slotData.cramponLevel == 1)
                    {
                        GameManager.control.cramponsUpgrade = true;
                    }
                    break;
                case Tools.Monocular:
                    GameManager.control.monocular = true;
                    break;
                case Tools.Phonograph:
                    GameManager.control.phonograph = true;
                    break;
                case Tools.Chalkbag:
                    GameManager.control.chalkBag = true;
                    break;
                case Tools.Rope:
                    GameManager.control.rope = true;
                    break;
                case Tools.Coffee:
                    GameManager.control.coffee = true;
                    break;
                case Tools.Lamp:
                    slotData.UnlockTool(Tools.Lamp);
                    break;
                case Tools.RightHand:
                    slotData.UnlockTool(Tools.RightHand);
                    break;
                case Tools.leftHand:
                    slotData.UnlockTool(Tools.leftHand);
                    break;
            }
            GameManager.control.Save();
            slotData.UnlockTool(tool);
        }
    }

    internal class RopeItem : ArchipelagoItem
    {
        public Ropes Rope { get; private set; }
        public RopeItem(ItemInfo info) : base(info)
        {
            Rope = ItemIDs.GetRopeFromID(info.ItemId);
        }

        public override void Unlock(ISlotData slotData)
        {
            if (slotData.ropeUnlockMode == SessionSettings.RopeUnlockMode.INSTANT)
            {
                slotData.UnlockTool(Tools.Rope);
                GameManager.control.rope = true;
            }
            slotData.UnlockRope(Rope);
            if (Rope < Ropes.ExtraFirst)
            {
                GameManager.control.ropesCollected++;
            }
            else
            {
                GameManager.control.ropesCollected++;
            }
            GameManager.control.Save();
        }
    }

    internal class PeakItem : ArchipelagoItem
    {
        public Peaks Peak { get; private set; }
        public PeakItem(ItemInfo info) : base(info)
        {
            Peak = ItemIDs.GetPeakFromID(info.ItemId);
        }

        public override void Unlock(ISlotData slotData)
        {
            slotData.UnlockPeak(Peak);
            return;
        }
    }


}
