using System;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace PeaksOfArchipelago;

static class UnityUtils
{
    public static ManualLogSource logger;

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
        logger.LogInfo("---------");
        logger.LogInfo($"name: {gameObject.name}");
        logger.LogInfo($"parent: {gameObject.transform.parent.name}");
        logger.LogInfo($"pos: {gameObject.transform.position}");
        logger.LogInfo($"rot: {gameObject.transform.rotation}");
        foreach (Component comp in gameObject.GetComponents<Component>())
        {
            Type type = comp.GetType();
            logger.LogInfo($"---{type.Name}---");
            // Copy fields and properties
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                logger.LogInfo($"{field.Name}: {field.GetValue(comp)}");
            }
        }
        foreach (Transform child in gameObject.transform)
        {
            PrintObjectData(child.gameObject);
        }
    }
}