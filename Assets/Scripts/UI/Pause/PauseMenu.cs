using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Sirenix.OdinInspector.ReadOnly()]
    public bool isPause = false;
    public float fadeInOutSpeed = 10f;
    CursorLockMode cursorStateWhenPause = CursorLockMode.Locked;

    public CanvasGroup background;
    public CanvasGroup pauseMenu;
    public CanvasGroup saveLoadMenu;


    public List<SaveFileButton> saveAndLoadButtons = new List<SaveFileButton>();


    public void Pause()
    {
        SaveAndLoad.ScreenshotForSave();

        cursorStateWhenPause = Cursor.lockState;
        Cursor.lockState = CursorLockMode.None;
        
        background.interactable = true;
        background.blocksRaycasts = true;
        isPause = true;
        Time.timeScale = 0;
    }

    private void Update()
    {
        if(background.alpha < 1 && isPause)
            background.alpha += Time.unscaledDeltaTime * fadeInOutSpeed;
        if(background.alpha > 0 && !isPause)
            background.alpha -= Time.unscaledDeltaTime * fadeInOutSpeed;
    }

    public void Resume()
    {
        ReturnToPauseMenu();

        Cursor.lockState = cursorStateWhenPause;
        
        Time.timeScale = 1;
        background.interactable = false;
        background.blocksRaycasts = false;
        isPause = false;
    }

    public void SaveMenu()
    {
        pauseMenu.alpha = .5f;
        pauseMenu.interactable = false;
        //ok so we have like 3 save ? 
        saveLoadMenu.alpha = 1;
        saveLoadMenu.interactable = true;
        saveLoadMenu.blocksRaycasts = true;

        foreach (SaveFileButton saveButt in saveAndLoadButtons)
        {
            saveButt.SetSaveForSave();
        }

        //and for each one of them we have a save file.... ok ok 
    }

    public void LoadMenu()
    {
        pauseMenu.alpha = .5f;
        pauseMenu.interactable = false;
        //again, we have 3 save files 
        saveLoadMenu.alpha = 1;
        saveLoadMenu.interactable = true;
        saveLoadMenu.blocksRaycasts = true;

        foreach (SaveFileButton saveButt in saveAndLoadButtons)
        {
            saveButt.SetSaveForLoad();
        }
        //only the load one are good to click
    }

    public void ReturnToPauseMenu()
    {
        pauseMenu.alpha = 1;
        pauseMenu.interactable = true;

        saveLoadMenu.alpha = 0;
        saveLoadMenu.interactable = false;
        saveLoadMenu.blocksRaycasts = false;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
