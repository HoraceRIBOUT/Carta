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
    public TMPro.TMP_Text dialogText0;
    public TMPro.TMP_Text dialogText1;
    [Sirenix.OdinInspector.ReadOnly] private int dialogText_currIndex = -1;
    [SerializeField] private float dialogText_fadeOutSpeed = 5f;
    public Animator dialogAnimator;
    private Coroutine fadeDial = null;
    private Coroutine printDial = null;
    private string printText_inSkipCase = "";
    public float printDelay = 0.05f;
    public int nbrIndxGrad = 5;

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

        if (printDial != null)
        {
            Debug.Log("Skip apparition time");
            StopCoroutine(printDial);
            printDial = null;
            TMPro.TMP_Text currentText = (dialogText_currIndex == 0 ? dialogText0 : dialogText1);
            currentText.text = printText_inSkipCase;
            //Just display it totally, in one try
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
        choiceEmbranchement = Vector3.zero;
        NextStep();

        if(closestPNJ != null)
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
        //TO DO : add two text. When one sentence is finish, fade away the previous sentence. Then, open the second one, letter by letter
        if(dialogText_currIndex != -1)
        {
            if (fadeDial != null)
                StopCoroutine(fadeDial);
            fadeDial = StartCoroutine(FadeDialogText(dialogText_currIndex));
        }
        dialogText_currIndex++;
        dialogText_currIndex = dialogText_currIndex % 2;
        TMPro.TMP_Text currentText = (dialogText_currIndex == 0 ? dialogText0 : dialogText1);

        currentText.text = data.text;
        if(data.color_override != Color.clear)
            currentText.color = data.color_override;
        else
            currentText.color = currentPNJ.defaultColor;

        if (printDial != null)
            StopCoroutine(printDial);
        printDial = StartCoroutine(PrintDialogText(dialogText_currIndex, data.text));

        printText_inSkipCase = data.text;
    }

    private IEnumerator PrintDialogText(int index, string originalText)
    {
        Debug.Log("Print " + index + " start.");
        TMPro.TMP_Text currentText = (index == 0 ? dialogText0 : dialogText1);
        int charIndex = 0;

        float minDelay = 1f / 60f;
        while (charIndex < originalText.Length)
        {
            if (originalText[charIndex] == ' ')
            {
                charIndex++;
                continue;
            }
            //Set text : 
            //Add a gradient
            {
                string part1 = originalText.Substring(0, Mathf.Max(0,charIndex - nbrIndxGrad));
                string[] partList = new string[nbrIndxGrad];
                for (int i = 0; i < nbrIndxGrad; i++)
                {
                    int inv = nbrIndxGrad - i;
                    partList[i] = originalText.Substring(Mathf.Max(0, charIndex - inv), charIndex - inv < 0 ? 0 : 1);
                }
                string partFinal = originalText.Substring(Mathf.Max(0, charIndex - 0), originalText.Length - charIndex);

                Color startCol = currentText.color;
                Color endCol = currentText.color - Color.black;
                string[] colorList = new string[nbrIndxGrad]; 
                for (int i = 0; i < nbrIndxGrad; i++)
                {
                    int inv = nbrIndxGrad - i;
                    colorList[i] = ColorUtility.ToHtmlStringRGBA(Color.Lerp(startCol, endCol, i * (1f / (nbrIndxGrad + 1))));
                }
                string colorGradientFinal = ColorUtility.ToHtmlStringRGBA(endCol);

                System.Text.StringBuilder build = new System.Text.StringBuilder(part1);
                for (int i = 0; i < nbrIndxGrad; i++)
                {
                    build.Append("<color=#" + colorList[i] + ">" + partList[i] + "</color>");
                }
                build.Append("<color=#" + colorGradientFinal + ">" + partFinal + "</color>");
                currentText.text = build.ToString();
            }
            
            yield return new WaitForSeconds(Mathf.Max(minDelay, printDelay));
            //Jump more char if delay is < than Mindelay
            if(printDelay < minDelay)
            {
                charIndex += Mathf.RoundToInt(printDelay / minDelay);
            }
            else
            {
                charIndex++;
            }
        }
        Debug.Log("Print " + index + " finish.");
        currentText.text = originalText;
        printDial = null;
    }

    private IEnumerator FadeDialogText(int index)
    {
        Debug.Log("Fade " + index + " start.");
        TMPro.TMP_Text currentText = (index == 0 ? dialogText0 : dialogText1);
        float lerp = currentText.alpha;
        while (lerp > 0)
        {
            lerp -= Time.deltaTime * dialogText_fadeOutSpeed;
            currentText.alpha = lerp;
            yield return new WaitForSeconds(1 / 60f);
        }
        Debug.Log("Fade " + index + " finish.");
        currentText.alpha = 0;
        fadeDial = null;
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
        inventoryBlock = true;

        yield return new WaitForSeconds(2.2f);
        canClick = true;
        inventoryBlock = false;
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
        canClick = true;
        inDialog = false;
        currentPNJ = null;
        dialogAnimator.SetBool("Open", false);
        GameManager.instance.cameraMng.UnSetSecondaryTarget();
        if (fadeDial != null)
            StopCoroutine(fadeDial);
        StartCoroutine(FadeDialogText(0));
        StartCoroutine(FadeDialogText(1));
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.playerMove.FinishTalk();

        inventoryBlock = false;
        dialogText_currIndex = -1;
    }


}
