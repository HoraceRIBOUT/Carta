using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SaveFileButton : MonoBehaviour
{
    [SerializeField] int index = 0;

    [SerializeField] UnityEngine.UI.Button button;

    private bool saveMode;
    [SerializeField] GameObject saveText;
    [SerializeField] GameObject loadText;

    [Header("Inner button")]
    [SerializeField] Image screenshot;
    [SerializeField] TMPro.TMP_Text fileName;
    [SerializeField] TMPro.TMP_Text progressin;
    [SerializeField] TMPro.TMP_Text dateAndHour;

    public Sprite spriteForNow;

    public void SetSaveForSave()
    {
        //ok so :

        //seek the save file document (.carta) :
        string path = Application.persistentDataPath + /*(GameManager.STEAMID != null ? "/" + GameManager.STEAMID + "/" : "") + */SaveAndLoad.SAVELOCATION + index + SaveAndLoad.TERMINAISON;
        if (File.Exists(path))
        {
            FillButtonFromSaveFile();
        }
        else
        {
            FillEmpty();
        }
        button.interactable = true;

        saveMode = true;
        saveText.SetActive(true);
        loadText.SetActive(false);
    }

    public void SetSaveForLoad()
    {
        //ok so :
        //seek the save file document (.carta) :
        string path = Application.persistentDataPath + /*(GameManager.STEAMID != null ? "/" + GameManager.STEAMID + "/" : "") + */SaveAndLoad.SAVELOCATION + index + SaveAndLoad.TERMINAISON;
        if (File.Exists(path))
        {
            FillButtonFromSaveFile();
            button.interactable = true;
            loadText.SetActive(true);
        }
        else
        {
            FillEmpty();
            //  if not :
            //      not interactable
            button.interactable = false;
            loadText.SetActive(false);
        }

        saveMode = false;
        saveText.SetActive(false);
    }

    void FillButtonFromSaveFile()
    {
        screenshot.sprite = spriteForNow;
        fileName.SetText("" + SaveAndLoad.SAVELOCATION + index);
        progressin.SetText("2/5");
        dateAndHour.SetText("2h59 17/07/2023");
        //      interactyable
        //      photo
        //      nom de la save file (save file + i) + un bouton "open location"
        //      progressin.text = number take / number delivered 
        //      sur le clique :
        //          - load la save
    }

    void FillEmpty()
    {
        screenshot.sprite = null;
        fileName.SetText("Libre");
        progressin.SetText("");
        dateAndHour.SetText("");
        //  if not :
        //      interactyable
        //      photo "white"
        //      <empty>
        //  else : 
    }

    public void OnClickOnButton()
    {
        if (saveMode)
        {
            SaveDatFile();
        }
        else
        {
            LoadDatFile();
        }
    }

    void SaveDatFile()
    {
        string path = Application.persistentDataPath + /*(GameManager.STEAMID != null ? "/" + GameManager.STEAMID + "/" : "") + */SaveAndLoad.SAVELOCATION + index + SaveAndLoad.TERMINAISON;
        if (!File.Exists(path))
        {
            SaveDatFile_ForReal();
        }
        else
        {
            //pop up to say "are you sure you want to do that ?" later
            SaveDatFile_ForReal();
        }
    } 

    void SaveDatFile_ForReal()
    {
        SaveAndLoad.SaveData(index);
        //maybe add a little wait here
        SetSaveForSave();
    }


    void LoadDatFile()
    {
        GameManager.instance.pauseManager.ReturnToPauseMenu();
        GameManager.instance.pauseManager.Resume();

        SaveAndLoad.LoadData(index);
    }
}
