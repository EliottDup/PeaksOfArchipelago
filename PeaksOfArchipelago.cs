using BepInEx;
using POKModManager;

using UnityEngine.Events;
using UnityEngine;
using BepInEx.Logging;

using Steamworks;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System;
using System.Reflection;

namespace PeaksOfArchipelago;

[BepInPlugin(ModInfo.MOD_GUID, ModInfo.MOD_NAME, ModInfo.MOD_VERSION)]
[BepInDependency("Data.POKManager")]
public class PeaksOfArchipelago : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;

        Logger.LogInfo($"Plugin {ModInfo.MOD_GUID} is loaded!");

        Harmony h = new(ModInfo.MOD_GUID + "_Paths");

        MethodInfo method = AccessTools.PropertyGetter(AccessTools.TypeByName("POKModManager.Paths"), "GameFolder");
        MethodInfo prefix = typeof(Patch_Paths).GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public);
        h.Patch(method, prefix: new HarmonyMethod(prefix));

        new POKManager(true);   //Re-initialise the ModManager with correct paths
        // A "funny" side effect is that if the mod is installed manually instead of using r2modman or thunderstore, the main menu gains a second "Mods" button
    }

    private void Start()
    {
        UIHandler.poyFont = GameObject.Find("Options")?.GetComponentInChildren<Text>()?.font;
        POKManager.RegisterMod(new PeaksOfArchipelagoMod(Logger), ModInfo.MOD_NAME, ModInfo.MOD_VERSION, ModInfo.MOD_DESC, UseEditableAttributeOnly: true);
    }
}

public class Patch_Paths
{
    public static bool Prefix(ref string __result)
    {
        __result = System.IO.Directory.GetParent(BepInEx.Paths.BepInExRootPath).ToString(); // this fixes a bug with the mod manager, where some paths would not always be correct and crash the mod
        return false;
    }
}

public class PeaksOfArchipelagoMod : ModClass
{
    [Editable] public string Hostname { get; set; } = "archipelago.gg";
    [Editable] public string Port { get; set; } = "123456";
    [Editable] public string SlotName { get; set; } = "";
    [Editable] public string Password { get; set; } = "";
    [Editable] public bool AutoConnect { get; set; } = false;
    [Editable] public UnityEvent Connect { get; set; } = new UnityEvent();

    private bool justConnected = false;

    readonly Harmony harmony = new(ModInfo.MOD_GUID);
    PlayerData playerData;

    private static POASession session;
    public static ManualLogSource logger;
    public static TimeAttackDefaultData timeAttackDefaultData;

    public PeaksOfArchipelagoMod(ManualLogSource logger)
    {
        PeaksOfArchipelagoMod.logger = logger;
    }

    public override void OnEnabled()    // Runs when the mod is enabled, and completely at the start
    {
        playerData = new PlayerData();
        session = new POASession(playerData);
        POASession.logger = logger;
        UnityUtils.logger = logger;
        UIHandler.logger = logger;
        Traps.logger = logger;
        harmony.PatchAll();

        Connect.AddListener(OnConnect);
        if (AutoConnect)
        {
            OnConnect();
        }
        logger.LogInfo("Enabled Peaks of Archipelago!");
    }

    public override void OnDisabled()
    {
        harmony.UnpatchSelf();
    }

    public override void Start()        // Runs every time a new scene is launched (whenever "Start" would be ran on a normal unity object)
    {
        // DO NOT PUT GAMEMANAGER.SAVE HERE IT FUCKED UP MY SAVES LOL
        if (session.currentScene == "TitleScreen" && justConnected)
        {
            justConnected = false;
            logger.LogInfo("entering cabin from main menu, resetting ropes, coffee, chalk and bird uses to zero");
            GameManager.control.ropesCollected = 0;
            GameManager.control.extraCoffeeUses = 0;
            GameManager.control.extraChalkUses = 0;
            GameManager.control.extraBirdSeedUses = 0;
        }
        session.currentScene = SceneManager.GetActiveScene().name;
        logger.LogInfo("Entering Scene " + session.currentScene);
        if (session.currentScene == "Cabin")
        {
            session.fundamentalsBook = GameObject.Find("PEAKJOURNAL");
        }
        logger.LogInfo($"You have {GameManager.control.extraBirdSeedUses} birdseeds");


        GameObject go = GameObject.Find("PeaksOfArchipelagoScriptHolder") ?? new GameObject("PeaksOfArchipelagoScriptHolder");
        go.transform.SetParent(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()[0].transform);
        go.AddComponent<UIHandler>();
    }

