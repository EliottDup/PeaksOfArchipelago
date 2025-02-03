using BepInEx;
using POKModManager;

using UnityEngine.Events;
using UnityEngine;
using BepInEx.Logging;

using HarmonyLib;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

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
    }

    public override void OnDisabled()
    {
        harmony.UnpatchSelf();
    }

    public override void Start()        // Runs every time a new scene is launched (whenever "Start" would be ran on a normal unity object)
    {
        // DO NOT PUT GAMEMANAGER.SAVE HERE IT FUCKED UP MY SAVES LOL
        Debug.Log("Now in scene: " + SceneManager.GetActiveScene().name);
    }

    private string GetUri()
    {
        return Hostname + ":" + Port;
    }

    private void OnConnect()
    {
        session.Connect(GetUri(), SlotName, Password);
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
            UnityUtils.SetArtefactText("Found " + itemInfo.playerName + "'s " + itemInfo.itemName + " uwu");
        }
    }

    [HarmonyPatch(typeof(RopeCollectable), "PickUpRope")]
    public class RopeCollectablePatch
    {
        static void Prefix(RopeCollectable __instance)
        {
            Debug.Log("prefixing");
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

    [HarmonyPatch(typeof(FallingEvent), "FellToDeath")]
    public class FellToDeathPatch
    {
        static void Postfix()
        {
            session.HandleDeath();
        }
    }

    [HarmonyPatch(typeof(ArtefactLoaderCabin), "Start")]
    public class ArtefactLoaderPatch
    {
        public static bool Prefix(ArtefactLoaderCabin __instance)
        {
            session.LoadArtefacts(__instance);
            return false;   // stop normal execution of LoadArtefacts
        }
    }

    [HarmonyPatch(typeof(EnterPeakScene), "Awake")]
    public class EnterPeakScenePatch
    {
        static void Postfix(EnterPeakScene __instance)
        {
            Debug.Log("Entering: " + GameObject.FindGameObjectWithTag("SummitBox").GetComponent<StamperPeakSummit>().peakNames.ToString());

            Debug.Log("texts:");
            foreach (Text text in GameObject.FindObjectsOfType<Text>())  //Leaving this in case some text is ever misbehaving
            {
                if (text.gameObject.name != "txt") continue;
                Debug.Log("TextMesh: " + text.gameObject.name + " : " + text.text);
                Debug.Log(text.transform.parent.name);
                foreach (Transform child in text.transform.parent)
                {
                    Debug.Log("    " + child.name);
                }
            }
        }
    }

    //------------------------- Harmony Transpilers -------------------------
    //! THESE ARE THE THINGS THAT MIGHT BREAK WHEN UPDATING THE GAME!!

    [HarmonyPatch(typeof(RopeAnchor), "Start")]
    public class StartTranspiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Debug.Log("Transpiling RopeAnchor.Start");
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i == 76 || i == 77 || i == 78)
                {
                    Debug.Log("Disabled line " + i + " : " + codes[i].ToString());
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
            Debug.Log("Transpiling DetachThenAttachToNew");
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i == 120 || i == 121 || i == 122 || i == 123)
                {
                    Debug.Log("Disabled line " + i + " : " + codes[i].ToString());
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
            Debug.Log("Transpiling PullOutAnchor");
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i == 79 || i == 80 || i == 81 || i == 82)
                {
                    Debug.Log("Disabled line " + i + " : " + codes[i].ToString());
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
            Debug.Log("Transpiling CheckRope");
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i == 267 || i == 268 || i == 269 || i == 270)
                {
                    Debug.Log("Disabled line " + i + " : " + codes[i].ToString());
                    continue;
                }
                yield return codes[i];
            }
        }
    }
}