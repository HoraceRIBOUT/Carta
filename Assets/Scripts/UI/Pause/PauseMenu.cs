using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public bool isPause = false;

    CursorLockMode cursorStateWhenPause = CursorLockMode.Locked;

    public CanvasGroup background;
    public CanvasGroup pauseMenu;
    public CanvasGroup saveLoadMenu;


    public List<SaveFileButton> saveAndLoadButtons = new List<SaveFileButton>();


    public void Pause()
    {
        cursorStateWhenPause = Cursor.lockState;
        Cursor.lockState = CursorLockMode.None;

        background.alpha = 1;
        background.interactable = true;
        background.blocksRaycasts = true;
        isPause = true;
        Time.timeScale = 0;
    }

    public void Resume()
    {
        ReturnToPauseMenu();

        Cursor.lockState = cursorStateWhenPause;
        
        Time.timeScale = 1;
        background.alpha = 0;
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