    private string GetUri()
    {
        return Hostname + ":" + Port;
    }

    private void OnConnect()
    {
        justConnected = true;
        _ = session.Connect(GetUri(), SlotName, Password);
    }

    //------------------------- Harmony Patches -------------------------

    [HarmonyPatch(typeof(StamperPeakSummit), "StampJournal")]
    public class PeakSummitedPatch
    {
        static void Prefix(StamperPeakSummit __instance)
        {
            Peaks peak = Utils.GetPeakFromCollectable(__instance);
            session.NotifyCollection(Utils.PeakToId(peak));
            session.CompletePeakCheck(peak);
            if (GameObject.FindGameObjectWithTag("Player").GetComponent<RopeAnchor>().ropesPlacedDuringMap == 0 && (int)peak >= 30)
            {
                session.NotifyCollection(Utils.FSPeakToId(peak));
                session.CompleteFSPeakCheck(peak);

            }
        }
    }

    [HarmonyPatch(typeof(ArtefactOnPeak), "PickUpItem")]
    public class ArtefactOnPeakPatch
    {
        static void Postfix(ArtefactOnPeak __instance)
        {
            Artefacts artefact = Utils.GetArtefactFromCollectable(__instance);
            session.NotifyCollection(Utils.ArtefactToId(artefact));
            session.CompleteArtefactCheck(artefact);
        }
    }

    [HarmonyPatch(typeof(RopeCollectable), "PickUpRope")]
    public class RopeCollectablePatch
    {
        static void Prefix(RopeCollectable __instance)
        {
            Ropes rope = Utils.GetRopeFromCollectable(__instance);
            session.NotifyCollection(Utils.RopeToId(rope));
            session.CompleteRopeCheck(rope);
        }

        static void Postfix(ref IEnumerator __result, RopeCollectable __instance)
        {
            __result = MyWrapper(__result, __instance);
        }

        static IEnumerator MyWrapper(IEnumerator original, RopeCollectable __instance)
        {
            while (original.MoveNext())
            {
                yield return original.Current;
            }

            UnityUtils.UndoRopeProgress(__instance);
            yield return null;
        }
    }

    [HarmonyPatch(typeof(BirdSeedCollectable), "PickUpBirdSeed")]
    public class BirdSeedCollectablePatch
    {
        static void Prefix(BirdSeedCollectable __instance)
        {
            BirdSeeds seed = Utils.GetSeedFromCollectable(__instance);
            session.NotifyCollection(Utils.BirdSeedToId(seed));
            session.CompleteSeedCheck(seed);
            GameManager.control.extraBirdSeedUses--;
        }

        static void Postfix(ref IEnumerator __result, RopeCollectable __instance)
        {
            __result = MyWrapper(__result, __instance);
        }

        static IEnumerator MyWrapper(IEnumerator original, RopeCollectable __instance)
        {
            while (original.MoveNext())
            {
                yield return original.Current;
            }
            yield return null;
        }
    }

    [HarmonyPatch(typeof(TimeAttack), "SetBestTime")]
    public class SetBestTimePatch
    {
        public static void Prefix(TimeAttack __instance)
        {
            if (!__instance.summitStamper.isCategory4)
            {
                Peaks peak = Utils.GetPeakFromCollectable(__instance.summitStamper);
                if (__instance.timer < timeAttackDefaultData.times[(int)peak])
                {
                    session.NotifyCollection((long)peak + Utils.timePBPeakOffset);
                    session.CompleteTimePBCheck(peak);

                }
                if (__instance.holdsMade < timeAttackDefaultData.holds[(int)peak])
                {
                    session.NotifyCollection((long)peak + Utils.holdPBPeakOffset);
                    session.CompleteHoldsPBCheck(peak);

                }
                if (__instance.ropesUsed <= timeAttackDefaultData.ropes[(int)peak])
                {
                    session.NotifyCollection((long)peak + Utils.ropePBPeakOffset);
                    session.CompleteRopesPBCheck(peak);
                }
            }
        }
    }

