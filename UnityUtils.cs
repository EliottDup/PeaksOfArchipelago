using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace PeaksOfArchipelago;

static class UnityUtils
{
    public static void SetRopeText(string message)
    {
        Text ropeCollected = GameObject.Find("RopeCollectedTxt")?.GetComponentInChildren<Text>();
        Text NPCRopeCollected = GameObject.Find("NPCRopeCollectedTxt")?.GetComponentInChildren<Text>();
        if (ropeCollected != null) ropeCollected.text = message;
        else Debug.LogWarning("didnt find RopeCollectedText");
        if (NPCRopeCollected != null) NPCRopeCollected.text = message;
        else Debug.LogWarning("didnt find NPCRopeCollectedText");
    }

    internal static void SetArtefactText(string message)
    {
        Text artefactCollected = GameObject.Find("ArtefactCollected")?.GetComponentInChildren<Text>();
        if (artefactCollected != null) artefactCollected.text = message;
        else Debug.LogWarning("didnt find artefactcollectedtext");
    }

    public static void SetSeedText(string message)
    {
        Text BirdSeedCollected = GameObject.Find("BirdSeedCollectedTxt")?.GetComponentInChildren<Text>();
        if (BirdSeedCollected != null) BirdSeedCollected.text = message;
        else Debug.LogWarning("didnt find BirdSeedCollectedTxt");
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

    public static void SetGameManagerArtefactDirty(Artefacts artefact, bool value)
    {
        string fieldname = "artefact_" + Utils.artefactToVariableName[artefact] + "_IsDirty";
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

    public static void PrintObjectData(GameObject gameObject)
    {
        Debug.Log("---------");
        Debug.Log($"name: {gameObject.name}");
        Debug.Log($"parent: {gameObject.transform.parent.name}");
        Debug.Log($"pos: {gameObject.transform.position}");
        Debug.Log($"rot: {gameObject.transform.rotation}");
        foreach (Component comp in gameObject.GetComponents<Component>())
        {
            Type type = comp.GetType();
            Debug.Log($"---{type.Name}---");
            // Copy fields and properties
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                Debug.Log($"{field.Name}: {field.GetValue(comp)}");
            }
        }
        foreach (Transform child in gameObject.transform)
        {
            PrintObjectData(child.gameObject);
        }
    }
}