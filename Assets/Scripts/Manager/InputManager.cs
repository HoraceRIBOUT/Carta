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
            Try_MoveInventoryUpdate();
            Try_CloseInventory();
            Try_GiveInventory();
            Try_ShowInventory();
            //First case
            //With variable around weither is in dialog or not
            //and if the current item is already given or not
        }
        else if (GameManager.instance.dialogMng.inDialog)
        {
            if(!GameManager.instance.dialogMng.inventoryBlock)
                Try_OpenInventory();
            Try_ValidateDialog();

            //Second case :
            //Can continue text
            //Can choose a choice when it's on
            //Can quit dialog rudly
            //Can open inventory
            //and... that's it ?
        }
        else
        {
            Try_OpenInventory();
            Try_TalkToPnj();

            //Dealt within PlayerMove.cs : 
                //Crouching
                //Moving
                //Jumping

            //In movement in the real world else !
        }

        //Will need a fourth choice : the UI one. A big one. With so many case. An hellscape just for myself. O pity me, why giving me such monstruous task! I will turn into moby dick and chase myself indefinitly in a see of green letter and black space, with no star and no guiding light


        //For inventory : 

            //Out dialog :
            // just show the info on it
            // B : Go back

            //On dialog
            // A : show
            // B : go back
            // Y : give
    }

    void Try_MoveInventoryUpdate()
    {
        Vector2 inputDirection = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
            );

        //Also the player
        //Also the choice in dialog

        GameManager.instance.inventory.IM_MoveUpDown(inputDirection);
    }

    void Try_TalkToPnj()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            GameManager.instance.dialogMng.IM_World();
        }
    }
    void Try_ValidateDialog()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            GameManager.instance.dialogMng.IM_Dialog();
        }
    }

    void Try_OpenInventory()
    {
        //For now, always, no limit.
        if (Input.GetKeyDown(KeyCode.E))
            GameManager.instance.inventory.IM_Open();
    }

    void Try_CloseInventory()
    {
        //For now, always, no limit.
        if (Input.GetKeyDown(KeyCode.E))
            GameManager.instance.inventory.IM_Close();
    }

    void Try_GiveInventory()
    {
        //For now, always, no limit.
        if (Input.GetKeyDown(KeyCode.Return))
            GameManager.instance.inventory.IM_Give();
    }
    void Try_ShowInventory()
    {
        //For now, always, no limit.
        if (Input.GetKeyDown(KeyCode.Space))
            GameManager.instance.inventory.IM_Show();
    }
}
