using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;

//Deal with the UI
public class DialogManager : MonoBehaviour
{
    public bool inDialog = false;
    public bool canClick = true;
    public bool giveSuspens = false;

    public List<pnj> allPNJ = new List<pnj>();
    [Sirenix.OdinInspector.ReadOnly] public List<Dialog> allDialog;
    [Sirenix.OdinInspector.ReadOnly] public Dictionary<string, Dialog> allDialogDico;

    [Header("UI")]
    public List<DialogBox> dialogTexts;
    [Sirenix.OdinInspector.ReadOnly] private int dialogText_currIndex = -1;
    public Animator dialogAnimator;
    public CanvasGroup dialogCanvas;
    public Animator buttonAnimator;
    public Volume blackWhite;

    [Header("Add item")]
    public Animator _addItem_anim;
    public Image _addItem_icon;
    public TMPro.TMP_Text _addItem_text;

    [Header("Choice")]
    private Step.Step_Choice choiceInMemory;
    public Vector3 choiceEmbranchement = Vector3.zero;

    [Header("Current dialog")]
    public Dialog currentDialog;
    public int currentStep = 0;
    public pnj currentPNJ = null;
    public pnj.pnjID lastTalkingPNJ = pnj.pnjID.None;
    private pnj closestPNJ = null;
    public Dialog errorDialog;
    public bool loadingDialogBox = false;
    public bool displayDialogText = false;

    public bool inventoryBlock = false;


    public void Start()
    {
        //Just in case, could be delete when finishing the game to lighten the start load
        FillAllPNJ(); 
        FillAllDialogDico();
    }

    [Sirenix.OdinInspector.Button()]
    public void FillAllPNJ()
    {
        for (int i = 0; i < allPNJ.Count; i++)
        {
            if (allPNJ[i] != null)
                continue;

            allPNJ.RemoveAt(i);
            i--;
        }

        foreach (pnj newPNJ in FindObjectsOfType<pnj>())
        {
            if(!allPNJ.Contains(newPNJ))
                allPNJ.Add(newPNJ);
        }
    }

    public void IM_ReachClosestOne()
    {
        pnj closestOne = null;
        float distMin = 1000f;
        Vector3 playerPos = GameManager.instance.playerMove.transform.position;
        foreach (pnj interactivePNJ in allPNJ)
        {
            if (!interactivePNJ.playerOnReach)
                continue;
            float distance = (interactivePNJ.transform.position - playerPos).magnitude;
            if (distMin > distance)
            {
                closestOne = interactivePNJ;
                distMin = distance;
            }
        }

        if(closestOne == closestPNJ)
            return;

        if(closestPNJ != null)
        {
            closestPNJ.TurnActionOnOrOff(false);
        }
        if(closestOne != null)
        {
            closestOne.TurnActionOnOrOff(true);
        }
        closestPNJ = closestOne;
    }
    public void IM_World()
    {
        if (closestPNJ != null)
        {
            closestPNJ.ReturnUpdate();
        }
    }
    public void IM_Dialog()
    {
        Click();
    }

    public void Click()
    {
        if(!canClick)
            return;
        if (loadingDialogBox)
            return;

        if (dialogTexts[dialogText_currIndex].Printing())
        {
            dialogTexts[dialogText_currIndex].SkipPrinting();
            return;
        }

        NextStep();
    }


    public void Update()
    {
        if (!inDialog)
            return;


        float alphaValue = 1f;
        Vector3 positionValue = Screen.width * 0.5f * Vector3.left;

        if (GameManager.instance.mapAndPaper.mapOpen)
        {
            float lowerDialValue = 0.25f;
            float underMouseValue = 0.14f;
            if (Input.mousePosition.y > Screen.height * lowerDialValue)
            {
                //if the mouse is on the lower part of the screen : up ! else : down.
                alphaValue = 0.6f;
                positionValue = Screen.height * lowerDialValue * Vector3.down + Screen.width * 0.5f * Vector3.left;
            }
            else
            {
                //if the mouse is on the lower part of the screen : up ! else : down.
                alphaValue = 0.8f;
                positionValue = Screen.height * underMouseValue * Vector3.down + Screen.width * 0.5f * Vector3.left;
            }
        }
        else if (GameManager.instance.inventory.inventoryDeployed)
        {
            //Also, lower the whole a little
            alphaValue = 0.8f;
        }

        //Also, set back to start pos
        dialogCanvas.alpha = Mathf.Lerp(dialogCanvas.alpha, alphaValue, Time.deltaTime * 2);
        dialogCanvas.transform.localPosition = Vector3.Lerp(dialogCanvas.transform.localPosition, positionValue, Time.deltaTime * 2);
    }


