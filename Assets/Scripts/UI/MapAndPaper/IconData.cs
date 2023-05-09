using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PNJ_Icon_", menuName = "Carta/UI/Pnj icon", order = 0)]
public class IconData : ScriptableObject
{
    //It's just data
    public pnj.pnjID id;

    public Sprite spriteLittleIcon;
    public Color defaultColor;

    public string nameText;
    public string descText;


    //Relationship  and how to link


    [System.Serializable]
    public class Icon_SaveData
    {
        public pnj.pnjID id;
        public string nameText;
        public string descText;
    }

    public Icon_SaveData GetSerialazableIconData()
    {
        Icon_SaveData saveData = new Icon_SaveData();
        saveData.id = id;
        saveData.nameText = nameText;
        saveData.descText = descText;
        return saveData;
    }

}
