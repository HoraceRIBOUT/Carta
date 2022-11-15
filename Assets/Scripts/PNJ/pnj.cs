using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pnj : MonoBehaviour
{
    public bool playerOnReach;

    public enum pnjID
    {
        None = 0,
        stagiaire       = 1,
        postma          = 2,
        flowerMom       = 3,
        flowerKid       = 4,
        flowerDad       = 5,
        babiol          = 6,

        crow            = 7,
        seawatcher      = 8,

        boxLudo         = 9,
        guitar          = 10,
        biblio          = 11,
        simonSez        = 12,

        parainTem       = 13,
        cuistTem        = 14,
        ospinoTem       = 15,
        sistUp          = 16,
        aguilarTem      = 17,
        sistLittle      = 18,
                        
        prof            = 19,
        chiefLudo       = 20,
        forge           = 21,

    }
    public pnjID id;

    public Color defaultColor = Color.black;

    public Dialog defaultDialog;
    public List<ItemReaction> reactions = new List<ItemReaction>();
    public List<Dialog_ToShow> nextDialog = new List<Dialog_ToShow>(); //sorted by priority
    [System.Serializable]
    public struct Dialog_ToShow
    {
        public Dialog dialog;
        public int priority;
        public Dialog_ToShow(Dialog _dialog, int _priority)
        {
            dialog = _dialog;
            priority = _priority;
        }
    }

    public List<Visual_Position> visuals = new List<Visual_Position>();
    [Sirenix.OdinInspector.ReadOnly]public int visualIndex = 0;
    [System.Serializable]
    public struct Visual_Position
    {
        public List<GameObject> toTurnOn;
        public Camera newCameraZero;
    }

    public Dialog giveReaction;         //when you give you an item, the first reaction
    public Dialog giveReaction_Fail;    //when you give an item, it's on the list but not give
    public Dialog defaultGiveReponse;  //when you give a non-needed item 
    public Dialog defaultShowReponse;  //when you show a non-needed item

    public List<Transform> cameraPoints = new List<Transform>();


    [Header("Action Button")]
    [SerializeField] private bool actionButt_On = false;
    [SerializeField] private float actionButt_Val = 0;
    [SerializeField] private float actionButt_speed = 3;
    private Coroutine actionButt_Coroutine;
    [SerializeField] private SpriteRenderer actionButt_visual_key;
    [SerializeField] private SpriteRenderer actionButt_visual_con;

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
            if (!System.IO.Directory.Exists("Assets/Data/Dialog/" + currentPNJ.name + "/Item/"))
            {
                System.IO.Directory.CreateDirectory("Assets/Data/Dialog/" + currentPNJ.name + "/Item/");
            }

            string filePath = "Assets/Data/Dialog/" + currentPNJ.name + "/Item/" + asset1.name;
            if (!System.IO.File.Exists(filePath))
            {
                UnityEditor.AssetDatabase.CreateAsset(asset1, filePath);
            }
            else
            {
                asset1 = (Dialog)UnityEditor.AssetDatabase.LoadAssetAtPath(filePath, typeof(Dialog));
            }

            filePath = "Assets/Data/Dialog/" + currentPNJ.name + "/Item/" + asset2.name;
            if (!System.IO.File.Exists(filePath))
            {
                UnityEditor.AssetDatabase.CreateAsset(asset2, filePath);
            }
            else
            {
                asset2 = (Dialog)UnityEditor.AssetDatabase.LoadAssetAtPath(filePath, typeof(Dialog));
            }

            responseGive = asset1;
            responseShow = asset2;
            UnityEditor.AssetDatabase.SaveAssets();

            UnityEditor.Selection.activeObject = asset1;
        }
