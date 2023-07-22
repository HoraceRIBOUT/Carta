using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveAndLoad : MonoBehaviour
{
    public static string SAVELOCATION = "/SaveSlot/SAVE_CARTA_";
    public static string SAVELOCATION_img = "/SaveSlot/SAVE_CARTA_screen_";
    public static string TERMINAISON = ".carta";
    public static string TERMINAISON_img = ".png";

    [System.Serializable]
    //Name this "Global_SaveData"
    // and create "PNJ_saveData" and "MaP_SaveDate"
    public class Global_SaveData
    {
        //Character inventory :
        public List<itemID> inventory_all;
        public List<itemID> inventory_current;

        //Character position :
        private float posWhenQuit_x, posWhenQuit_y, posWhenQuit_z;
        //Character visual : (maybe)

        //PNJ state (for the pnj who change position or state (factrice, aguilar, wolfgirl...) : 
        public List<pnj.PNJ_SaveData> pnjSave;
        public PNJ_Manager.PNJ_Manager_Save triggerSave;
        /// <summary>
        /// P'tite explication en français pour si je retourne sur ce sujet plus tard. 
        /// On a besoin de sauvegarder pour chaque dialog, si il a était entièrement lu ET si il a était lu au moins une fois. 
        /// Du coup, on sauvegarder une liste de string (allDialogName) qui nous donne l'index où chercher cette valeur (allDialogValue)
        /// Et ce int vaux 0 ou 1 ou 2 ou 3 selon que il y a !A&!B ou A&!B ou !A&B ou A&B. 
        /// Dans une version plus clean, on essayera de faire en sorte que la liste de string soit stocké quelque part dans le jeu en fix/static. 
        /// Une liste de string par version. 
        /// Lorsqu'on sauvegarde, à la place de "allDialogName" on qsauvegarde la version du jeu 
        /// Et ainsi, on pourra aller se reférer à la bonne version du jeu qui correspond à la sauvegarde. 
        /// Et en raccourcicant la liste de int en mettant les 2 bits d'infos pour 16 dialog dans un int (32 bit = 2 bit x 16) 
        /// on aura une save plus légère.
        /// </summary>
        public List<string> allDialogName; //in later version, the list will be fix and be like "for each version of the game" 
        public List<int> allDialogValue; //this will be change to have an optimisation : we only need 2 bool, but we have a 32 bit value. so, we can store 16 data by []

        //Time of the day :
        public float timeOfTheDay;
        public SerialazableDateTime dateTime;
        private int progressionX,progressionY;

        //element and page (mostly allready Serializable ?)
        public List<UI_MaP_Paper.Paper_SaveData> papersSave;
        public List<int> papersUnlock;
        public List<IconData.Icon_SaveData> iconsSave;
        public List<pnj.pnjID> pnjAlreadyMet;

        public Global_SaveData(List<itemID> _inventory_all, List<itemID> _inventory_current, Vector3 _posWhenQuit, 
            float _timeOfTheDay, SerialazableDateTime _dateTime, Vector2 _progression,
            List<pnj.PNJ_SaveData> _pnjSave, PNJ_Manager.PNJ_Manager_Save _triggerSave, List<string> _allDialogName, List<int> _allDialogValue,
            List<UI_MaP_Paper.Paper_SaveData> _papersSave, List<int> _papersUnlock, List<IconData.Icon_SaveData> _iconsSave, List<pnj.pnjID> _pnjAlreadyMet)
        {
            inventory_all = _inventory_all;
            inventory_current = _inventory_current;
            posWhenQuit_x = _posWhenQuit.x;
            posWhenQuit_y = _posWhenQuit.y;
            posWhenQuit_z = _posWhenQuit.z;
            timeOfTheDay = _timeOfTheDay;
            dateTime = _dateTime;
            progressionX = (int)_progression.x;
            progressionY = (int)_progression.y;
            pnjSave = _pnjSave;
            triggerSave = _triggerSave;
            allDialogName = _allDialogName;
            allDialogValue = _allDialogValue;
            papersSave = _papersSave;
            papersUnlock = _papersUnlock;
            iconsSave = _iconsSave;
            pnjAlreadyMet = _pnjAlreadyMet;
        }

        public Vector3 posWhenQuit()
        {
            return new Vector3(posWhenQuit_x, posWhenQuit_y, posWhenQuit_z);
        }
        public Vector2 Progression()
        {
            return new Vector2(progressionX, progressionY);
        }
    }




    public static void SaveData(int slotNumber)
    {
        Debug.Log("Save data in slot n° "+slotNumber);
        string path = Application.persistentDataPath + /*(GameManager.STEAMID != null ? "/" + GameManager.STEAMID + "/" : "") + */SAVELOCATION + slotNumber + TERMINAISON;
        if (!Directory.Exists(Path.GetDirectoryName(path)))
            Directory.CreateDirectory(Path.GetDirectoryName(path));

        BinaryFormatter formater = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

        Global_SaveData data = GatherDataToSave();

        formater.Serialize(stream, data);
        stream.Close();

        //maybve add a screenshot here ? if yes, screenshot before the pause ?

        //Open in windows explorer
        //Application.OpenURL(Path.GetDirectoryName(path));
        //Cursor.visible = true;
        //Cursor.lockState = CursorLockMode.None;
        Debug.Log("Finish save data");


        //Create a screenshot : 
        string path_img = Application.persistentDataPath + /*(GameManager.STEAMID != null ? "/" + GameManager.STEAMID + "/" : "") + */SAVELOCATION_img + slotNumber + TERMINAISON_img;
        if (File.Exists(path_img))
        {
            File.Delete(path_img);
        }

        if (GameManager.instance != null && GameManager.instance.pauseManager.isPause)
        {
            string pause_img = PathPauseImage();
            File.Copy(pause_img, path_img);
        }
        else
        {
            ScreenCapture.CaptureScreenshot(path_img);
        }
        
    }
    public static void LoadData(int slotNumber)
    {
        Debug.Log("Load Data "+slotNumber);
        string path = Application.persistentDataPath + /*(GameManager.STEAMID != null ? "/" + GameManager.STEAMID + "/" : "") + */SAVELOCATION + slotNumber + TERMINAISON;
        if (File.Exists(path))
        {
            BinaryFormatter formater = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            Global_SaveData data = formater.Deserialize(stream) as Global_SaveData;
            stream.Close();

            ApplyLoadedData(data);
            Debug.Log("Finish load data");
        }
        else
        {
            //Can't load, and load a default save with warning message
            Debug.LogError("Can't load save file n°" + slotNumber);
        }
    }
    public static Global_SaveData GetData(int slotNumber)
    {
        Debug.Log("Get Data " + slotNumber);
        string path = Application.persistentDataPath + /*(GameManager.STEAMID != null ? "/" + GameManager.STEAMID + "/" : "") + */SAVELOCATION + slotNumber + TERMINAISON;
        if (File.Exists(path))
        {
            BinaryFormatter formater = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            Global_SaveData data = formater.Deserialize(stream) as Global_SaveData;
            stream.Close();

            return data;
        }
        else
        {
            //Can't load, and load a default save with warning message
            Debug.LogError("Can't load save file n°" + slotNumber);
            return null;
        }
    }


    /// <summary>
    /// Gather the data (save)
    /// </summary>
    /// <returns></returns>

    private static Global_SaveData GatherDataToSave()
    {
        //What do we save ? 
        // - item : delivered or not
        List<itemID> inventoryAll = new List<itemID>();
        foreach(var item in GameManager.instance.inventory.inventory_all)
        {
            inventoryAll.Add(item.Key);
        }

        List<itemID> inventory_current = new List<itemID>();
        foreach (var item in GameManager.instance.inventory.inventory_current)
        {
            inventory_current.Add(item.Key);
        }
        // - position ?
        // Maybe just a general position ?
        // - just test where the character is (zone) and load them there
        Vector3 characterPos = GameManager.instance.playerMove.transform.position;
        //Maybe the visual

        //PNJ state (int for each pnjID not at 0)
        //pnj : next dialog / current idle dial 
        List<pnj.PNJ_SaveData> pnjSave = new List<pnj.PNJ_SaveData>();
        foreach(pnj pnj in GameManager.instance.dialogMng.allPNJ)
        {
            if (pnj.id == pnj.pnjID.None)
                continue;//can ignore the None pnj (we may need to know which one are activate or not)  

            if(pnj.defaultDialog != null && pnj.showFail_Dial != null ) //need to be sure it's not WIP
                pnjSave.Add(pnj.GetSaveData());
        }

        PNJ_Manager.PNJ_Manager_Save triggerSave = GameManager.instance.pnjManager.CreateSave();
        //Already read dialog : 
        //OK, THIS is going to take a huge chunck.
        // But can also be really good 


        //Time of the day
        float timeOfTheDay = FindObjectOfType<SkyManager>().timeOfTheDay;

        //Just for the save display, no need to set it back
        SerialazableDateTime trueHourAndDay = new SerialazableDateTime(System.DateTime.Now);
        Vector2 progression = new Vector2(GameManager.instance.inventory.TotalItemDelivered(), GameManager.instance.inventory.TotalItemReceived());

        //also the screenshot right next to here ? can be load ??? not sure...

        //UI : MaP 
        // - icon and text associate
        List<IconData.Icon_SaveData> iconsSave = GameManager.instance.mapAndPaper.GetIconSaveData();
        List<pnj.pnjID> pnjAlreadyMet = GameManager.instance.mapAndPaper.sideTab.pnjToDeploy;
        // - page
        List <UI_MaP_Paper.Paper_SaveData> papersList = GameManager.instance.mapAndPaper.GetPaperSaveData();
        List<int> papersUnlock = GameManager.instance.mapAndPaper.papersUnlock;
        //

        //Dialog read or launch : 
        List<string> allDialogNames = GameManager.instance.dialogMng.GetSaveData_DialogStateName();
        List<int> allDialogValues = GameManager.instance.dialogMng.GetSaveData_DialogStateValue();

        Global_SaveData data = new Global_SaveData(
            inventoryAll, inventory_current,                            //Inventory 
            characterPos, timeOfTheDay, trueHourAndDay, progression,    //Global gameplay
            pnjSave, triggerSave, allDialogNames, allDialogValues,      //pnj and progression element
            papersList, papersUnlock, iconsSave, pnjAlreadyMet);        //ui and map and paper
        return data;
    }



    /// <summary>
    /// Apply the data (load)
    /// </summary>
    /// <param name="data"></param>

    private static void ApplyLoadedData(Global_SaveData data)
    {
        //Character :
        //Go reach the closest spawn point to where you quit 
        //  (for now, just the pos)
        GameManager.instance.playerMove.transform.position = data.posWhenQuit();

        //Character inventory : 
        Dictionary<itemID, Item> inventoryAll = new Dictionary<itemID, Item>();
        foreach (var itemId in data.inventory_all)
        {
            Debug.Log("Item Id = "+ itemId);
            Item item = GameManager.instance.inventory.GetItem(itemId);
            inventoryAll.Add(itemId, item);
        }
        GameManager.instance.inventory.inventory_all = inventoryAll;

        Dictionary<itemID, Item> inventory_current = new Dictionary<itemID, Item>();
        foreach (var itemId in data.inventory_current)
        {
            Item item = GameManager.instance.inventory.GetItem(itemId);
            inventory_current.Add(itemId, item);
        }
        GameManager.instance.inventory.inventory_current = inventory_current;

        GameManager.instance.inventory.PopulateCurrentItemList();
        GameManager.instance.inventory.UpdateVisual();

        //PNJ 
        foreach (pnj.PNJ_SaveData pnjData in data.pnjSave)
        {
            pnj pnj = GameManager.instance.dialogMng.GetPNJFromID(pnjData.id);
            pnj.LoadData(pnjData);
        }

        //Trigger zone
        GameManager.instance.pnjManager.LoadSave(data.triggerSave);


        //Time of the day :
        FindObjectOfType<SkyManager>().timeOfTheDay = data.timeOfTheDay;


        //Dialog read or launch : 
        GameManager.instance.dialogMng.LoadDialogState(data.allDialogName, data.allDialogValue);

        //MaP :
        GameManager.instance.mapAndPaper.ApplySaveData(data.papersSave, data.papersUnlock, data.iconsSave, data.pnjAlreadyMet);

    }

    [System.Serializable]
    public struct SerialazableDateTime
    {
        int year;
        int month;
        int day;
        int hour;
        int minute;
        int second;

        public SerialazableDateTime(System.DateTime dateTime)
        {
            year = dateTime.Year;
            month = dateTime.Month;
            day = dateTime.Day;
            hour = dateTime.Hour;
            minute = dateTime.Minute;
            second = dateTime.Second;
        }

        public System.DateTime GetDate()
        {
            return new System.DateTime(year, month, day, hour, minute, second);
        }
    }






    public static void ScreenshotForSave()
    {
        string path_img = PathPauseImage();
        ScreenCapture.CaptureScreenshot(path_img);
    }

    public static string PathPauseImage()
    {
        return Application.persistentDataPath + /*(GameManager.STEAMID != null ? "/" + GameManager.STEAMID + "/" : "") + */SAVELOCATION_img + " PAUSE" + TERMINAISON_img;
    }
}
