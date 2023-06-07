using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public PlayerMovement playerMove;
    public CameraManager cameraMng;
    public DialogManager dialogMng;
    public UI_Inventory inventory;
    public UI_MapAndPaper mapAndPaper;
    public PNJ_Manager pnjManager;
    public UnityEngine.UI.Image blackScreenForQuit; //for demo only, need a pause menu


    private float quitButton = 0;
    public void Update()
    {

        if (Input.GetKey(KeyCode.Escape))
        {
            if (quitButton > 1)
                Application.Quit();
            quitButton += Time.deltaTime;
        }
        else
        {
            quitButton = 0;
        }

        if(blackScreenForQuit != null)
            blackScreenForQuit.color = Color.Lerp(Color.clear, Color.black, quitButton * quitButton);

        if(mapAndPaper != null)
        {
            if (mapAndPaper.IsEditingText())
                return;//Avoid p and j to make thing while on text input field
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Break();
        }
#endif

        if (Input.GetKey(KeyCode.J))
        { 
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                //Could be cool to close the menu before loading ?
                //or else just reload the scene ? don't know...
                     if (Input.GetKeyDown(KeyCode.Alpha1))                SaveAndLoad.LoadData(1);
                else if (Input.GetKeyDown(KeyCode.Alpha2))                SaveAndLoad.LoadData(2);
                else if (Input.GetKeyDown(KeyCode.Alpha3))                SaveAndLoad.LoadData(3);
                else if (Input.GetKeyDown(KeyCode.Alpha4))                SaveAndLoad.LoadData(4);
                else if (Input.GetKeyDown(KeyCode.Alpha5))                SaveAndLoad.LoadData(5);
            }
            else 
            {
                     if (Input.GetKeyDown(KeyCode.Alpha1))                SaveAndLoad.SaveData(1);
                else if (Input.GetKeyDown(KeyCode.Alpha2))                SaveAndLoad.SaveData(2);
                else if (Input.GetKeyDown(KeyCode.Alpha3))                SaveAndLoad.SaveData(3);
                else if (Input.GetKeyDown(KeyCode.Alpha4))                SaveAndLoad.SaveData(4);
                else if (Input.GetKeyDown(KeyCode.Alpha5))                SaveAndLoad.SaveData(5);
            }
        }
       
          
    }
}