#endif
    }

    public void Start()
    {
        foreach(Transform gO in cameraPoints)
        {
            Camera cam = gO.GetComponent<Camera>();
            if (cam != null)
            {
                //maybe we will save the VOF if needed
                Destroy(cam);
            }
        }
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
        GameManager.instance.dialogMng.StartDialog(GetDialogToShow(), this);

        if (cameraPoints != null && cameraPoints.Count != 0)
            GameManager.instance.cameraMng.SetSecondaryTarget(cameraPoints[0]);
    }

    public Dialog GetDialogToShow()
    {
        if(nextDialog.Count == 0)
        {
            return defaultDialog;
        }
        Dialog res = nextDialog[0].dialog;
        nextDialog.RemoveAt(0);
        return res;
    }

    public void AddNextDialog(Dialog dial, int priority)
    {
        Dialog_ToShow dialInfo = new pnj.Dialog_ToShow(dial, priority);
        for (int i = 0; i < nextDialog.Count; i++)
        {
            if(priority > nextDialog[i].priority)
            {
                nextDialog.Insert(i, dialInfo);
                return;
            }
        }
        //else
        nextDialog.Add(dialInfo);
    }

    public void ChangeVisual(int index)
    {
        if(index < 0 || index >= visuals.Count)
        {
            Debug.LogError("Try change visual of "+id + " to index " + index + " but only have "+ visuals.Count + " visual.");
        }
        foreach(GameObject gO in visuals[visualIndex].toTurnOn) 
        {
            gO.SetActive(false);
        }

        foreach (GameObject gO in visuals[index].toTurnOn)
        {
            gO.SetActive(false);
        }

        cameraPoints[0] = visuals[index].newCameraZero.transform;

        visualIndex = index;
    }


    public void ChangeIcon(bool controllerIcon)
    {
            actionButt_visual_con.gameObject.SetActive(controllerIcon);
            actionButt_visual_key.gameObject.SetActive(!controllerIcon);
    }

    public void TurnActionOnOrOff(bool value)
    {
        if(actionButt_On == value)
        {
            return;
        }
        actionButt_On = value;
        if (actionButt_Coroutine != null)
            StopCoroutine(actionButt_Coroutine);
        actionButt_Coroutine = StartCoroutine(ActionButt_GoesToTarget());
    }

    private IEnumerator ActionButt_GoesToTarget()
    {
        while ((actionButt_On ?  actionButt_Val < 1 : actionButt_Val > 0) )
        {
            actionButt_Val += Time.deltaTime * actionButt_speed* (actionButt_On ? 1 : -1);
            ActionButt_UpdateVisual();
            yield return new WaitForSeconds(1f / 60f);
        }
        ActionButt_UpdateVisual();
    }

    private void ActionButt_UpdateVisual()
    {
        actionButt_visual_con.color = Color.Lerp(Color.white-Color.black, Color.white, actionButt_Val);
        actionButt_visual_key.color = Color.Lerp(Color.white-Color.black, Color.white, actionButt_Val);
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
        else
        {
            defaultDialog = (Dialog) UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Data/Dialog/" + currentPNJ.name + "/" + currentPNJ.name + "_DefaultDialog.asset", typeof(Dialog));
        }


        if (!System.IO.File.Exists("Assets/Data/Dialog/" + currentPNJ.name + "/" + currentPNJ.name + "_GiveReaction.asset"))
        {
            Dialog asset2 = ScriptableObject.CreateInstance<Dialog>();
            asset2.name = currentPNJ.name + "_GiveReaction.asset";
            UnityEditor.AssetDatabase.CreateAsset(asset2, "Assets/Data/Dialog/" + currentPNJ.name + "/" + asset2.name);

            giveReaction = asset2;
            UnityEditor.Selection.activeObject = asset2;
        }
        else
        {
            giveReaction = (Dialog)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Data/Dialog/" + currentPNJ.name + "/" + currentPNJ.name + "_GiveReaction.asset", typeof(Dialog));
        }

        if (!System.IO.File.Exists("Assets/Data/Dialog/" + currentPNJ.name + "/" + currentPNJ.name + "_GiveReaction_Fail.asset"))
        {
            Dialog asset2 = ScriptableObject.CreateInstance<Dialog>();
            asset2.name = currentPNJ.name + "_GiveReaction_Fail.asset";
            UnityEditor.AssetDatabase.CreateAsset(asset2, "Assets/Data/Dialog/" + currentPNJ.name + "/" + asset2.name);

            giveReaction_Fail = asset2;
            UnityEditor.Selection.activeObject = asset2;
        }
        else
        {
            giveReaction_Fail = (Dialog)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Data/Dialog/" + currentPNJ.name + "/" + currentPNJ.name + "_GiveReaction_Fail.asset", typeof(Dialog));
        }

        if (!System.IO.File.Exists("Assets/Data/Dialog/" + currentPNJ.name + "/" + currentPNJ.name + "_DefaultGiveReaction.asset"))
        {
            Dialog asset2 = ScriptableObject.CreateInstance<Dialog>();
            asset2.name = currentPNJ.name + "_DefaultGiveReaction.asset";
            UnityEditor.AssetDatabase.CreateAsset(asset2, "Assets/Data/Dialog/" + currentPNJ.name + "/" + asset2.name);

            defaultGiveReponse = asset2;
            UnityEditor.Selection.activeObject = asset2;
        }
        else
        {
            defaultGiveReponse = (Dialog)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Data/Dialog/" + currentPNJ.name + "/" + currentPNJ.name + "_DefaultGiveReaction.asset", typeof(Dialog));
        }

        if (!System.IO.File.Exists("Assets/Data/Dialog/" + currentPNJ.name + "/" + currentPNJ.name + "_DefaultShowReaction.asset"))
        {
            Dialog asset2 = ScriptableObject.CreateInstance<Dialog>();
            asset2.name = currentPNJ.name + "_DefaultShowReaction.asset";
            UnityEditor.AssetDatabase.CreateAsset(asset2, "Assets/Data/Dialog/" + currentPNJ.name + "/" + asset2.name);

            defaultShowReponse = asset2;
            UnityEditor.Selection.activeObject = asset2;
        }
        else
        {
            defaultShowReponse = (Dialog)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Data/Dialog/" + currentPNJ.name + "/" + currentPNJ.name + "_DefaultShowReaction.asset", typeof(Dialog));
        }

        UnityEditor.AssetDatabase.SaveAssets();
    }



    [Sirenix.OdinInspector.Button]
    public static Dialog CreateDefaultDialog_GenericDialog(string fileGroup, string fileSuffix)
    {
        if (!System.IO.Directory.Exists("Assets/Data/Dialog/" + fileGroup + "/"))
        {
            System.IO.Directory.CreateDirectory("Assets/Data/Dialog/" + fileGroup + "/");
        }
        Dialog asset = null;
        if (!System.IO.File.Exists("Assets/Data/Dialog/" + fileGroup + "/" + fileGroup + fileSuffix))
        {
            asset = ScriptableObject.CreateInstance<Dialog>();
            asset.name = fileGroup + fileSuffix;
            UnityEditor.AssetDatabase.CreateAsset(asset, "Assets/Data/Dialog/" + fileGroup + "/" + asset.name);

            UnityEditor.Selection.activeObject = asset;
        }

        UnityEditor.AssetDatabase.SaveAssets();
        return asset;
    }
#endif

}
