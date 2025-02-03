using System;
using System.Reflection;
using PeaksOfArchipelago;
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

    // progress disablers (these basically undo whatever progress has been done in order to have the randomiser work properly)
    public static void UndoRopeProgress(RopeCollectable ropeCollectable)
    {
        RopeAnchor ropeAnchor = GameObject.FindGameObjectWithTag("Player").GetComponent<RopeAnchor>();
        if (ropeCollectable.isSingleRope)
        {
            GameManager.control.ropesCollected--;
            ropeAnchor.anchorsInBackpack--;
            ropeAnchor.UpdateRopesCollected();
        }
        else
        {
            GameManager.control.ropesCollected -= 2;
            ropeAnchor.anchorsInBackpack -= 2;
            ropeAnchor.UpdateRopesCollected();
        }
    }

    internal static void UndoArtefactProgress(ArtefactOnPeak instance)
    {   //BEHOLD: THE ABOMINATION hang on maybe not
        Artefacts artefact = Utils.GetArtefactFromCollectable(instance);

    }

    internal static void LoadArtefacts(ArtefactLoaderCabin instance)
    {
        // foreach (Artefacts artefact in )
    }

    // GameManager Helpers
    public static void SetGameManagerArtefactCollected(Artefacts artefact, bool value)
    {
        string fieldname = "hasArtefact_" + Utils.artefactToVariableName[artefact];
        FieldInfo field = typeof(GameManager).GetField(fieldname, BindingFlags.Instance | BindingFlags.Public);
        if (field != null && field.FieldType == typeof(bool))
        {
            field.SetValue(GameManager.control, value);
            return;
        }
        throw new Exception("No boolean field " + fieldname + "found in GameManager");
    }

    public static bool GetGameManagerArtefactCollected(Artefacts artefact)
    {
        string fieldname = "hasArtefact_" + Utils.artefactToVariableName[artefact];
        FieldInfo field = typeof(GameManager).GetField(fieldname, BindingFlags.Instance | BindingFlags.Public);
        if (field != null && field.FieldType == typeof(bool))
        {
            return (bool)field.GetValue(GameManager.control);
        }
        throw new Exception("No boolean field " + fieldname + "found in GameManager");
    }
}