    [HarmonyPatch(typeof(TimeAttackSetter), "SetDefaults")]
    public class SetDefaultTimes
    {
        public static void Postfix(TimeAttackSetter __instance)
        {
            if (timeAttackDefaultData.holds == null)
            {
                timeAttackDefaultData.times = [.. __instance.category1_defaultTimes, .. __instance.category2_defaultTimes, .. __instance.category3_defaultTimes];
                timeAttackDefaultData.holds = [.. __instance.category1_defaultHolds, .. __instance.category2_defaultHolds, .. __instance.category3_defaultHolds];
                timeAttackDefaultData.ropes = [.. __instance.category1_defaultRopes, .. __instance.category2_defaultRopes, .. __instance.category3_defaultRopes];
            }
        }
    }

    [HarmonyPatch(typeof(FallingEvent), "FellToDeath")]
    public class FellToDeathPatch
    {
        static void Postfix()
        {
            session.HandleDeath();
        }
    }

    // [HarmonyPatch(typeof(IntermediateJournal), "JournalPageUpdate")]
    // public class IJournalPageUpdatePatch
    // {
    //     public static void Postfix(IntermediateJournal __instance)
    //     {
    //         logger.LogInfo($"IntermediateJournal {__instance.name} found");
    //         __instance.leftPageCol.enabled = false;
    //     }
    // }

    // [HarmonyPatch(typeof(AdvancedJournal), "JournalPageUpdate")]
    // public class AJournalPageUpdatePatch
    // {
    //     public static void Postfix(AdvancedJournal __instance)
    //     {
    //         logger.LogInfo($"AdvancedJournal {__instance.name} found");
    //         __instance.leftPageCol.enabled = false;
    //     }
    // }

    // [HarmonyPatch(typeof(PeakJournal), "Update")]
    // public class JournalUpdatePatch
    // {
    //     static List<int> disabledPeaks = new List<int> { 0, 5, 7, 8 };
    //     public static void Postfix(PeakJournal __instance)
    //     {
    //         if (disabledPeaks.Contains(__instance.currentPage))
    //         {
    //             __instance.rightPageCol.enabled = false;
    //         }
    //         if (disabledPeaks.Contains(__instance.currentPage - 1))
    //         {
    //             __instance.leftPageCol.enabled = false;
    //         }
    //     }
    // }

    [HarmonyPatch(typeof(ResetPosition), "FadeToBlack")]
    public class ResetPositionPatch
    {
        static void Prefix(ResetPosition __instance)
        {
            if (!__instance.isSea)
            {
                session.HandleDeath();
                //player fell on rocks and therefore must die
            }
            // else they fell in the water which is fine :)
        }
    }

    [HarmonyPatch(typeof(ArtefactLoaderCabin), "LoadArtefacts")]
    public class ArtefactLoaderPatch
    {
        static CheckList<Artefacts> savestate = new();
        public static void Prefix(ArtefactLoaderCabin __instance)
        {
            foreach (Artefacts artefact in Enum.GetValues(typeof(Artefacts)))
            {
                savestate.SetCheck(artefact, UnityUtils.GetGameManagerArtefactCollected(artefact));
                UnityUtils.SetGameManagerArtefactCollected(artefact, session.playerData.items.artefacts.IsChecked(artefact));
                if (session.playerData.items.artefacts.IsChecked(artefact))
                {
                    UnityUtils.SetGameManagerArtefactDirty(artefact, false);
                }
            }
        }

        public static void Postfix()
        {
            foreach (Artefacts artefact in Enum.GetValues(typeof(Artefacts)))
            {
                UnityUtils.SetGameManagerArtefactCollected(artefact, savestate.IsChecked(artefact));    // reset gamemanager to default state
                if (UnityUtils.GetGameManagerArtefactCollected(artefact) != savestate.IsChecked(artefact))
                {
                    logger.LogWarning($"Error: {artefact} should be {savestate.IsChecked(artefact)} but is {!savestate.IsChecked(artefact)}!");
                }
            }
            GameManager.control.Save();
        }
    }

    [HarmonyPatch(typeof(NPCEvents), "CheckProgress")]
    public class CheckProgressPatch
    {
        static bool usingPipe;

