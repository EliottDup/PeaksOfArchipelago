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
        POKManager.RegisterMod(new PeaksOfArchipelagoMod(), ModInfo.MOD_NAME, ModInfo.MOD_VERSION, ModInfo.MOD_DESC, UseEditableAttributeOnly: true);
        Logger.LogInfo($"Plugin {ModInfo.MOD_GUID} is loaded2!");
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

    Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
    PlayerData playerData;

    private static POASession session;

    public override void OnEnabled()    // Runs when the mod is enabled, and completely at the start
    {
        playerData = new PlayerData();
        session = new POASession(playerData);
        harmony.PatchAll();
        Connect.AddListener(OnConnect);
        if (AutoConnect)
        {
            OnConnect();
        }
        Debug.Log("Loaded Peaks of Archipelago!");

        // Debug.Log(Utils.GetDataAsJson());
    }

    public override void OnDisabled()
    {
        harmony.UnpatchSelf();
    }

    public override void Start()        // Runs every time a new scene is launched (whenever "Start" would be ran on a normal unity object)
    {
        // DO NOT PUT GAMEMANAGER.SAVE HERE IT FUCKED UP MY SAVES LOL
        session.currentScene = SceneManager.GetActiveScene().name;
    }

    private string GetUri()
    {
        return Hostname + ":" + Port;
    }

    private void OnConnect()
    {
        _ = session.Connect(GetUri(), SlotName, Password);
    }

    //------------------------- Harmony Patches -------------------------

    [HarmonyPatch(typeof(StamperPeakSummit), "StampJournal")]
    public class PeakSummitedPatch
    {
        static void Postfix(StamperPeakSummit __instance)
        {
            Peaks peak = session.CompletePeakCheck(__instance);
        }
    }

    [HarmonyPatch(typeof(ArtefactOnPeak), "PickUpItem")]
    public class ArtefactOnPeakPatch
    {
        static void Postfix(ArtefactOnPeak __instance)
        {
            Artefacts artefact = session.CompleteArtefactCheck(__instance);
            SimpleItemInfo itemInfo = session.GetLocationDetails(Utils.ArtefactToId(artefact));
            UnityUtils.SetArtefactText("Found " + itemInfo.playerName + "'s " + itemInfo.itemName);
        }
    }

    [HarmonyPatch(typeof(RopeCollectable), "PickUpRope")]
    public class RopeCollectablePatch
    {
        static void Prefix(RopeCollectable __instance)
        {
            Ropes rope = session.CompleteRopeCheck(__instance);
            SimpleItemInfo itemInfo = session.GetLocationDetails(Utils.RopeToId(rope));
            UnityUtils.SetRopeText("Found " + itemInfo.playerName + "'s " + itemInfo.itemName);
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
            BirdSeeds seed = session.CompleteSeedCheck(__instance);
            SimpleItemInfo itemInfo = session.GetLocationDetails(Utils.BirdSeedToId(seed));
            UnityUtils.SetSeedText("Found " + itemInfo.playerName + "'s " + itemInfo.itemName);
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


    [HarmonyPatch(typeof(FallingEvent), "FellToDeath")]
    public class FellToDeathPatch
    {
        static void Postfix()
        {
            session.HandleDeath();
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
                UnityUtils.SetGameManagerArtefactDirty(artefact, false);
            }
        }

        public static void Postfix()
        {
            foreach (Artefacts artefact in Enum.GetValues(typeof(Artefacts)))
            {
                UnityUtils.SetGameManagerArtefactCollected(artefact, savestate.IsChecked(artefact));    // reset gamemanager to default state
            }
        }
    }

    [HarmonyPatch(typeof(NPCEvents), "CheckProgress")]
    public class CheckProgressPatch
    {
        public static void Postfix(NPCEvents __instance)
        {
            session.fundamentalsBook = GameObject.Find("PEAKJOURNAL");
            session.SetFundamentalsBookActive(session.playerData.items.books.IsChecked(Books.Fundamentals));
            GameManager.control.monocular = session.playerData.items.monocular;

            if (!ItemEventsPatch.isCustomEvent && __instance.runningEvent)
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
                    case "AllArtefacts":
                    case "TimeAttack_Event1":
                    case "Category_2":
                    case "Category_3":
                    case "Category_4":
                    case "Phonograph":
                        {
                            __instance.runningEvent = false;
                            Debug.Log("Blocking event: " + __instance.eventName);
                            __instance.StopCoroutine("GlowDoorEvent");
                            break;
                        }
                }
            }
            ItemEventsPatch.isCustomEvent = false;
            session.CheckWin();
            session.UpdateRecievedItems();
            if (session.uncollectedItems.Count != 0 && !__instance.runningEvent && session.currentScene != "TitleScreen")
            {
                Debug.Log("starting custom event");
                ItemEventsPatch.isCustomEvent = true;
                __instance.eventName = "AllArtefacts";
                // __instance.StartCoroutine("GlowDoorEvent");
                __instance.npcParcelDeliverySystem.StartCoroutine("FadeScreenAndStartUnpackEvent");
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

    [HarmonyPatch(typeof(NPCSystem), "GivePlayerRope")]
    public class GivePlayerRopePatch
    {
        public static void Postfix(NPCEvents __instance)
        {
            bool isStHaelga = (bool)typeof(NPCEvents).GetField("isStHaelga", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
            bool isGreatGaol = (bool)typeof(NPCEvents).GetField("isGreatGaol", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
            if (isStHaelga)
            {
                session.CompleteRopeCheck(Ropes.StHaelgaGiven);
                GameManager.control.ropesCollected--;
                GameObject.FindGameObjectWithTag("Player").GetComponent<RopeAnchor>().anchorsInBackpack--;
            }
            if (isGreatGaol)
            {
                session.CompleteRopeCheck(Ropes.StHaelgaGiven);
                GameManager.control.ropesCollected--;
                GameObject.FindGameObjectWithTag("Player").GetComponent<RopeAnchor>().anchorsInBackpack--;
            }
        }
    }

    [HarmonyPatch(typeof(NPCEvents), "ItemEvents")]
    public class ItemEventsPatch
    {
        public static bool isCustomEvent;
        static bool hasAllArtefacts = false;
        static string tempText = "";
        static Text textElement;
        static void Prefix(NPCEvents __instance)
        {
            hasAllArtefacts = GameManager.control.allArtefactsUnlocked;
            Text text = __instance.allArtefactsInfo.GetComponentInChildren<Text>();
            tempText = text.text;
            textElement = text;

            string msg = "You got: ";

            session.UpdateRecievedItems();
            SimpleItemInfo last = session.uncollectedItems.Last();
            foreach (SimpleItemInfo info in session.uncollectedItems)
            {
                msg += info.itemName;
                if (info.id != last.id || info.itemName != last.itemName || info.playerName != last.playerName)
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

                session.UpdateRecievedItems();
                List<SimpleItemInfo> items = session.uncollectedItems;
                session.uncollectedItems = [];
                foreach (SimpleItemInfo item in items)
                {
                    session.UnlockById(item.id);
                }
                foreach (RopeCabinDescription ropeCabinDescription in GameObject.FindObjectsOfType<RopeCabinDescription>())
                    ropeCabinDescription.UpdateCoffeeChalk();
                GameObject.FindObjectOfType<ArtefactLoaderCabin>()?.LoadArtefacts();
            }
            yield return null;
        }
    }

    [HarmonyPatch(typeof(EnterPeakScene), "Awake")]
    public class EnterPeakScenePatch
    {
        static void Postfix(EnterPeakScene __instance)
        {
            Debug.Log("Entering: " + GameObject.FindGameObjectWithTag("SummitBox").GetComponent<StamperPeakSummit>().peakNames.ToString());

            // Debug.Log("texts:");
            // foreach (Text text in GameObject.FindObjectsOfType<Text>())  //Leaving this in case some text is ever misbehaving
            // {
            //     if (text.gameObject.name != "txt") continue;
            //     Debug.Log("TextMesh: " + text.gameObject.name + " : " + text.text);
            //     Debug.Log(text.transform.parent.name);
            //     foreach (Transform child in text.transform.parent)
            //     {
            //         Debug.Log("    " + child.name);
            //     }
            // }
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
            Debug.Log("Game wants to do something with achievements, but we blocking that shit");
            return false;
        }
    }
    [HarmonyPatch(typeof(SteamUserStats), "SetAchievement")]
    [HarmonyPatch(typeof(SteamUserStats), "StoreStats")]
    public class SetSteamAchievementPatch
    {
        static bool Prefix(ref bool __result)
        {
            Debug.Log("Game wants to do something with achievements, but we blocking that shit");
            __result = true;
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
                    // Debug.Log("Disabled line " + i + " : " + codes[i].ToString());
                    continue;
                }
                yield return codes[i];
            }
        }
    }


    // [HarmonyPatch(typeof(NPCEvents), "ItemEvents", MethodType.Enumerator)]
    // public class ItemEventsTranspiler
    // {
    //     static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //     {
    //         CodeMatcher codeMatcher = new CodeMatcher(instructions);
    //         codeMatcher.Start().MatchForward(false,
    //             new CodeMatch(OpCodes.Br_S, 445)
    //         ).Repeat(
    //             matcher =>
    //                 matcher.;
    //         );
    //         // .MatchForward(false,

    //         //     new CodeMatch(OpCodes.Ldloc_1),
    //         //     new CodeMatch(i => i.opcode == OpCodes.Ldloc_1)
    //         // );
    //     }
    // }

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
                    // Debug.Log("Disabled line " + i + " : " + codes[i].ToString());
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
                    // Debug.Log("Disabled line " + i + " : " + codes[i].ToString());
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
                    // Debug.Log("Disabled line " + i + " : " + codes[i].ToString());
                    continue;
                }
                yield return codes[i];
            }
        }
    }
}