using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class LDTools : MonoBehaviour
{
#if UNITY_EDITOR
    public List<GameObject> prefabsListTest_01 = new List<GameObject>();
    public List<GameObject> prefabsListTest_02 = new List<GameObject>();//balcony
    [Space]
    public GameObject prefabToSwapFor;
    public GameObject prefabToAddDefaultFor;

    //To change the selection afterward
    private GameObject[] newSelection = new GameObject[0];

    public float offsetDefault = 1;

    public void LateUpdate()
    {
        if(newSelection.Length > 0)
        {
            Selection.objects = newSelection;
            Debug.Log("Selection : "+ Selection.objects.Length);
            newSelection = new GameObject[0];
        }
    }


    [MenuItem("OrangeLetter/TryReplaceLeft #%A")]
    public static void TryReplaceLeft()    {        TryReplace(true);    }
    [MenuItem("OrangeLetter/TryReplaceRight #%E")]
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

        myLDTools.newSelection = Selection.gameObjects;
        for (int i = 0; i < myLDTools.newSelection.Length; i++)
        {
            GameObject gO = myLDTools.newSelection[i];

            GameObject newObj = TryReplaceOneElement(gO, myLDTools.prefabsListTest_01, left);
            if (newObj != null) { myLDTools.newSelection[i] = newObj; continue; }
            //Then, try on the second list
            newObj = TryReplaceOneElement(gO, myLDTools.prefabsListTest_02, left);
            if (newObj != null) { myLDTools.newSelection[i] = newObj; continue; }

            //else, item stay the same in selection
            Debug.LogWarning("Did not change " + myLDTools.newSelection[i].name + " : was not a prefab in any list.");
        }

    }

    public static GameObject TryReplaceOneElement(GameObject gO, List<GameObject> replaceList, bool left)
    {
        if (replaceList.Count == 0)
            return null;

        GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(gO);
        if (prefab == null)
            return null;

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
            string number = gO.name.Substring(gO.name.LastIndexOf(' '));
            string newName = newObject.name + number;
            newObject.name                      = newName;//need to only keep the number of any special element
            newObject.transform.parent          = gO.transform.parent;
            newObject.transform.localPosition   = gO.transform.localPosition;
            newObject.transform.localRotation   = gO.transform.localRotation;
            newObject.transform.localScale      = gO.transform.localScale;
            newObject.transform.SetSiblingIndex(  gO.transform.GetSiblingIndex());

            Debug.Log("Change " + gO.name + " to " + gO.name + "(" + index + ")");

            Undo.DestroyObjectImmediate(gO);
            return newObject;
        }

        return null;
    }


    [MenuItem("OrangeLetter/Move/TryRotateLeft #%Q")]
    public static void TryRotateLeft() { TryRotate(true); }
    [MenuItem("OrangeLetter/Move/TryRotateRight #%D")]
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
            Undo.RegisterCompleteObjectUndo(Selection.gameObjects[i], "Rotate game object via Shortcut");
            Selection.gameObjects[i].transform.Rotate((left ? 90 : -90) * Vector3.up);
        }

    }

    [MenuItem("OrangeLetter/Move/ChangeOffset Plus %#o")]
    public static void TryChangeOffsetPlus()
    {
        LDTools myLDTools = FindObjectOfType<LDTools>();
        if (myLDTools.offsetDefault <= .1f)
            myLDTools.offsetDefault = .5f;
        else if (myLDTools.offsetDefault <= .5f)
            myLDTools.offsetDefault = 1f;
        Debug.Log("Default offset is now : " + myLDTools.offsetDefault);
    }
    [MenuItem("OrangeLetter/Move/ChangeOffset Minus %#l")]
    public static void TryChangeOffsetMinus()
    {
        LDTools myLDTools = FindObjectOfType<LDTools>();
        if (myLDTools.offsetDefault >= 1f)
            myLDTools.offsetDefault = .5f;
        else if (myLDTools.offsetDefault >= .5f)
            myLDTools.offsetDefault = .1f;
        Debug.Log("Default offset is now : " + myLDTools.offsetDefault);
    }

    [MenuItem("OrangeLetter/Move/TryMoveLeft %#LEFT")]
    public static void TryMoveLeft () { TryMove(Vector3.left); }
    [MenuItem("OrangeLetter/Move/TryMoveRight %#RIGHT")]
    public static void TryMoveRight() { TryMove(Vector3.right); }
    [MenuItem("OrangeLetter/Move/TryMoveForward %#UP")]
    public static void TryMoveForward() { TryMove(Vector3.forward); }
    [MenuItem("OrangeLetter/Move/TryMoveBackward %#DOWN")]
    public static void TryMoveBackWard() { TryMove(Vector3.back); }
    [MenuItem("OrangeLetter/Move/TryMoveUp %#U")]
    public static void TryMoveUp() { TryMove(Vector3.up); }
    [MenuItem("OrangeLetter/Move/TryMoveDown %#J")]
    public static void TryMoveDown() { TryMove(Vector3.down); }


    public static void TryMove(Vector3 direction)
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

        LDTools myLDTools = FindObjectOfType<LDTools>();

        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            Undo.RegisterCompleteObjectUndo(Selection.gameObjects[i], "Move game object via Shortcut");
            Selection.gameObjects[i].transform.position += direction * myLDTools.offsetDefault;
        }

    }



    [MenuItem("OrangeLetter/TryReplaceSelection")]
    public static void TryReplaceSelectionWithOne()
    {
        LDTools myLDTools = FindObjectOfType<LDTools>();


        myLDTools.newSelection = Selection.gameObjects;
        for (int i = 0; i < myLDTools.newSelection.Length; i++)
        {
            GameObject gO = myLDTools.newSelection[i];

            GameObject newObj = TryReplaceOneElementWithOne(gO, myLDTools.prefabToSwapFor);
            if (newObj != null) { myLDTools.newSelection[i] = newObj; continue; }
            //else, item stay the same in selection
            Debug.LogWarning("Did not change " + myLDTools.newSelection[i].name + " : was not a prefab in any list.");
        }
    }
    public static GameObject TryReplaceOneElementWithOne(GameObject gO, GameObject prefab) 
    {
        if (gO == null)
            return null;

        if (prefab == null)
            return null;

        GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(newObject, "Replace With Prefabs");
        newObject.name = gO.name; //kept the same name
        newObject.SetActive(gO.activeSelf);
        newObject.transform.parent = gO.transform.parent;
        newObject.transform.localPosition = gO.transform.localPosition;
        newObject.transform.localRotation = gO.transform.localRotation;
        newObject.transform.localScale = gO.transform.localScale;
        newObject.transform.SetSiblingIndex(gO.transform.GetSiblingIndex());

        Debug.Log("Change " + gO.name + " to " + prefab.name);

        Undo.DestroyObjectImmediate(gO);
        return newObject;
    }



    [MenuItem("OrangeLetter/TryAddAtSelection %#=")]
    public static void TryAddAtSelection()
    {
        LDTools myLDTools = FindObjectOfType<LDTools>();

        GameObject gO = Selection.activeGameObject;
        GameObject newObj = TryAddOneElementAtSelection(gO, myLDTools.prefabToAddDefaultFor);
        if (newObj != null) { myLDTools.newSelection = new GameObject[1]; myLDTools.newSelection[0] = newObj; return; }
        //else, item stay the same in selection
        Debug.LogWarning("Did not change " + myLDTools.newSelection[0].name + " : was not a prefab in any list.");

    }
    public static GameObject TryAddOneElementAtSelection(GameObject gO, GameObject prefab)
    {
        if (gO == null)
            return null;

        if (prefab == null)
            return null;

        GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(newObject, "Add At Selection");
        newObject.name = gO.name;//need to only keep the number of any special element
        newObject.transform.parent = gO.transform.parent;
        newObject.transform.localPosition = gO.transform.localPosition;
        newObject.transform.localRotation = gO.transform.localRotation;
        newObject.transform.localScale = gO.transform.localScale;
        newObject.transform.SetSiblingIndex(gO.transform.GetSiblingIndex());

        Debug.Log("Add " + prefab.name + " at " + gO.name);
        return newObject;
    }

#endif
}
