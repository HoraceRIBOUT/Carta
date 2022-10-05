using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pnj : MonoBehaviour
{
    public bool playerOnReach;

    public enum pnjID
    {
        ned,
        flowerMom,
        flowerAunt,
    }
    public pnjID id;



    public Dialog defaultDialog;
    public List<ItemReaction> reactions = new List<ItemReaction>();

    public Dialog giveReaction;         //when you give you an item, the first reaction
    public Dialog giveReaction_Fail;    //when you give an item, it's on the list but not give
    public Dialog defaultGiveReaction;  //when you give a non-needed item
    public Dialog defaultShowReaction;  //when you show a non-needed item

    public List<Transform> cameraPoints = new List<Transform>();

    [System.Serializable]
    public class ItemReaction
    {
        public itemID itemToReactFrom = itemID.none;

        public bool finalTarget = false;//can give, it take
        [Sirenix.OdinInspector.ShowIf("finalTarget")]
        public AudioClip musicGiveCorrect;
        public Dialog responseGive; //most of the time, just redirect after a line when not final target
        public Dialog responseShow;

#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button]
        public void CreateNeededDialog()
        {
            GameObject currentPNJ = UnityEditor.Selection.activeGameObject;

            Dialog asset1 = ScriptableObject.CreateInstance<Dialog>();
            asset1.name = currentPNJ.name + "_" + itemToReactFrom.ToString() + "_Give.asset";
            Dialog asset2 = ScriptableObject.CreateInstance<Dialog>();
            asset2.name = currentPNJ.name + "_" + itemToReactFrom.ToString() + "_Show.asset";

            if(!System.IO.Directory.Exists("Assets/Data/Dialog/" + currentPNJ.name + "/"))
            {
                System.IO.Directory.CreateDirectory("Assets/Data/Dialog/" + currentPNJ.name + "/");
            }

            UnityEditor.AssetDatabase.CreateAsset(asset1, "Assets/Data/Dialog/" + currentPNJ.name + "/" + asset1.name);
            responseGive = asset1;
            UnityEditor.AssetDatabase.CreateAsset(asset2, "Assets/Data/Dialog/" + currentPNJ.name + "/" + asset2.name);
            responseShow = asset2;
            UnityEditor.AssetDatabase.SaveAssets();

            UnityEditor.Selection.activeObject = asset1;
        }
#endif
    }

    public bool ReturnUpdate()
    {
        //Better if take care in a "control manager" and compare to "dialogManager" too
        if (!playerOnReach)
            return false;
        if(!GameManager.instance.playerMove.talking)
        {
            Talk();
            return true;
        }
        return false;
    }

    public void Talk()
    {
        GameManager.instance.dialogMng.StartDialog(defaultDialog, this);

        if (cameraPoints != null && cameraPoints.Count != 0)
            GameManager.instance.cameraMng.SetSecondaryTarget(cameraPoints[0]);
    }







#if UNITY_EDITOR
    [Sirenix.OdinInspector.Button]
    public void CreateDefaultDialog()
    {
        GameObject currentPNJ = UnityEditor.Selection.activeGameObject;

        if (!System.IO.Directory.Exists("Assets/Data/Dialog/" + currentPNJ.name + "/"))
        {
            System.IO.Directory.CreateDirectory("Assets/Data/Dialog/" + currentPNJ.name + "/");
        }

        if (!System.IO.File.Exists("Assets/Data/Dialog/" + currentPNJ.name + "/" + currentPNJ.name + "_DefaultDialog.asset"))
        {
            Dialog asset1 = ScriptableObject.CreateInstance<Dialog>();
            asset1.name = currentPNJ.name + "_DefaultDialog.asset";
            UnityEditor.AssetDatabase.CreateAsset(asset1, "Assets/Data/Dialog/" + currentPNJ.name + "/" + asset1.name);

            defaultDialog = asset1;
            UnityEditor.Selection.activeObject = asset1;
        }


        if (!System.IO.File.Exists("Assets/Data/Dialog/" + currentPNJ.name + "/" + currentPNJ.name + "_GiveReaction.asset"))
        {
            Dialog asset2 = ScriptableObject.CreateInstance<Dialog>();
            asset2.name = currentPNJ.name + "_GiveReaction.asset";
            UnityEditor.AssetDatabase.CreateAsset(asset2, "Assets/Data/Dialog/" + currentPNJ.name + "/" + asset2.name);

            giveReaction = asset2;
            UnityEditor.Selection.activeObject = asset2;
        }

        if (!System.IO.File.Exists("Assets/Data/Dialog/" + currentPNJ.name + "/" + currentPNJ.name + "_GiveReaction_Fail.asset"))
        {
            Dialog asset2 = ScriptableObject.CreateInstance<Dialog>();
            asset2.name = currentPNJ.name + "_GiveReaction_Fail.asset";
            UnityEditor.AssetDatabase.CreateAsset(asset2, "Assets/Data/Dialog/" + currentPNJ.name + "/" + asset2.name);

            giveReaction_Fail = asset2;
            UnityEditor.Selection.activeObject = asset2;
        }

        if (!System.IO.File.Exists("Assets/Data/Dialog/" + currentPNJ.name + "/" + currentPNJ.name + "_DefaultGiveReaction.asset"))
        {
            Dialog asset2 = ScriptableObject.CreateInstance<Dialog>();
            asset2.name = currentPNJ.name + "_DefaultGiveReaction.asset";
            UnityEditor.AssetDatabase.CreateAsset(asset2, "Assets/Data/Dialog/" + currentPNJ.name + "/" + asset2.name);

            defaultGiveReaction = asset2;
            UnityEditor.Selection.activeObject = asset2;
        }

        if (!System.IO.File.Exists("Assets/Data/Dialog/" + currentPNJ.name + "/" + currentPNJ.name + "_DefaultShowReaction.asset"))
        {
            Dialog asset2 = ScriptableObject.CreateInstance<Dialog>();
            asset2.name = currentPNJ.name + "_DefaultShowReaction.asset";
            UnityEditor.AssetDatabase.CreateAsset(asset2, "Assets/Data/Dialog/" + currentPNJ.name + "/" + asset2.name);

            defaultShowReaction = asset2;
            UnityEditor.Selection.activeObject = asset2;
        }

        UnityEditor.AssetDatabase.SaveAssets();
    }
#endif

}
