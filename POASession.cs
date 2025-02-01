using System;
using System.Reflection;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using UnityEngine;

namespace PeaksOfArchipelago;

class POASession
{
    ArchipelagoSession session;
    DeathLinkService deathLinkService;
    bool playerKilled = false;
    public bool Connect(string uri, string SlotName, string Password)
    {
        session = ArchipelagoSessionFactory.CreateSession(uri);
        LoginResult result = session.TryConnectAndLogin("Peaks Of Yore", SlotName, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, password: Password);
        if (session.DataStorage["version"] != ModInfo.MOD_VERSION)
        {
            Debug.Log("Version Incompatible");
            return false;
        }

        deathLinkService = session.CreateDeathLinkService();
        deathLinkService.EnableDeathLink();


        deathLinkService.OnDeathLinkReceived += (deathLinkObject) =>
        {
            Debug.Log(deathLinkObject.Source + deathLinkObject.Cause);
            KillPlayer();
        };
        return result.Successful;
    }

    public void HandleDeath()
    {
        if (playerKilled)
        {
            playerKilled = false;   // This is done to prevent players killed by deathlink to send out deathlink ticks again
            return;
        }
        Debug.Log("Sending Deathlink");
        if (session == null) return;
        deathLinkService.SendDeathLink(new DeathLink(session.Players.GetPlayerAliasAndName(session.ConnectionInfo.Slot), "Fell off."));
    }

    public void KillPlayer()
    {
        FallingEvent[] events = GameObject.FindObjectsOfType<FallingEvent>();
        Debug.Log("found " + events.Length + " fallingEvents");
        foreach (FallingEvent fallingEvent in events)
        {
            MethodInfo method = typeof(FallingEvent).GetMethod("FellToDeath", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                return;
            }
            Debug.Log("killing player");
            playerKilled = true;
            method.Invoke(fallingEvent, null);
        }
    }

    public Ropes CompleteRopeCheck(RopeCollectable ropeCollectable)
    {
        if (session == null) return (Ropes)(-1);
        Ropes rope = Utils.GetRopeFromCollectable(ropeCollectable);
        session.Locations.CompleteLocationChecks(Utils.GetLocationFromRope(rope));
        Debug.Log("Completing rope " + rope.ToString());    // TODO: BLOCK UNLOCKING OF ROPE IN-GAME
        Debug.Log("TODO: Block rope pick up in-game");
        return rope;
    }

    public Artefacts CompleteArtefactCheck(ArtefactOnPeak artefactOnPeak)
    {
        if (session == null) return (Artefacts)(-1);

        Artefacts artefact = Utils.GetArtefactFromCollectable(artefactOnPeak);
        session.Locations.CompleteLocationChecks(Utils.GetLocationFromArtefact(artefact));
        Debug.Log("Completing artefact " + artefact.ToString());    // TODO: BLOCK UNLOCKING OF ARTEFACT IN-GAME
        Debug.Log("TODO: Block artefact pick up in-game");
        return artefact;
    }

    private bool assertSession()
    {
        if (session == null) Debug.Log("No Session Found!");
        return session != null;
    }
}