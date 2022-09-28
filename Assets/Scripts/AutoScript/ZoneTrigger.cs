using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ZoneTrigger : MonoBehaviour
{
    public TextAsset script;
    public bool regenerate = false;
    public ZoneTrigger_AutoGeneratePart.ZoneTriggerType myType;

    public void Start()
    {
        if (GetComponent<ZoneTrigger_AutoGeneratePart>() == null)
            this.gameObject.AddComponent<ZoneTrigger_AutoGeneratePart>();
    }

    public void EnterBalcony()
    {
        Debug.Log("Balcony ! Yeah !");
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