    public void StartDialog(Dialog dialog, bool inventoryToggle = false, pnj pnj = null)
    {
        if(dialog.allSteps.Count == 0)
        {
            Debug.LogError("Dialog " + dialog.name + " is empty. ", dialog);
            //cool to use a default dialog
            dialog = errorDialog;
        }

        //maybe not that effect ? espcially if it's the basic response like : idle, give Fail or show Fail.
        //blackWhite.weight = dialog.IsAlreadyRead() ? 1 : 0;
        inventoryBlock = !inventoryToggle; //we block inventory if we send us "false" for toggling the inventory

        GameManager.instance.pnjManager.StartDialog();

        currentDialog = dialog;
        if (pnj != null)
            currentPNJ = pnj;
        currentStep = -1;
        choiceEmbranchement = Vector3.zero;
        NextStep();

        if (closestPNJ != null)
        {
            closestPNJ.TurnActionOnOrOff(false);
        }

        GameManager.instance.playerMove.Talk();
        dialogAnimator.SetBool("Open", true);
        inDialog = true;

        //en vrai, need a delay before you can act/type, so you don't pass the text by accident ! 
        //and when it show the full text, can't be pass before X seconds

        loadingDialogBox = false;
    }
    
    public void NextStep()
    {
        if (giveSuspens)
        {
            return; //can't progress while waiting to see if give or not
        }

        Debug.Log("Next step : process ( " + currentDialog.allSteps.Count + " == " + (currentStep + 1) + ")");
        if (currentDialog.allSteps.Count == currentStep + 1)
        {
            /*Debug.LogError("Finish Dialog");*/
            FinishDialog();
            return;
        }

        currentStep++;

        if(choiceEmbranchement != Vector3.zero)
        {
            if(choiceEmbranchement.y + 1 == currentStep)
            {
                Debug.Log("Choice finish and redirect");
                currentStep = (int)choiceEmbranchement.z;
            }
        }

        //Debug.Log("next step : done." + currentStep);
        TreatDepending(currentDialog, currentStep);
    }

    public void TreatDepending(Dialog dialog, int index)
    {
        switch (dialog.allSteps[index].type)
        {
            case Step.stepType.dialog:
                TreatText((Step.Step_Dialog) dialog.allSteps[index].GetData());
                break;
            case Step.stepType.camera:
                TreatCamera((Step.Step_Camera)dialog.allSteps[index].GetData());
                NextStep();
                break;
            case Step.stepType.additem:
                GameManager.instance.inventory.AddItem((Step.Step_AddItem)dialog.allSteps[index].GetData());
                StartCoroutine(AddItem((Step.Step_AddItem)dialog.allSteps[index].GetData()));
                break;
            case Step.stepType.remitem:
                GameManager.instance.inventory.RemItem((Step.Step_RemItem)dialog.allSteps[index].GetData());
                NextStep();
                break;
            case Step.stepType.iteminteractivity:
                CanOpenInventory((Step.Step_ItemInteractivity)dialog.allSteps[index].GetData());
                NextStep();
                break;
            case Step.stepType.dialogredirection:
                DialogRedirection((Step.Step_DialogRedirection)dialog.allSteps[index].GetData());
                //the "start dialog" will habndle the "next step"
                break;
            case Step.stepType.setdefaultdialog:
                SetDefaultDialog((Step.Step_SetDefaultDialog)dialog.allSteps[index].GetData());
                NextStep();
                break;
            case Step.stepType.setnextdialog:
                SetNextDialog((Step.Step_SetNextDialog)dialog.allSteps[index].GetData());
                NextStep();
                break;
            case Step.stepType.choice:
                DisplayChoice((Step.Step_Choice)dialog.allSteps[index].GetData());
                break;
            case Step.stepType.changeface:
                ChangePNJFace((Step.Step_ChangeFace)dialog.allSteps[index].GetData());
                NextStep();
                break;
            case Step.stepType.animation:
                LaunchAnimation((Step.Step_Animation)dialog.allSteps[index].GetData());
                NextStep();
                break;
            case Step.stepType.changevisual:
                ChangeVisual((Step.Step_ChangeVisual)dialog.allSteps[index].GetData());
                NextStep();
                break;
            case Step.stepType.unlockpaper:
                UnlockPaper((Step.Step_UnlockPaper)dialog.allSteps[index].GetData());
                NextStep();
                break;
            case Step.stepType.zonechange:
                ChangeZone((Step.Step_ZoneChange)dialog.allSteps[index].GetData());
                NextStep();
                break;
            default:
                Debug.LogError("Did not implement correct value for step type " + dialog.allSteps[index].type);
                NextStep();
                break;
        }

        dialog.allSteps[index].alreadyRead = true;
    }

