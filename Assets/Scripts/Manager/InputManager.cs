using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    //Need to know if :
    //in dialog or not
    //  in inventory or not
    //  
    //else 
    //  in range of pnj


    // Update is called once per frame
    void Update()
    {
        ReturnUpdate();

        EUpdate();

        //For inventory : 

            //Out dialog :
            // just show the info on it
            // B : Go back

            //On dialog
            // A : show
            // B : go back
            // Y : give
    }

    void ReturnUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Return))
            GameManager.instance.dialogMng.ReturnUpdate();
    }

    void EUpdate()
    {
        //For now, always, no limit.
        if (Input.GetKeyDown(KeyCode.E))
            GameManager.instance.inventory.EUpdate();
    }
}
