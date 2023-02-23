using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

//Deal with the UI
public class DialogManager : MonoBehaviour
{
    public bool inDialog = false;
    public bool canClick = true;

    public List<pnj> allPNJ = new List<pnj>();

    [Header("UI")]
    public List<DialogBox> dialogTexts;
    [Sirenix.OdinInspector.ReadOnly] private int dialogText_currIndex = -1;
    public Animator dialogAnimator;
    public CanvasGroup dialogCanvas;

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
    private pnj closestPNJ = null;
    public bool loadingDialogBox = false;
    public bool displayDialogText = false;

    public bool inventoryBlock = false;


    public void Start()
    {
    }

    [Sirenix.OdinInspector.Button()]
    public void FillAllPNJ()
    {
        allPNJ = new List<pnj>();
        foreach (pnj interac in FindObjectsOfType<pnj>())
        {
            allPNJ.Add(interac);
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



    public void StartDialog(Dialog dialog, pnj pnj = null)
    {
        currentDialog = dialog;
        if (pnj != null)
            currentPNJ = pnj;
        currentStep = -1;
        inventoryBlock = true; // by default, you can only give item during "default" dial
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
        if (currentDialog.allSteps.Count == currentStep + 1)
        {
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
                NextStep();
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
            default:
                Debug.LogError("Did not implement correct value for step type " + dialog.allSteps[index].type);
                NextStep();
                break;
        }

        dialog.allSteps[index].alreadyRead = true;
    }

    public void TreatText(Step.Step_Dialog data)
    {
        //Make every box goes to next step (the available one will skip this by themself)
        foreach (DialogBox dialogBox in dialogTexts)
        {
            dialogBox.Next();
        }
        dialogText_currIndex++;
        if (dialogText_currIndex == dialogTexts.Count)
            dialogText_currIndex -= dialogTexts.Count;
        DialogBox currentText = dialogTexts[dialogText_currIndex];

        if(data.color_override != Color.clear)
            currentText.Open(data.text, data.color_override);
        else
            currentText.Open(data.text, currentPNJ.defaultColor);
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

    public void SetDefaultDialog(Step.Step_SetDefaultDialog data)
    {
        pnj target = null;
        foreach (pnj potential in allPNJ)
        {
            if(potential.id == data.targetID)
            {
                target = potential;
                break;
            }
        }
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
        pnj target = null;
        foreach (pnj potential in allPNJ)
        {
            if (potential.id == data.targetID)
            {
                target = potential;
                break;
            }
        }
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
        StartDialog(data.dialogToGo, currentPNJ);
        //Seems that's it. Nothing else.
    }

    public void DisplayChoice(Step.Step_Choice data) 
    {
        dialogAnimator.SetBool("Button", true);
        inventoryBlock = true;
        Cursor.lockState = CursorLockMode.None;

        choiceInMemory = data;
        canClick = false;
    }

    public void PressButton(bool yes)
    {
        if (choiceInMemory == null)
            return;
        Step.Step_Choice ch = choiceInMemory;
        dialogAnimator.SetBool("Yes", yes);
        dialogAnimator.SetBool("Button", false);
        canClick = true;
        inventoryBlock = false;
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
        if (closestPNJ != null)
        {
            closestPNJ.TurnActionOnOrOff(true);
        }


        canClick = true;
        inDialog = false;
        currentPNJ = null;
        dialogAnimator.SetBool("Open", false);
        GameManager.instance.cameraMng.UnSetSecondaryTarget();
        //May need a delay here
        foreach (DialogBox dialBox in dialogTexts)
        {
            dialBox.Close();
        }

        yield return new WaitForSeconds(0.1f);
        GameManager.instance.playerMove.FinishTalk();

        inventoryBlock = false;
        dialogText_currIndex = -1;

    }



    public void InventoryOrMapOpen()
    {
        //Also, lower the whole a little
        dialogCanvas.alpha = 0.6f;
    }
    public void InventoryOrMapClose()
    {
        //Also, set back to start pos
        dialogCanvas.alpha = 1f;
    }

}