    public void TreatText(Step.Step_Dialog data)
    {
        if (data.text.Trim() == "")
        {
            NextStep();
            return;
        }

        //Make every box goes to next step (the available one will skip this by themself)
        foreach (DialogBox dialogBox in dialogTexts)
        {
            dialogBox.Next();
        }
        int dialogText_lastIndex = (dialogText_currIndex == -1 ? 0 : dialogText_currIndex);
        dialogText_currIndex++;
        if (dialogText_currIndex == dialogTexts.Count)
            dialogText_currIndex -= dialogTexts.Count;
        DialogBox currentText = dialogTexts[dialogText_currIndex];

        if(currentPNJ == null || data.text.Trim()[0] == '<')
        {
            currentText.Open(data.text, Color.black, "");
            lastTalkingPNJ = pnj.pnjID.None;
            currentPNJ.LineEnd();
            return;
            //no people talking so don't need the rest.
        }
        else
        {
            pnj.pnjID currentlyTalkingPNJ = (data.pnj_override == pnj.pnjID.None ? currentPNJ.id : data.pnj_override);
            string pnjTitle = dialogTexts[dialogText_lastIndex].GetCurrentTitle();
            Color pnjCol = dialogTexts[dialogText_lastIndex].GetCurrentColor();
            if (lastTalkingPNJ != currentlyTalkingPNJ)
            {
                //if not the same pnj than last one : load info 
                IconData savedData = GameManager.instance.mapAndPaper.sideTab.GetDataForThisPJN(currentlyTalkingPNJ);
                pnjTitle = savedData.nameText;
                //Debug.Log("title = " + pnjTitle);
                pnjCol = savedData.defaultColor;
                lastTalkingPNJ = currentlyTalkingPNJ;
            }
            if (data.color_override != Color.clear)
            {
                pnjCol = data.color_override;
            }


            currentText.Open(data.text, pnjCol, pnjTitle); //keep the same attribute
        }

        if (data.pnj_override != pnj.pnjID.None)
            GetPNJFromID(data.pnj_override).LineStart();
        if (currentPNJ != null && currentPNJ.id != pnj.pnjID.None)
            currentPNJ.LineStart();
    }

    public void FinishTalk()
    {
        if (currentPNJ != null && lastTalkingPNJ == currentPNJ.id)
        {
            currentPNJ.LineEnd();
        }
        else if (lastTalkingPNJ != pnj.pnjID.None)
        {
            GetPNJFromID(lastTalkingPNJ).LineEnd();
        }
    }


    public void TreatCamera(Step.Step_Camera data)
    {
        if(data.cameraIndex == 0)
        {
            GameManager.instance.cameraMng.UnSetThirdariesTarget();
        }
        else
        {
            //TO DO : deal with error if data.cameraIndex > cameraPoints.count
            GameManager.instance.cameraMng.SetThirdariesTarget(currentPNJ.cameraPoints[data.cameraIndex]/*, data.directTP*/);
        }
    }

