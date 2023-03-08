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

    public PlayerMove playerMove;
    public CameraManager cameraMng;
    public DialogManager dialogMng;
    public UI_Inventory inventory;
    public UI_MapAndPaper mapAndPaper;



    private float quitButton = 0;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (quitButton > 1)
                Application.Quit();
            quitButton += Time.deltaTime;
        }
        else
        {
            quitButton = 0;
        }

        if (Input.GetKey(KeyCode.S))
        { 
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
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
