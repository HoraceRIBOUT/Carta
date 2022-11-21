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
    public static bool controller = false;


    // Update is called once per frame
    void Update()
    {
        //Need to be add : pause.
        //Four state for now :
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
        else if (GameManager.instance.mapAndPaper.mapOpen)
        {
            if (!GameManager.instance.dialogMng.inventoryBlock)
                Try_OpenInventory();
            Try_CloseMapAndPaper();

        }
        else if (GameManager.instance.dialogMng.inDialog)
        {
            if(!GameManager.instance.dialogMng.inventoryBlock)
                Try_OpenInventory();
            Try_ValidateDialog();
            Try_OpenMapAndPaper();

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
            Try_OpenMapAndPaper();

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

        if(Mathf.Abs(Input.GetAxis("Horizontal")) > 0.3f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.3f)
        {
            bool joystick = (Input.GetAxis("Horizontal_joy") != 0 || Input.GetAxis("Vertical_joy") != 0);
            ChangeIconToCorrectDevice(joystick);
        }


        //if (Input.GetKeyDown(KeyCode.JoystickButton0))            Debug.Log("0");
        //if (Input.GetKeyDown(KeyCode.JoystickButton1))            Debug.Log("&");
        //if (Input.GetKeyDown(KeyCode.JoystickButton2))            Debug.Log("2");
        //if (Input.GetKeyDown(KeyCode.JoystickButton3))            Debug.Log("\"");
        //if (Input.GetKeyDown(KeyCode.JoystickButton4))            Debug.Log("'");
        //if (Input.GetKeyDown(KeyCode.JoystickButton5))            Debug.Log("(");
        //if (Input.GetKeyDown(KeyCode.JoystickButton6))            Debug.Log("6"); //back
        //if (Input.GetKeyDown(KeyCode.JoystickButton7))            Debug.Log("7");
        //if (Input.GetKeyDown(KeyCode.JoystickButton8))            Debug.Log("8");
        //if (Input.GetKeyDown(KeyCode.JoystickButton9))            Debug.Log("9");
        //if (Input.GetKeyDown(KeyCode.JoystickButton10))            Debug.Log("10");
        //if (Input.GetKeyDown(KeyCode.JoystickButton11))            Debug.Log("11");



            if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.JoystickButton2) || Input.GetKeyDown(KeyCode.JoystickButton3))
            ChangeIconToCorrectDevice(true);
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
            ChangeIconToCorrectDevice(false);
    }

    void Try_MoveInventoryUpdate()
    {
        Vector2 inputDirection = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
            );

        //Detect gamepad joystick
        //if not joystock : then mouse.


        //Also the player
        //Also the choice in dialog

        GameManager.instance.inventory.IM_MoveUpDown(inputDirection);
    }

    void Try_TalkToPnj()
    {
        GameManager.instance.dialogMng.IM_ReachClosestOne();

        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameManager.instance.dialogMng.IM_World();
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            GameManager.instance.dialogMng.IM_World();
        }
    }
    void Try_ValidateDialog()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            GameManager.instance.dialogMng.IM_Dialog();
        }
        else if (Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            GameManager.instance.dialogMng.IM_Dialog();
        }
    }

    void Try_OpenInventory()
    {
        //For now, always, no limit.
        if (Input.GetKeyDown(KeyCode.E))
        {
            GameManager.instance.inventory.IM_Open();
        }
        else if (Input.GetKeyDown(KeyCode.Joystick1Button3))
        {
            GameManager.instance.inventory.IM_Open();
        }
    }

    void Try_CloseInventory()
    {
        //For now, always, no limit.
        if (Input.GetKeyDown(KeyCode.E))
        {
            GameManager.instance.inventory.IM_Close();
        }
        else if (Input.GetKeyDown(KeyCode.Joystick1Button1))
        {
            GameManager.instance.inventory.IM_Close();
        }
        //Need to add : if try to go to paper and map : just close the inventory AND try open 
    }

    void Try_GiveInventory()
    {
        //For now, always, no limit.
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameManager.instance.inventory.IM_Give();
        }
        else if (Input.GetKeyDown(KeyCode.Joystick1Button3))
        {
            GameManager.instance.inventory.IM_Give();
        }
    }
    void Try_ShowInventory()
    {
        //For now, always, no limit.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameManager.instance.inventory.IM_Show();
        }
        else if (Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            GameManager.instance.inventory.IM_Show();
        }
    }

    void Try_OpenMapAndPaper()
    {
        //For now, always, no limit.
        if (Input.GetKeyDown(KeyCode.Alpha1) 
            || Input.GetKeyDown(KeyCode.Alpha2) 
            || Input.GetKeyDown(KeyCode.Alpha3) 
            || Input.GetKeyDown(KeyCode.Alpha4) 
            || Input.GetKeyDown(KeyCode.Alpha5)
            )
        {
            GameManager.instance.mapAndPaper.IM_Open();
        }
        else if (Input.GetKeyDown(KeyCode.Joystick1Button6))
        {
            Debug.Log("6 is press");
            GameManager.instance.mapAndPaper.IM_Open();
        }
    }

    void Try_CloseMapAndPaper()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) 
            || Input.GetKeyDown(KeyCode.Alpha2)
            || Input.GetKeyDown(KeyCode.Alpha3)
            || Input.GetKeyDown(KeyCode.Alpha4)
            || Input.GetKeyDown(KeyCode.Alpha5)
            )
        {
            GameManager.instance.mapAndPaper.IM_Close();
        }
        else if (Input.GetKeyDown(KeyCode.Joystick1Button6))
        {
            GameManager.instance.mapAndPaper.IM_Close();
        }
        //Need to add : if try to go to paper and map : just close the inventory AND try open 
    }






    public List<GameObject> iconController = new List<GameObject>();
    public List<GameObject> iconKeyboard = new List<GameObject>();

    public void ChangeIconToCorrectDevice(bool newControllerValue)
    {
        if (controller != newControllerValue)
        {
            controller = newControllerValue;
            foreach (GameObject gO in iconKeyboard)
                gO.SetActive(!controller);
            foreach (GameObject gO in iconController)
                gO.SetActive(controller);

            foreach (pnj pn in GameManager.instance.dialogMng.allPNJ)
                pn.ChangeIcon(controller);

            GameManager.instance.inventory.ChangePromptVisual();
        }
    }
}