    public IEnumerator AddItem(Step.Step_AddItem item)
    {
        Item it = GameManager.instance.inventory.GetItem(item.itemId);
        _addItem_icon.sprite = it.icon;
        _addItem_text.text = it.nameDisplay;
        _addItem_anim.SetTrigger("Play");
        canClick = false;
        bool tmp_inventoryBlock = inventoryBlock;
        inventoryBlock = true;

        yield return new WaitForSeconds(2.2f);
        canClick = true;
        inventoryBlock = tmp_inventoryBlock;
        NextStep();
    }

    public void CanOpenInventory(Step.Step_ItemInteractivity itemInteraciv)
    {
        //TO DO : 
        if(GameManager.instance.inventory.inventoryDeployed)
        {
            GameManager.instance.inventory.Retract();
        }
        inventoryBlock = !itemInteraciv.itemInvoCanBeOpen;
    }

    public void LaunchAnimation(Step.Step_Animation data)
    {
        pnj target = GetPNJFromID(data.targetID);
        if (target == null)
        {
            Debug.LogError("No pnj for " + data.targetID);
            return;
        }

        target.LaunchAnimation(data.animIndex);
    }

    public void ChangePNJFace(Step.Step_ChangeFace data)
    {
        pnj target = GetPNJFromID(data.targetID);
        if (target == null)
        {
            Debug.LogError("No pnj for " + data.targetID);
            return;
        }

        target.ChangeFace(data.eyesIndex, data.mouthIndex);
    }

    public void ChangeVisual(Step.Step_ChangeVisual data)
    {
        pnj target = GetPNJFromID(data.targetID);
        if (target == null)
        {
            Debug.LogError("No pnj for " + data.targetID);
            return;
        }

        target.ChangeVisual(data.visualIndex);
    }

    public void UnlockPaper(Step.Step_UnlockPaper data)
    {
        Debug.Log("Unlock paper n° " + data.papersIndex);
        GameManager.instance.mapAndPaper.UnlockPaper(data.papersIndex);
    }

    public void ChangeZone(Step.Step_ZoneChange data)
    {
        Debug.Log("Change zone " + data.zoneName + " turned " + (data.change ? "ON." : "OFF."));
        GameManager.instance.pnjManager.ChangeZoneStatus(data.zoneName, data.change);
    }

    public void SetDefaultDialog(Step.Step_SetDefaultDialog data)
    {
        pnj target = GetPNJFromID(data.targetID);
        if (target == null)
        {
            Debug.LogError("No pnj for " + data.targetID);
            return;
        }

        Debug.Log("will set new default");
        target.defaultDialog = data.newDefaultDial;
        Debug.Log("new default !");
    }
    public void SetNextDialog(Step.Step_SetNextDialog data)
    {
        pnj target = GetPNJFromID(data.targetID);
        if (target == null)
        {
            Debug.LogError("No pnj for " + data.targetID);
            return;
        }

        Debug.Log("will add new dialog to use");
        target.AddNextDialog(data.dialToAdd, data.priority);
    }

    public void DialogRedirection(Step.Step_DialogRedirection data)
    {
        Debug.Log("New dialog : "+data.dialogToGo.name);
        StartDialog(data.dialogToGo, false, currentPNJ);
        //Seems that's it. Nothing else.
    }

    public void DisplayChoice(Step.Step_Choice data) 
    {
        buttonAnimator.SetBool("Button", true);
        inventoryBlock = true;
        Cursor.lockState = CursorLockMode.None;

        choiceInMemory = data;
        canClick = false;
    }

