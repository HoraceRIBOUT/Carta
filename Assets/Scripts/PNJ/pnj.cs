using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class pnj : MonoBehaviour
{
    [Sirenix.OdinInspector.ReadOnly]
    public bool playerOnReach;

    public enum pnjID
    {
        None = 0,
        
        postWoman       = 2,
        seawatcher      = 8,
        boxLudo         = 9,
        wolfGirl        = 25,
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
        //PNJ_number = 29,


        tomb_grandad = 26,
        tomb_mom = 27,
        tomb_infant = 28,

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
    
    [Tooltip("when you give a not-in-the-list item")]
    public Dialog giveFail_Dial;
    [Tooltip("when you show a not-in-the-list item")]
    public Dialog showFail_Dial;  //when you show a not-in-the-list item

    [Header("Visual")]
    [SerializeField] private Transform visual_parent;
    [SerializeField] private Animator animator;
    public List<Visual_Position> visuals = new List<Visual_Position>();
    [Sirenix.OdinInspector.InlineButton("FillWithCurrentPos", "Fill")]
    [Sirenix.OdinInspector.InlineButton("ChangeVisual", "Change")]
    public int visualIndex = 0;
    private int visualIndex_mem = 0;
    [System.Serializable]
    public struct Visual_Position
    {
        //in it : visual 
        public List<GameObject> toTurnOn; //so , we turn off when changing.
        public Transform newVisualPosition;
        public Transform newActionButton;
        public Transform newInteractZone;
        public Transform newCameraDefaut;
    }
    public pnj_face face = null;

    public List<Transform> cameraPoints = new List<Transform>();


    [Header("Action Button")]
    [SerializeField] private Transform actionButt_parent;
    [SerializeField] private bool actionButt_On = false;
    [SerializeField] private float actionButt_Val = 0;
    [SerializeField] private float actionButt_speed = 3;
    private Coroutine actionButt_Coroutine;
    [SerializeField] private SpriteRenderer actionButt_visual_key;
    [SerializeField] private SpriteRenderer actionButt_visual_con;
    [SerializeField] private SpriteRenderer actionButt_visual_exc;
    [SerializeField] private Transform interactionZone_parent;

    [Header("SaveData")]
    [Sirenix.OdinInspector.ReadOnly()] public PNJ_SaveData saveData;
    [System.Serializable]
    public class PNJ_SaveData
    {
        //Info to save :
        public pnjID id;

        //visuals index 
        public int visualsIndex = 0;

        //nextDial List
        //current idleDial 
        public string idleDial_Name;
        public List<string> nextDial_Names;
        public List<int> nextDial_Priority;

        public PNJ_SaveData(pnjID _id, int _visualsIndex, Dialog _idleDial, List<Dialog_ToShow> _nextDial)
        {
            id = _id;
            visualsIndex = _visualsIndex;
            idleDial_Name = _idleDial.name;

            nextDial_Names = new List<string>();
            nextDial_Priority = new List<int>();
            foreach (Dialog_ToShow nextDial in _nextDial)
            {
                nextDial_Names.Add(nextDial.dialog.name);
                nextDial_Priority.Add(nextDial.priority);
            }
        }
    }


    [System.Serializable]
    public class ItemReaction
    {
        public itemID itemToReactFrom = itemID.none;

        public bool finalTarget = false;//can give, it take
        [Sirenix.OdinInspector.HideIf("@this.finalTarget || this.responseGive == null")]
        public bool redirection = false;//some special data
        [Sirenix.OdinInspector.ShowIf("finalTarget")]
        public AudioClip musicGiveCorrect;
        public Dialog responseGive; //most of the time, just redirect after a line when not final target
        public Dialog responseShow;

#if UNITY_EDITOR
        [Sirenix.OdinInspector.HorizontalGroup][Sirenix.OdinInspector.Button]
        public void FillNeededDialog()
        {
            GameObject currentGO = UnityEditor.Selection.activeGameObject;
            pnj currentPNJ = currentGO.GetComponent<pnj>();

            //TO DO : follow the nomenclature :
            // - item/ITEMID/[item]_[pnj]_[Give||Show].asset
            string pnjName = currentPNJ.id.ToString();
            if (currentPNJ.id == pnjID.None)
                pnjName = currentPNJ.gameObject.name;

            Dialog asset1 = ScriptableObject.CreateInstance<Dialog>();
            asset1.name = itemToReactFrom.ToString() + "_" + pnjName + "_Give.asset";
            Dialog asset2 = ScriptableObject.CreateInstance<Dialog>();
            asset2.name = itemToReactFrom.ToString() + "_" + pnjName + "_Show.asset";
            
            string filePath = "Assets/Data/Dialog/item/" + itemToReactFrom.ToString() + "/" + asset1.name;
            if (System.IO.File.Exists(filePath))
            {
                responseGive = (Dialog)UnityEditor.AssetDatabase.LoadAssetAtPath(filePath, typeof(Dialog));
            }

            filePath = "Assets/Data/Dialog/item/" + itemToReactFrom.ToString() + "/" + asset2.name;
            if (System.IO.File.Exists(filePath))
            {
                responseShow = (Dialog)UnityEditor.AssetDatabase.LoadAssetAtPath(filePath, typeof(Dialog));
            }
            
            UnityEditor.AssetDatabase.SaveAssets();
        }

        [Sirenix.OdinInspector.HorizontalGroup][Sirenix.OdinInspector.Button]
        public void CreateNeededDialog()
        {
            GameObject currentGO = UnityEditor.Selection.activeGameObject;
            pnj currentPNJ = currentGO.GetComponent<pnj>();

            //TO DO : follow the nomenclature :
            // - item/ITEMID/[item]_[pnj]_[Give||Show].asset
            string pnjName = currentPNJ.id.ToString();
            if (currentPNJ.id == pnjID.None)
                pnjName = currentPNJ.gameObject.name;

            Dialog asset1 = ScriptableObject.CreateInstance<Dialog>();
            asset1.name = itemToReactFrom.ToString() + "_" + pnjName + "_Give.asset";
            Dialog asset2 = ScriptableObject.CreateInstance<Dialog>();
            asset2.name = itemToReactFrom.ToString() + "_" + pnjName + "_Show.asset";

            if(!System.IO.Directory.Exists("Assets/Data/Dialog/item/" + itemToReactFrom.ToString() + "/"))
            {
                System.IO.Directory.CreateDirectory("Assets/Data/Dialog/item/" + itemToReactFrom.ToString() + "/");
            }

            string filePath = "Assets/Data/Dialog/item/" + itemToReactFrom.ToString() + "/" + asset1.name;
            if (!System.IO.File.Exists(filePath))
            {
                UnityEditor.AssetDatabase.CreateAsset(asset1, filePath);
            }
            else
            {
                asset1 = (Dialog)UnityEditor.AssetDatabase.LoadAssetAtPath(filePath, typeof(Dialog));
            }

            filePath = "Assets/Data/Dialog/item/" + itemToReactFrom.ToString() + "/" + asset2.name;
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
        visualIndex_mem = visualIndex;
        foreach (Transform gO in cameraPoints)
        {
            Camera cam = gO.GetComponent<Camera>();
            if (cam != null)
            {
                //maybe we will save the VOF if needed
                //Destroy(cam);
            }
        }
    }

    private void Update()
    {
        //for now here so we can have the "!" showing easily even when too far away
        ActionButt_UpdateVisual();
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
        GameManager.instance.mapAndPaper.sideTab.AddIconIfNeeded(this.id);

        //Will need to have a way to make a smooth change and not a brutal like that (thirdaries ?)
        bool defaultDial = nextDialog.Count == 0;
        GameManager.instance.dialogMng.StartDialog(GetDialogToShow(), defaultDial, this);
        StartCameraForDialog();
    }

    public void StartCameraForDialog()
    {
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

    private void ChangeVisual()
    {
        ChangeVisual(visualIndex);
    }
    public void ChangeVisual(int index)
    {
        if (index == visualIndex_mem)
            return; //No need
        if (index < 0 || index >= visuals.Count)
        {
            Debug.LogError("Try change visual of " + id + " to index " + index + " but only have " + visuals.Count + " visual.");
            visualIndex = Mathf.Clamp(index, 0, visuals.Count);
        }
        if (visuals.Count == 0)
        {
            visualIndex_mem = visualIndex;
            return;
        }

        foreach(GameObject gO in visuals[visualIndex_mem].toTurnOn) 
        {
            gO.SetActive(false);
        }

        foreach (GameObject gO in visuals[index].toTurnOn)
        {
            gO.SetActive(true);
        }
        
        visual_parent.position              = visuals[index].newVisualPosition.transform.position;
        visual_parent.rotation              = visuals[index].newVisualPosition.transform.rotation;
        visual_parent.localScale            = visuals[index].newVisualPosition.transform.localScale;

        interactionZone_parent.position     = visuals[index].newInteractZone.transform.position;
        interactionZone_parent.rotation     = visuals[index].newInteractZone.transform.rotation;
        interactionZone_parent.localScale   = visuals[index].newInteractZone.transform.localScale;

        actionButt_parent.position          = visuals[index].newActionButton.transform.position;
        actionButt_parent.rotation          = visuals[index].newActionButton.transform.rotation;
        actionButt_parent.localScale        = visuals[index].newActionButton.transform.localScale;

        cameraPoints[0].position            = visuals[index].newCameraDefaut.transform.position; 
        cameraPoints[0].rotation            = visuals[index].newCameraDefaut.transform.rotation; 
        cameraPoints[0].localScale          = visuals[index].newCameraDefaut.transform.localScale; 


        visualIndex = index;
        visualIndex_mem = visualIndex;

        //if (Application.isPlaying)
        //    StartCameraForDialog();

    }

    public void FillWithCurrentPos()
    {
        if (visuals.Count == 0)
            return;
        
        visuals[visualIndex].newVisualPosition.transform.position     =   visual_parent.position              ;
        visuals[visualIndex].newVisualPosition.transform.rotation     =   visual_parent.rotation              ;
        visuals[visualIndex].newVisualPosition.transform.localScale   =   visual_parent.localScale            ;
                                                             
        visuals[visualIndex].newInteractZone.transform.position       =   interactionZone_parent.position     ;
        visuals[visualIndex].newInteractZone.transform.rotation       =   interactionZone_parent.rotation     ;
        visuals[visualIndex].newInteractZone.transform.localScale     =   interactionZone_parent.localScale   ;
                                                              
        visuals[visualIndex].newActionButton.transform.position       =   actionButt_parent.position          ;
        visuals[visualIndex].newActionButton.transform.rotation       =   actionButt_parent.rotation          ;
        visuals[visualIndex].newActionButton.transform.localScale     =   actionButt_parent.localScale        ;
                                                              
        visuals[visualIndex].newCameraDefaut.transform.position       =   cameraPoints[0].position            ; 
        visuals[visualIndex].newCameraDefaut.transform.rotation       =   cameraPoints[0].rotation            ;
        visuals[visualIndex].newCameraDefaut.transform.localScale     =   cameraPoints[0].localScale          ; 

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
            yield return new WaitForSeconds(1f / 60f);
        }
    }
    
    private void ActionButt_UpdateVisual()
    {
        actionButt_visual_con.color = Color.Lerp(Color.white-Color.black, Color.white, actionButt_Val);
        actionButt_visual_key.color = Color.Lerp(Color.white-Color.black, Color.white, actionButt_Val);

        float computeDeltaTime = 1 - Mathf.Pow(0.5f, Time.deltaTime * 3);
        actionButt_visual_exc.color = Color.Lerp(actionButt_visual_exc.color, (nextDialog.Count == 0 ? Color.white - Color.black : Color.white), computeDeltaTime);
    }



    public PNJ_SaveData GetSaveData()
    {
        PNJ_SaveData data = new PNJ_SaveData(id, visualIndex, defaultDialog, nextDialog);

        return data;
    }
    public void LoadData(PNJ_SaveData dataToLoad)
    {
        //Visuals
        ChangeVisual(dataToLoad.visualsIndex);

        //Idle dial and next dial 
        defaultDialog = GameManager.instance.dialogMng.GetDialByName(dataToLoad.idleDial_Name);
        nextDialog = new List<Dialog_ToShow>();
        for (int i = 0; i < dataToLoad.nextDial_Names.Count; i++)
        {
            string name = dataToLoad.nextDial_Names[i];
            Dialog dial = GameManager.instance.dialogMng.GetDialByName(name);
            int index = dataToLoad.nextDial_Priority[i];
            nextDialog.Add(new Dialog_ToShow(dial, index));
        }
        //Maybe sort again by priority


    }


#if UNITY_EDITOR
    [Sirenix.OdinInspector.Button]
    public void CreateDefaultDialog()
    {
        GameObject currentGO = UnityEditor.Selection.activeGameObject;
        pnj currentPNJ = currentGO.GetComponent<pnj>();

        if (!System.IO.Directory.Exists("Assets/Data/Dialog/" + currentPNJ.id + "/"))
        {
            System.IO.Directory.CreateDirectory("Assets/Data/Dialog/" + currentPNJ.id + "/");
        }

        if (!System.IO.File.Exists("Assets/Data/Dialog/" + currentPNJ.id + "/" + currentPNJ.id + "_DefaultDialog.asset"))
        {
            Dialog asset1 = ScriptableObject.CreateInstance<Dialog>();
            asset1.name = currentPNJ.id + "_DefaultDialog.asset";
            UnityEditor.AssetDatabase.CreateAsset(asset1, "Assets/Data/Dialog/" + currentPNJ.id + "/" + asset1.name);

            defaultDialog = asset1;
            UnityEditor.Selection.activeObject = asset1;
        }
        else
        {
            defaultDialog = (Dialog) UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Data/Dialog/" + currentPNJ.id + "/" + currentPNJ.id + "_DefaultDialog.asset", typeof(Dialog));
        }

        

        if (!System.IO.File.Exists("Assets/Data/Dialog/" + currentPNJ.id + "/" + currentPNJ.id + "_GiveFail.asset"))
        {
            Dialog asset2 = ScriptableObject.CreateInstance<Dialog>();
            asset2.name = currentPNJ.id + "_GiveFail.asset";
            UnityEditor.AssetDatabase.CreateAsset(asset2, "Assets/Data/Dialog/" + currentPNJ.id + "/" + asset2.name);

            giveFail_Dial = asset2;
            UnityEditor.Selection.activeObject = asset2;
        }
        else
        {
            giveFail_Dial = (Dialog)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Data/Dialog/" + currentPNJ.id + "/" + currentPNJ.id + "_GiveFail.asset", typeof(Dialog));
        }

        if (!System.IO.File.Exists("Assets/Data/Dialog/" + currentPNJ.id + "/" + currentPNJ.id + "_ShowFail.asset"))
        {
            Dialog asset2 = ScriptableObject.CreateInstance<Dialog>();
            asset2.name = currentPNJ.id + "_ShowFail.asset";
            UnityEditor.AssetDatabase.CreateAsset(asset2, "Assets/Data/Dialog/" + currentPNJ.id + "/" + asset2.name);

            showFail_Dial = asset2;
            UnityEditor.Selection.activeObject = asset2;
        }
        else
        {
            showFail_Dial = (Dialog)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Data/Dialog/" + currentPNJ.id + "/" + currentPNJ.id + "_ShowFail.asset", typeof(Dialog));
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
        contatenatedDialog.Add(giveFail_Dial);
        contatenatedDialog.Add(showFail_Dial);
        CreateCSV.WriteDialogInCVS("Assets/Resources/CSV/Basic_" + id + ".csv", contatenatedDialog);
        
        Debug.Log("Finish exporting CSV for " + id);
    }



#endif

}
