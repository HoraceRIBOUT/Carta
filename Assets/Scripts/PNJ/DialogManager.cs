using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Deal with the UI
public class DialogManager : MonoBehaviour
{
    public bool inDialog = false;

    public List<pnj> allPNJ = new List<pnj>();

    [Header("UI")]
    public TMPro.TMP_Text dialogText;
    public Animator dialogAnimator;

    [Header("Current dialog")]
    public Dialog currentDialog;
    public int currentStep = 0;
    public pnj currentPNJ = null;
    public bool loadingDialogBox = false;
    public bool displayDialogText = false;


    public void Start()
    {
        if(allPNJ == null || allPNJ.Count == 0)
        {
            FillAllPNJ();
        }
    }


    public void FillAllPNJ()
    {
        foreach (pnj interac in FindObjectsOfType<pnj>())
        {
            allPNJ.Add(interac);
        }
    }

    public void Update()
    {
        if (!inDialog)
        {
            foreach (pnj interactivePNJ in allPNJ)
            {
                if (interactivePNJ.ReturnUpdate())
                    break;
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Click();
        }
    }

    public void Click()
    {
        //if(cant continue)
        // return;
        if (loadingDialogBox)
            return;

        if (displayDialogText)
        {
            //Just display it totally, in one try
            return;
        }

        if(currentDialog.allSteps.Count == currentStep + 1)
        {
            FinishDialog();
        }
        else
        {
            NextStep();
        }
    }



    public void StartDialog(Dialog dialog, pnj pnj = null)
    {
        currentDialog = dialog;
        currentPNJ = pnj;
        currentStep = -1;
        NextStep();


        GameManager.instance.playerMove.Talk();
        dialogAnimator.SetBool("Open", true);
        inDialog = true;

        //en vrai, need a delay before you can act/type, so you don't pass the text by accident ! 
        //and when it show the full text, can't be pass before X seconds

        loadingDialogBox = false;
    }
    
    public void NextStep()
    {
        currentStep++;
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
                //TO DO
                //Probably an animation an then, goes next. Or wait for confirmation ? 
                //Like a box on center, confirmation make it on the side THEN goes next 
                //seems good
                break;
            case Step.stepType.remitem:
                //TO DO
                NextStep();
                break;
            default:
                break;
        }
    }

    public void TreatText(Step.Step_Dialog data)
    {
        //TO DO : add two text. When one sentence is finish, fade away the previous sentence. Then, open the second one, letter by letter
        dialogText.text = data.text;


        if(data.color_override.a > 0)
            dialogText.color = data.color_override;
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

    public void FinishDialog()
    {
        StartCoroutine(CloseDialog());
    }

    public IEnumerator CloseDialog()
    {
        inDialog = false;
        currentPNJ = null;
        dialogAnimator.SetBool("Open", false);
        GameManager.instance.cameraMng.UnSetSecondaryTarget();
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.playerMove.FinishTalk();
    }


    [Sirenix.OdinInspector.Button]
    public void ReloadEnumUtils()
    {
        Generate_StepEnum.ReadTheFile();
    }
}