        public static void Prefix(NPCEvents __instance)
        {
            usingPipe = GameManager.control.isUsingPipe;
            ItemEventsPatch.isCustomEvent = false;
            session.CheckWin();
            session.UpdateReceivedItems();
            if (session.uncollectedItems.Count != 0 && !__instance.runningEvent && session.currentScene != "TitleScreen")   //player has received new items
            {
                logger.LogInfo("starting custom event");
                ItemEventsPatch.isCustomEvent = true;
                __instance.eventName = "AllArtefacts";
                __instance.runningEvent = true;
                // __instance.StartCoroutine("GlowDoorEvent");
                __instance.npcParcelDeliverySystem.StartCoroutine("FadeScreenAndStartUnpackEvent");
            }
            else if (session.finished && !session.seenFinishedCutScene) // player finished the game and won!
            {
                ItemEventsPatch.isFinishEvent = true;
                session.seenFinishedCutScene = true;
                __instance.eventName = "CompleteGame_Base";
                __instance.StartCoroutine("GlowDoorEvent");
            }
        }

        public static void Postfix(NPCEvents __instance)
        {
            session.SetFundamentalsBookActive(session.playerData.items.books.IsChecked(Books.Fundamentals));
            GameManager.control.monocular = session.playerData.items.monocular;
            if (!GameManager.control.monocular)
            {
                __instance.StopCoroutine("MissedMonocularTooltip");
            }

            if (__instance.runningEvent)
            {
                switch (__instance.eventName)
                {
                    case "Rope":
                    case "RopesUpgrade":
                    case "ArtefactMap":
                    case "Pocketwatch":
                    case "Crampons":
                    case "CramponsUpgrade":
                    case "Chalkbag":
                    case "TimeAttack_Event1":
                    case "Category_2":
                    case "Category_3":
                    case "Category_4":
                    case "Phonograph":
                        {
                            __instance.runningEvent = false;
                            logger.LogInfo("Blocking event: " + __instance.eventName);
                            __instance.StopCoroutine("GlowDoorEvent");
                            break;
                        }
                    case "AllArtefacts":
                        {
                            if (!ItemEventsPatch.isCustomEvent)
                            {
                                __instance.runningEvent = false;
                                logger.LogInfo("Blocking event: " + __instance.eventName);
                                __instance.StopCoroutine("GlowDoorEvent");
                            }
                            break;
                        }

                }
            }


            if (session.playerData.items.pipe) //reset pipe if necessary
            {
                GameManager.control.smokingpipe = true;
                GameManager.control.isUsingPipe = usingPipe;
                __instance.cabinPipe.SetActive(true);
            }
        }
    }

    [HarmonyPatch(typeof(NPCSystem), "GivePlayerMonocular")]
    public class GivePlayerMonocularPatch
    {
        public static void Postfix()
        {
            GameManager.control.monocular = session.playerData.items.monocular;
            GameManager.control.Save();
        }
    }

    [HarmonyPatch(typeof(NPCSystem), "GivePlayerCoffee")]
    public class GivePlayerCoffeePatch
    {
        public static void Postfix()
        {
            GameManager.control.coffee = session.playerData.items.coffee;
            GameManager.control.Save();
        }
    }

    [HarmonyPatch(typeof(NPCSystem), "GivePlayerRope")]
    public class GivePlayerRopePatch
    {
        public static void Postfix(NPCSystem __instance)
        {
            Ropes rope = (Ropes)(-1);
            if (__instance.isStHaelga)
            {
                rope = Ropes.StHaelga;
            }
            if (__instance.isGreatGaol)
            {
                rope = Ropes.GreatGaol;
            }
            GameManager.control.ropesCollected--;
            GameObject.FindGameObjectWithTag("Player").GetComponent<RopeAnchor>().anchorsInBackpack--;

            session.NotifyCollection(Utils.RopeToId(rope));
            session.CompleteRopeCheck(rope);
        }
    }

    [HarmonyPatch(typeof(NPC_Climber), "GivePlayerRope")]
    public class ClimberGivePlayerRopePatch
    {
        public static void Postfix(NPC_Climber __instance)
        {
            Ropes rope = (Ropes)(-1);
            if (__instance.isWaltersCrag)
            {
                rope = Ropes.WaltersCrag;
            }
            if (__instance.isWalkersPillar)
            {
                rope = Ropes.WalkersPillar;
            }

            GameManager.control.ropesCollected--;
            GameObject.FindGameObjectWithTag("Player").GetComponent<RopeAnchor>().anchorsInBackpack--;

            session.NotifyCollection(Utils.RopeToId(rope));
            session.CompleteRopeCheck(rope);
        }
    }

