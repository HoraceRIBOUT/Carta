using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveAndLoad : MonoBehaviour
{
    public static string SAVELOCATION = "/SaveSlot/SAVE_CARTA_";
    public static string TERMINAISON = ".carta";

    [System.Serializable]
    //Name this "Global_SaveData"
    // and create "PNJ_saveData" and "MaP_SaveDate"
    private class Global_SaveData
    {
        //Character inventory :
        public List<itemID> inventory_all;
        public List<itemID> inventory_current;

        //Character position :
        public float posWhenQuit_x;
        public float posWhenQuit_y;
        public float posWhenQuit_z;
        //Character visual : (maybe)

        //PNJ state (for the pnj who change position or state (factrice, aguilar, wolfgirl...) : 
        public List<pnj.PNJ_SaveData> pnjSave;

        //Time of the day :
        public float timeOfTheDay;

        //element and page (mostly allready Serializable ?)

        public Global_SaveData(List<itemID> _inventory_all, List<itemID> _inventory_current, Vector3 _posWhenQuit, float _timeOfTheDay, List<pnj.PNJ_SaveData> _pnjSave)
        {
            inventory_all = _inventory_all;
            inventory_current = _inventory_current;
            posWhenQuit_x = _posWhenQuit.x;
            posWhenQuit_y = _posWhenQuit.y;
            posWhenQuit_z = _posWhenQuit.z;
            timeOfTheDay = _timeOfTheDay;
            pnjSave = _pnjSave;
        }

        public Vector3 posWhenQuit()
        {
            return new Vector3(posWhenQuit_x, posWhenQuit_y, posWhenQuit_z);
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


        //Open in windows explorer
        Application.OpenURL(Path.GetDirectoryName(path));
        Debug.Log("Finish save data");
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




    private static Global_SaveData GatherDataToSave()
    {
        //What do we save ? 
        // - item : delivered or not
        List<itemID> inventoryAll = new List<itemID>();
        foreach(var item in GameManager.instance.inventory.inventory_all)
        {
            inventoryAll.Add(item.Key);
        };

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

            if(pnj.defaultDialog != null && pnj.giveWait_Dial != null) //need to be sure it's not WIP
                pnjSave.Add(pnj.GetSaveData());
        }

        //Already read dialog : 
        //OK, THIS is going to take a huge chunck.
        // But can also be really good 


        //Time of the day
        float timeOfTheDay = FindObjectOfType<SkyManager>().timeOfTheDay;

        //UI : MaP 
        // - icon and text associate
        // - page

        //

        Global_SaveData data = new Global_SaveData(inventoryAll, inventory_current, characterPos, timeOfTheDay, pnjSave);
        return data;
    }

    private static void ApplyLoadedData(Global_SaveData data)
    {
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


        //Character :
        //Go reach the closest spawn point to where you quit 


        //Time of the day :
        FindObjectOfType<SkyManager>().timeOfTheDay = data.timeOfTheDay;

        //MaP :

    }
}
