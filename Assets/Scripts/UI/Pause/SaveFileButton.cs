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
        string path = Path();
        if (File.Exists(path))
        {
            FillButtonFromSaveFile();
        }
        else
        {
            FillAsEmpty();
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
        string path = Path();
        if (File.Exists(path))
        {
            FillButtonFromSaveFile();
            button.interactable = true;
            loadText.SetActive(true);
        }
        else
        {
            FillAsEmpty();
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
        fileName.SetText("" + System.IO.Path.GetFileNameWithoutExtension(SaveAndLoad.SAVELOCATION) + index);
        //      photo
        string imagePath = ImagePath();
        if (string.IsNullOrEmpty(imagePath))
        {
            Debug.LogError("not screenshot for savefile "+ index);
        }
        else if (System.IO.File.Exists(imagePath))
        {
            byte[] bytes = System.IO.File.ReadAllBytes(imagePath);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            screenshot.sprite = sprite;
        }
        //Stock it to be sure : 
        //also, if the "save data" become larger, maybe put the progress and time in a separated, lighter, saveFiles and load only this
        var data = SaveAndLoad.GetData(index);
        //      horaire de la save
        //      progressin.text = number take / number delivered 
        progressin.SetText(data.Progression().x + "/" + data.Progression().y);
        System.DateTime dateTime = data.dateTime.GetDate();
        dateAndHour.SetText(dateTime.ToShortTimeString() + " " + dateTime.ToShortDateString());
    }

    void FillAsEmpty()
    {
        screenshot.sprite = null;
        fileName.SetText("Libre");
        progressin.SetText("");
        dateAndHour.SetText("");
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
        string path = Path();
        if (!File.Exists(path))
        {
            SaveDatFile_ForReal();
        }
        else
        {
            //pop up to say "are you sure you want to do that ?" later
            File.Delete(path);
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

    public void TrashBinButton()
    {
        //normally, add a "are you SURE about that ???" button
        DeleteDatFile();
        FillAsEmpty();

        if (!saveMode)
        {
            button.interactable = false;
            loadText.SetActive(false);
        }
    }

    void DeleteDatFile()
    {
        string path = Path();

        File.Delete(path);
    }

    public void MagicFolder()
    {
        Debug.Log("Opening the save folder sir!");
        Application.OpenURL(System.IO.Path.GetDirectoryName(Path()));
        //just in case :
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    string Path()
    {
        return Application.persistentDataPath + /*(GameManager.STEAMID != null ? "/" + GameManager.STEAMID + "/" : "") + */SaveAndLoad.SAVELOCATION + index + SaveAndLoad.TERMINAISON;
    }
    string ImagePath()
    {
        return Application.persistentDataPath + /*(GameManager.STEAMID != null ? "/" + GameManager.STEAMID + "/" : "") + */SaveAndLoad.SAVELOCATION_img + index + SaveAndLoad.TERMINAISON_img;
    }
}
