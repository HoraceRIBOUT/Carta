using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class LDTools : MonoBehaviour
{
#if UNITY_EDITOR
    public List<GameObject> prefabsListTest_01 = new List<GameObject>();


    [MenuItem("OrangeLetter/TryReplaceLeft %UP")]
    public static void TryReplaceLeft()    {        TryReplace(true);    }
    [MenuItem("OrangeLetter/TryReplaceRight %DOWN")]
    public static void TryReplaceRight()    {       TryReplace(false);    }

    public static void TryReplace(bool left)
    {
        LDTools myLDTools = FindObjectOfType<LDTools>();

        if(Selection.activeGameObject == null)
        {
            Debug.LogError("No item selected. (activeGO == null)");
            return;
        }
        if(Selection.gameObjects.Length == 0)
        {
            Debug.LogError("No item selected. (gameObjects == 0)");
            return;
        }

        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            if (TryReplaceOneElement(i, myLDTools.prefabsListTest_01, left)) break;
        }
    }

    public static bool TryReplaceOneElement(int selectionIndex, List<GameObject> replaceList, bool left)
    {
        GameObject gO = Selection.gameObjects[selectionIndex];
        if (replaceList.Count == 0)
            return false;

        GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(gO);
        if (prefab == null)
            return false;

        int index = replaceList.IndexOf(prefab);
        if (index >= 0)
        {
            index += (left ? -1 : 1);
            if (index < 0)
                index = replaceList.Count-1;
            if (index >= replaceList.Count)
                index = 0;
            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(replaceList[index]); 
            Undo.RegisterCreatedObjectUndo(newObject, "Replace With Prefabs");
            newObject.name                      = gO.name;//need to only keep the number of any special element
            newObject.transform.parent          = gO.transform.parent;
            newObject.transform.localPosition   = gO.transform.localPosition;
            newObject.transform.localRotation   = gO.transform.localRotation;
            newObject.transform.localScale      = gO.transform.localScale;
            newObject.transform.SetSiblingIndex(  gO.transform.GetSiblingIndex());

            Debug.Log("Change " + gO.name + " to " + gO.name + "(" + index + ")");
            Selection.activeGameObject = newObject;

            Undo.DestroyObjectImmediate(gO);
            return true;
        }

        return false;
    }


    [MenuItem("OrangeLetter/TryRotateLeft %LEFT")]
    public static void TryRotateLeft() { TryRotate(true); }
    [MenuItem("OrangeLetter/TryRotateRight %RIGHT")]
    public static void TryRotateRight() { TryRotate(false); }

    public static void TryRotate(bool left)
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogError("No item selected. (activeGO == null)");
            return;
        }
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogError("No item selected. (gameObjects == 0)");
            return;
        }

        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            Selection.gameObjects[i].transform.Rotate((left ? 90 : -90) * Vector3.up);
        }

    }


#endif
}
