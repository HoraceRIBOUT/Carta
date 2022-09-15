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
        //Three state for now :

        if (GameManager.instance.inventory.inventoryDeployed)
        {
            //First case
            //With variable around weither is in dialog or not
            //and if the current item is already given or not
        }
        else if (GameManager.instance.dialogMng.inDialog)
        {
            ReturnUpdate();

            //Second case :
            //Can continue text
            //Can choose a choice when it's on
            //Can quit dialog rudly
            //Can open inventory
            //and... that's it ?
        }
        else
        {
            ReturnUpdate();
            //In movement in the real world else !
        }

        //Will need a fourth choice : the UI one. A big one. With so many case. An hellscape just for myself. O pity me, why giving me such monstruous task! I will turn into moby dick and chase myself indefinitly in a see of green letter and black space, with no star and no guiding light

        MoveUpdate();


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

    void MoveUpdate()
    {
        Vector2 inputDirection = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
            );

        //Also the player
        //Also the choice in dialog

        GameManager.instance.inventory.InputManagement_MoveUpDown(inputDirection);
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