    public void PressButton(bool yes)
    {
        Debug.Log("PRESS " + (yes ? "yes" : "no"));
        if (choiceInMemory == null)
            return;
        Step.Step_Choice ch = choiceInMemory;
        if (yes)
            buttonAnimator.SetTrigger("Yes");
        else
            buttonAnimator.SetTrigger("No");
        buttonAnimator.SetBool("Button", false);
        canClick = true;
        Cursor.lockState = CursorLockMode.Locked;

        switch (yes ? ch.typeYes : ch.typeNo)
        {
            case Step.Step_Choice.choiceType.dialogUnique:
                TreatText(yes ? ch.dialogYes : ch.dialogNo);
                break;
            case Step.Step_Choice.choiceType.redirectNumber:
                choiceEmbranchement = new Vector3(
                    yes ? ch.redirectNumberIfYes    : ch.redirectNumberIfNo   ,
                    yes ? ch.redirectNumberStopYes  : ch.redirectNumberStopNo ,
                    yes ? ch.redirectNumberAfterYes : ch.redirectNumberAfterNo);
                //Go to this step :
                if(currentStep >= choiceEmbranchement.x)
                {
                    Debug.LogError("Choice at " + currentStep + " in " + currentDialog.name + " have redirection BEFORE the choice.");
                    NextStep();
                }
                else
                {
                    currentStep = (int)choiceEmbranchement.x - 1;//To test
                    NextStep();
                }
                break;
            case Step.Step_Choice.choiceType.redirectDialog:
                DialogRedirection(yes ? ch.redirectYes : ch.redirectNo);
                break;
            default:
                break;
        }


        choiceInMemory = null;
    }

    public void FinishDialog()
    {
        StartCoroutine(CloseDialog());
    }

    public IEnumerator CloseDialog()
    {
        blackWhite.weight = 0;
        GameManager.instance.pnjManager.FinishDialog();

        if (closestPNJ != null)
        {
            closestPNJ.TurnActionOnOrOff(true);
        }

        canClick = true;
        inDialog = false;
        currentPNJ = null;
        dialogAnimator.SetBool("Open", false);
        GameManager.instance.cameraMng.UnSetSecondaryTarget();
        GameManager.instance.cameraMng.UnSetThirdariesTarget();
        //May need a delay here
        foreach (DialogBox dialBox in dialogTexts)
        {
            dialBox.Close();
        }

        yield return new WaitForSeconds(0.1f);
        GameManager.instance.playerMove.FinishTalk();

        dialogText_currIndex = -1;

    }

    public pnj GetPNJFromID(pnj.pnjID id)
    {
        foreach (pnj potential in allPNJ)
        {
            if (potential.id == id)
            {
                return potential;
            }
        }
        return null;
    }


#if UNITY_EDITOR

    [Sirenix.OdinInspector.Button]
    void LoadAllDialog()
    {
        allDialog = new List<Dialog>();
        string largestPath = "Assets/Data/Dialog/";
        Debug.Log("Go seek in  " + largestPath);
        foreach (var filePath in Directory.EnumerateFiles(largestPath, "*.asset", SearchOption.AllDirectories))
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath.Trim());
            Debug.Log("What filename are you : " + fileName + " (fullPath = " + filePath + ")");
            //Get rid of the directory : only the filename ! (and test if it's a dialog, please, by the mother of god)

            Dialog dial = (Dialog)UnityEditor.AssetDatabase.LoadAssetAtPath(filePath, typeof(Dialog));
            if (dial != null)
                allDialog.Add(dial);
        }
        Debug.Log("Finish seek but found " + allDialog.Count + " files.");
    }
#endif

    private void FillAllDialogDico()
    {
        allDialogDico = new Dictionary<string, Dialog>();
        foreach (Dialog dial in allDialog)
        {
            allDialogDico.Add(dial.name, dial);
        }
    }

    [Sirenix.OdinInspector.Button]
    void InfoAllDialog()
    {
        Debug.Log("AllDialog " + (allDialog == null ? "is null." : "have " + allDialog.Count + " files."));
    }

    public Dialog GetDialByName(string name)
    {
        return allDialogDico[name];
    }

    public void InventoryOrMapOpen()
    {

    }
    public void InventoryOrMapClose()
    {

    }



    public void UpdateTitle(pnj.pnjID id, string newName)
    {
        if (inDialog)
        {
            if (lastTalkingPNJ == id)
            {
                DialogBox currentText = dialogTexts[dialogText_currIndex];
                currentText.UpdateTitle(newName);
            }
        }
        else
        {
            lastTalkingPNJ = pnj.pnjID.None;
            //so it's reset on the next change
        }
    }

}
