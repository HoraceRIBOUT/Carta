using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pnj : MonoBehaviour
{

    public bool playerOnReach;



    public void Update()
    {
        if (!playerOnReach)
            return;
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(!GameManager.instance.playerMove.talking) 
            {
                Talk();
            }
        }
    }

    public void Talk()
    {
        GameManager.instance.dialogMng.StartDialog();
        //Also to the UI and etc...
        Debug.Log("bla bla bla");
    }

}
