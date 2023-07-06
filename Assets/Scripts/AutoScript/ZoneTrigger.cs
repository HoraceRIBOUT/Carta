using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    public ZoneTrigger_AutoGeneratePart.ZoneTriggerType myType;

    public void Start()
    {
        if (GetComponent<ZoneTrigger_AutoGeneratePart>() == null)
            this.gameObject.AddComponent<ZoneTrigger_AutoGeneratePart>();
    }


    [Sirenix.OdinInspector.ShowIf("myType", ZoneTrigger_AutoGeneratePart.ZoneTriggerType.balcony)]
    public string balcony_id;
    [Sirenix.OdinInspector.ShowIf("myType", ZoneTrigger_AutoGeneratePart.ZoneTriggerType.balcony)]
    public Dialog balconyDialog;
    [Sirenix.OdinInspector.ShowIf("myType", ZoneTrigger_AutoGeneratePart.ZoneTriggerType.balcony)]
    public pnj balconyPNJ;

    public void EnterBalcony()
    {
        Debug.Log("Balcony ! Yeah !");
        //Play the dialog :
        GameManager.instance.dialogMng.StartDialog(balconyDialog, false, balconyPNJ);
        balconyPNJ.StartCameraForDialog();
        this.gameObject.SetActive(false);
    }
    public void ExitBalcony()
    {
        Debug.Log("Balcony ? no...");
    }

    public void EnterFountain()
    {
        Debug.Log("Oh, a fountain ?");
    }
    public void ExitFountain()
    {
        Debug.Log("No more fountain.");
    }

    public void EnterDream()
    {
        Debug.Log("It'a dream ! ?");
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
