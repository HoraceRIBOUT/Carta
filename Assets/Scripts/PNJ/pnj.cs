using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pnj : MonoBehaviour
{

    public bool playerOnReach;

    public Dialog dialog;
    public List<Transform> cameraPoints = new List<Transform>();


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

        if (cameraPoints != null && cameraPoints.Count != 0)
            GameManager.instance.cameraMng.SetSecondaryTarget(cameraPoints[0]);
    }

}