    [HarmonyPatch(typeof(NPCEvents), "ItemEvents")]
    public class ItemEventsPatch
    {
        public static bool isCustomEvent = false;
        public static bool isFinishEvent = false;
        static bool hasAllArtefacts = false;
        static string tempText = "";
        static bool usingPipe;
        static Text textElement;
        static void Prefix(NPCEvents __instance)
        {
            if (!isCustomEvent) return;
            usingPipe = GameManager.control.isUsingPipe;
            hasAllArtefacts = GameManager.control.allArtefactsUnlocked;
            Text text = __instance.allArtefactsInfo.GetComponentInChildren<Text>();
            tempText = text.text;
            textElement = text;

            string msg = "You got: ";

            session.UpdateReceivedItems();

            for (int i = 0; i < session.uncollectedItems.Count; i++)
            {
                SimpleItemInfo info = session.uncollectedItems[i];
                if (info.itemName == "Trap") continue;
                msg += info.itemName;
                if (i != session.uncollectedItems.Count - 1)
                {
                    msg += ", ";
                }
            }
            text.text = msg;
        }

        static void Postfix(ref IEnumerator __result, NPCEvents __instance)
        {
            __result = MyWrapper(__result, __instance);
        }

        static IEnumerator MyWrapper(IEnumerator original, NPCEvents __instance)
        {
            while (original.MoveNext())
            {
                yield return original.Current;
            }

            if (isCustomEvent)
            {
                isCustomEvent = false;  //reset to what they were before
                GameManager.control.allArtefactsUnlocked = hasAllArtefacts;
                GameManager.control.ropesCollected -= 5;
                GameManager.control.extraCoffeeUses -= 999999999;
                GameManager.control.extraChalkUses -= 999999999;
                textElement.text = tempText;

                session.UpdateReceivedItems();
                List<SimpleItemInfo> items = session.uncollectedItems;
                session.uncollectedItems = [];
                foreach (SimpleItemInfo item in items)
                {
                    session.UnlockById(item.id);
                }
                foreach (RopeCabinDescription ropeCabinDescription in GameObject.FindObjectsOfType<RopeCabinDescription>())
                    ropeCabinDescription.UpdateCoffeeChalk();
                GameObject.FindObjectOfType<ArtefactLoaderCabin>()?.LoadArtefacts();
                GameObject.FindObjectOfType<RopeCabinDescription>()?.CheckCabinItems();
            }
            if (session.playerData.items.pipe) //reset pipe if necessary
            {
                GameManager.control.smokingpipe = true;
                GameManager.control.isUsingPipe = usingPipe;
                __instance.cabinPipe.SetActive(true);
            }
            yield return null;
        }
    }

    [HarmonyPatch(typeof(Pipe), "CheckLoad")]
    public class PipeCheckPatch
    {
        static bool isUsingPipe;
        static void Prefix()
        {
            isUsingPipe = GameManager.control.isUsingPipe;
        }

        static void Postfix(Pipe __instance)
        {
            if (session.playerData.items.pipe) //reset pipe if necessary
            {
                GameManager.control.isUsingPipe = isUsingPipe;
                GameManager.control.smokingpipe = true;
                __instance.gameObject.SetActive(isUsingPipe);
            }
        }
    }

    [HarmonyPatch(typeof(PipeCabin), "CheckLoad")]
    public class PipeCheck2Patch
    {
        static bool isUsingPipe;
        static void Prefix()
        {
            isUsingPipe = GameManager.control.isUsingPipe;
        }

        static void Postfix(PipeCabin __instance)
        {
            if (session.playerData.items.pipe) //reset pipe if necessary
            {
                GameManager.control.isUsingPipe = isUsingPipe;
                GameManager.control.smokingpipe = true;
                __instance.gameObject.SetActive(true);
                __instance.playerPipe.SetActive(isUsingPipe);
            }
        }
    }

    [HarmonyPatch(typeof(EnterPeakScene), "Awake")]
    public class EnterPeakScenePatch
    {
        static void Prefix(EnterPeakScene __instance)
        {
            string peak = GameObject.FindGameObjectWithTag("SummitBox").GetComponent<StamperPeakSummit>().peakNames.ToString();
            logger.LogInfo("Entering peak: " + peak);

            GameObject go = GameObject.Find("PeaksOfArchipelagoScriptHolder") ?? new GameObject("PeaksOfArchipelagoScriptHolder");
            Traps t = go.AddComponent<Traps>();
            logger.LogInfo("Adding traps instance");
            Traps.instance = t;
            Traps.playerData = session.playerData;
        }
    }

