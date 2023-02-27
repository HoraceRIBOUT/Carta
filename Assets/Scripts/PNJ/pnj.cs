using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class pnj : MonoBehaviour
{
    public bool playerOnReach;

    public enum pnjID
    {
        None = 0,
        
        postWoman       = 2,
        seawatcher      = 8,
        boxLudo         = 9,
        wolfGirl        = 23,
        chiefLudo       = 20,
        mimo            = 22,

        youngUncle      = 23,
        oldNephew       = 24,
        guitar          = 10,

        babiol          = 6,
        flowerMom       = 3,
        climbrDad       = 5,
        stagiaire       = 1,
        flowerKid       = 4,

        sistUp          = 16,
        sistLittle      = 18,

        parainTem       = 13,
        cuistTem        = 14,
        ospinoTem       = 15,
        forge           = 21,
        aguilarTem      = 17,
        crowCool        = 7,
        kiddo           = 12, 

                        
        biblio          = 11, 
        prof            = 19,
        //PNJ_number = 25,

    }
    public pnjID id;

    public Color defaultColor = Color.black;

    [Header("Dialog data")]
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

    public Dialog giveReaction;         //when you give you an item, the first reaction
    [Tooltip("when you give a not-in-the-list item")]
    public Dialog defaultGiveReponse;
    [Tooltip("when you show a not-in-the-list item")]
    public Dialog defaultShowReponse;  //when you show a not-in-the-list item

    [Header("Visual")]
    [SerializeField] private Animator animator;
    public List<Visual_Position> visuals = new List<Visual_Position>();
    [Sirenix.OdinInspector.ReadOnly]public int visualIndex = 0;
    [System.Serializable]
    public struct Visual_Position
    {
        public List<GameObject> toTurnOn;
        public Camera newCameraZero;
    }
    public pnj_face face = null;

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
                //Destroy(cam);
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
        GameManager.instance.mapAndPaper.iconZone.AddIconIfNeeded(this.id);


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

    public void LineStart()
    {
        animator.SetBool("Blabla", true);
    }
    public void LineEnd()
    {
        animator.SetBool("Blabla", false);
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

    public void LaunchAnimation(int animationIndex)
    {
        for (int i = 1; i < 6; i++)
        {
            animator.SetBool("Reaction" + i, false);
        }

        if(animationIndex != 0)
            animator.SetBool("Reaction" + animationIndex, true);
    }

    public void ChangeFace(int eyeIndex, int mouthIndex)
    {
        if(face == null)
        {
            face = GetComponentInChildren<pnj_face>();
            if(face == null)
            {
                Debug.LogError(id + " don't have a face.");
                return;
            }
        }
        face.ChangeFace(eyeIndex, mouthIndex);
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

    [Sirenix.OdinInspector.Button]
    public void ExportToCSV()
    {
        //create a csv with :
        // defaultDialog --> just a start dialog
        // giveReaction --> wait before giving
        // defaultGiveReponse -> fail
        // defaultShowReponse -> fail
        List<Dialog> contatenatedDialog = new List<Dialog>(); 
        contatenatedDialog.Add(defaultDialog);
        contatenatedDialog.Add(giveReaction);
        contatenatedDialog.Add(defaultGiveReponse);
        contatenatedDialog.Add(defaultShowReponse);
        CreateCSV.WriteDialogInCVS("Assets/Resources/CSV/Basic_" + id + ".csv", contatenatedDialog);
        
        Debug.Log("Finish exporting CSV for " + id);
    }



#endif

}
