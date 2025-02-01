using BepInEx;
using System;
using POKModManager;

using UnityEngine.Events;
using UnityEngine;
using BepInEx.Logging;

using HarmonyLib;
using Archipelago.MultiClient.Net;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace PeaksOfArchipelago;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("Data.POKManager")]
public class PeaksOfArchipelago : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        POKManager.RegisterMod(new PeaksOfArchipelagoMod(), MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION, "An Archipelago Implementation for Peaks Of Yore!", UseEditableAttributeOnly: true);
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded2!");

    }
}

public class PeaksOfArchipelagoMod : ModClass
{
    [Editable] public string Hostname { get; set; } = "archipelago.gg";
    [Editable] public string Port { get; set; } = "";
    [Editable] public string SlotName { get; set; } = "";
    [Editable] public string Password { get; set; } = "";
    [Editable] public bool AutoConnect { get; set; } = false;
    [Editable] public UnityEvent Connect { get; set; } = new UnityEvent();

    Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    public ArchipelagoSession session = null;

    public override void OnEnabled()
    {
        harmony.PatchAll();
        Connect.AddListener(OnConnect);
        if (AutoConnect)
        {
            ConnectFunc();
        }
        Debug.Log("Loaded Peaks of Archipelago!");
    }

    public override void Start()
    {
        // DO NOT PUT GAMEMANAGER.SAVE HERE IT FUCKED UP MY SAVES LOL
        Debug.Log("Now in scene: " + SceneManager.GetActiveScene().name);
    }

    public override void OnDisabled()
    {
        harmony.UnpatchSelf();
    }

    private void OnConnect()
    {
        ConnectFunc();
    }

    private bool ConnectFunc()
    {
        Debug.Log("Connecting to " + GetHostNamePort());
        session = ArchipelagoSessionFactory.CreateSession(GetHostNamePort());
        session.SetClientState(Archipelago.MultiClient.Net.Enums.ArchipelagoClientState.ClientReady);
        LoginResult result = session.TryConnectAndLogin("Peaks Of Yore", SlotName, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, password: Password);
        return false;
    }

    private string GetHostNamePort()
    {
        return Hostname + ":" + Port;
    }

    // Harmony Patches
    [HarmonyPatch(typeof(StamperPeakSummit), "StampJournal")]
    public class PeakSummitedPatch
    {
        static void Prefix(StamperPeakSummit __instance)
        {
            Debug.Log("stamping: " + __instance.peakNames.ToString());

        }

        static void Postfix(StamperPeakSummit __instance)
        {
            Debug.Log("done stamping: " + __instance.peakNames.ToString());
        }
    }

    [HarmonyPatch(typeof(FallingEvent), "FellToDeath")]
    public class FellToDeathPatch
    {
        static void Postfix()
        {
            Debug.Log("Died");
        }
    }

    [HarmonyPatch(typeof(EnterPeakScene), "Awake")]
    public class EnterPeakScenePatch
    {
        static void Postfix(EnterPeakScene __instance)
        {
            Debug.Log("Entering: " + GameObject.FindGameObjectWithTag("SummitBox").GetComponent<StamperPeakSummit>().peakNames.ToString());
        }
    }

    [HarmonyPatch(typeof(ArtefactOnPeak), "PickUpItem")]
    public class ArtefactOnPeakPatch
    {
        static void Postfix(ArtefactOnPeak __instance)
        {
            Debug.Log("Picked Up: " + __instance.peakArtefact);
            ArtefactOnPeak.Artefacts artefact = __instance.peakArtefact;
        }
    }

    [HarmonyPatch(typeof(RopeCollectable), "PickUpRope")]
    public class RopeCollectablePatch
    {
        static void Postfix(RopeCollectable __instance)
        {
            Ropes rope = Utils.GetRopeCollectable(__instance);    //CHECK FOR ROPE
            Debug.Log("Picked Up rope: " + rope);
        }
    }

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