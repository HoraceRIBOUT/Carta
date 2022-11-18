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
    }
}
