using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class ZoneTrigger : MonoBehaviour
{
    public ZoneTrigger_AutoGeneratePart.ZoneTriggerType myType;

    public void Start()
    {
        if (GetComponent<ZoneTrigger_AutoGeneratePart>() == null)
            this.gameObject.AddComponent<ZoneTrigger_AutoGeneratePart>();
    }


    [ShowIf("@this.myType ==  ZoneTrigger_AutoGeneratePart.ZoneTriggerType.dialog || this.myType ==  ZoneTrigger_AutoGeneratePart.ZoneTriggerType.dialogandevents")]
    public string balcony_id;
    [ShowIf("@this.myType ==  ZoneTrigger_AutoGeneratePart.ZoneTriggerType.dialog || this.myType ==  ZoneTrigger_AutoGeneratePart.ZoneTriggerType.dialogandevents")]
    public Dialog balconyDialog;
    [ShowIf("@this.myType ==  ZoneTrigger_AutoGeneratePart.ZoneTriggerType.dialog || this.myType ==  ZoneTrigger_AutoGeneratePart.ZoneTriggerType.dialogandevents")]
    public pnj balconyPNJ;


    [ShowIf("@this.myType ==  ZoneTrigger_AutoGeneratePart.ZoneTriggerType.events || this.myType ==  ZoneTrigger_AutoGeneratePart.ZoneTriggerType.dialogandevents")]
    public UnityEvent eventEnter;
    [ShowIf("@this.myType ==  ZoneTrigger_AutoGeneratePart.ZoneTriggerType.events || this.myType ==  ZoneTrigger_AutoGeneratePart.ZoneTriggerType.dialogandevents")]
    public UnityEvent eventExit;

    [ShowIf("typeYes", ZoneTrigger_AutoGeneratePart.ZoneTriggerType.knowledgecheckzone)]
    [SerializeField] Dialog knowledge_dialog;
    [ShowIf("typeYes", ZoneTrigger_AutoGeneratePart.ZoneTriggerType.knowledgecheckzone)]
    [SerializeField] int knowledge_number;

    public void EnterDialog()
    {
        Debug.Log("Balcony ! Yeah !");
        //Play the dialog :
        GameManager.instance.dialogMng.StartDialog(balconyDialog, false, balconyPNJ);
        balconyPNJ.StartCameraForDialog();
        this.gameObject.SetActive(false);
    }
    public void ExitDialog()
    {

    }

    public void EnterEvents()
    {
        eventEnter.Invoke();
    }
    public void ExitEvents()
    {
        eventExit.Invoke();
    }

    public void EnterDialogAndEvents()
    {
        EnterEvents();
        EnterDialog();
    }
    public void ExitDialogAndEvents()
    {
        ExitEvents();
    }

    public void EnterKnowledgeCheckZone()
    {
        Debug.Log("You unlock some knowledge.");
        knowledge_dialog.allSteps[knowledge_number].alreadyRead = true;
    }

    public void ExitRestaurant()
    {
        Debug.Log("No more restauran.");
    }

    public void EnterFlowerShop()
    {
        Debug.Log("Flower? Flower !");
    }
    public void ExitFlowerShop()
    {
        Debug.Log("No more flower ! What have we done");
    }
}