    [HarmonyPatch(typeof(OilLamp), "OilLampLightUp")]
    public class OilLampLightUpPatch
    {
        static void Postfix(OilLamp __instance, ref IEnumerator __result)
        {
            if (session.playerData.items.lamp)
            {
                return;
            }
            __result = Wrapper(__instance);
        }

        static IEnumerator Wrapper(OilLamp __instance)
        {
            __instance.oilLampLight.intensity = 0;
            yield return null;
        }
    }

    [HarmonyPatch(typeof(Rewired.Player), "GetButton", [typeof(string)])]
    public class HandPatch
    {
        static bool Prefix(ref bool __result, string actionName)
        {
            if (actionName == "Arm Right" && !session.playerData.items.rightHand) return false;
            if (actionName == "Arm Left" && !session.playerData.items.leftHand) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(StatsAndAchievements), "SetAchievement")]
    [HarmonyPatch(typeof(StatsAndAchievements), "SetStatFloat")]
    [HarmonyPatch(typeof(StatsAndAchievements), "SetStatInt")]
    [HarmonyPatch(typeof(StatsAndAchievements), "ResetStatsAndAchievements")]
    public class SetAchievementPatch
    {
        static bool Prefix()
        {
            logger.LogInfo("Game wants to do something with achievements, but I say no");
            return false;
        }
    }
    [HarmonyPatch(typeof(SteamUserStats), "SetAchievement")]
    [HarmonyPatch(typeof(SteamUserStats), "StoreStats")]
    public class SetSteamAchievementPatch
    {
        static bool Prefix(ref bool __result)
        {
            logger.LogInfo("Game wants to do something with achievements, but I say no");
            __result = true;
            return false;
        }
    }

    [HarmonyPatch]
    public class CollectionTextPatch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(BirdSeedCollectedText), "InitiateCollectedTextSingle");
            yield return AccessTools.Method(typeof(RopeCollectedText), "InitiateCollectedTextSingle");
            yield return AccessTools.Method(typeof(BirdSeedCollectedText), "InitiateCollectedText");
            yield return AccessTools.Method(typeof(RopeCollectedText), "InitiateCollectedText");
            yield return AccessTools.Method(typeof(ArtefactCollectedText), "InitiateCollectedText");
        }

        static bool Prefix()
        {
            logger.LogInfo("Blocking collection Text");
            return false;
        }
    }

    //------------------------- Harmony Transpilers -------------------------
    //! THESE ARE THE THINGS THAT MIGHT BREAK WHEN UPDATING THE GAME!!

    [HarmonyPatch(typeof(RopeAnchor), "Start")]
    public class StartTranspiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i == 76 || i == 77 || i == 78)
                {
                    // logger.LogInfo("Disabled line " + i + " : " + codes[i].ToString());
                    continue;
                }
                yield return codes[i];
            }
        }
    }

    [HarmonyPatch(typeof(RopeAnchor), "DetachThenAttachToNew", MethodType.Enumerator)]
    public class DetachThenAttachToNewTranspiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i == 120 || i == 121 || i == 122 || i == 123)
                {
                    // logger.LogInfo("Disabled line " + i + " : " + codes[i].ToString());
                    continue;
                }
                yield return codes[i];
            }
        }
    }

    [HarmonyPatch(typeof(RopeAnchor), "PullOutAnchor", MethodType.Enumerator)]
    public class PullOutAnchorTranspiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i == 79 || i == 80 || i == 81 || i == 82)
                {
                    // logger.LogInfo("Disabled line " + i + " : " + codes[i].ToString());
                    continue;
                }
                yield return codes[i];
            }
        }
    }

    [HarmonyPatch(typeof(RopeCollectable), "CheckRope")]
    public class CheckRopeTranspiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i == 267 || i == 268 || i == 269 || i == 270)
                {
                    // logger.LogInfo("Disabled line " + i + " : " + codes[i].ToString());
                    continue;   // this stops ropes on peaks from disappearing if you have 42+ ropes
                }
                yield return codes[i];
            }
        }
    }

    [HarmonyPatch(typeof(BirdSeedCollectable), "CheckBirdSeed")]
    public class CheckBirdSeedTranspler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i == 83 || i == 84 || i == 85 || i == 86)
                {
                    // logger.LogInfo("Disabled line " + i + " : " + codes[i].ToString());
                    continue;   // this stops bird seeds on peaks from disappearing if you have 5+ seeds
                }
                yield return codes[i];
            }
        }
    }
}
