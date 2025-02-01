using System;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using UnityEngine;

namespace PeaksOfArchipelago;

class POASession
{
    ArchipelagoSession session;
    DeathLinkService deathLinkService;
    public bool Connect(String uri, String SlotName, String Password)
    {
        session = ArchipelagoSessionFactory.CreateSession(uri);
        LoginResult result = session.TryConnectAndLogin("Peaks Of Yore", SlotName, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, password: Password);
        deathLinkService = session.CreateDeathLinkService();
        deathLinkService.EnableDeathLink();
        deathLinkService.OnDeathLinkReceived += (deathLinkObject) =>
        {
            Debug.Log(deathLinkObject.Source + deathLinkObject.Cause);
            //! TODO: KILL
        };
        return result.Successful;
    }



    public void HandleDeath()
    {
        deathLinkService.SendDeathLink(new DeathLink(session.Players.GetPlayerAliasAndName(session.ConnectionInfo.Slot), "Fell off."));
    }

    internal void CompleteLocationCheck(LocationCheck loc)
    {
        Debug.Log("Unlocking " + loc.ToString());
        session.Locations.CompleteLocationChecks((long)loc);
    }
}