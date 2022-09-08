using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pnj : MonoBehaviour
{

    public bool playerOnReach;

    public Dialog dialog;
    public Transform cameraPoint;


    public bool ReturnUpdate()
    {
        //Better if take care in a "control manager" and compare to "dialogManager" too
        if (!playerOnReach)
            return false;
        
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if(!GameManager.instance.playerMove.talking) 
            {
                Talk();
                return true;
            }
        }
        return false;
    }

    public void Talk()
    {
        GameManager.instance.dialogMng.StartDialog(dialog, this);

        if(cameraPoint != null)
            GameManager.instance.cameraMng.SetSecondaryTarget(cameraPoint);
        //Also to the UI and etc...
        Debug.Log("bla bla bla");
    }

}
