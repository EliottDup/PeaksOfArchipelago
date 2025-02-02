using System;
using UnityEngine;
using UnityEngine.UI;

static class UnityUtils
{
    public static void SetRopeText(string message)
    {
        Text ropeCollected = GameObject.Find("RopeCollectedTxt")?.GetComponentInChildren<Text>();
        Text NPCRopeCollected = GameObject.Find("NPCRopeCollectedTxt")?.GetComponentInChildren<Text>();
        if (ropeCollected != null) ropeCollected.text = message;
        else Debug.Log("didnt find RopeCollectedText");
        if (NPCRopeCollected != null) NPCRopeCollected.text = message;
        else Debug.Log("didnt find NPCRopeCollectedText");
    }

    internal static void SetArtefactText(string message)
    {
        Text artefactCollected = GameObject.Find("ArtefactCollected")?.GetComponentInChildren<Text>();
        if (artefactCollected != null) artefactCollected.text = message;
        else Debug.Log("didnt find artefactcollectedtext");
    }
}