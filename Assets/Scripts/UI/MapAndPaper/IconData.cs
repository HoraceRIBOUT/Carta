using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PNJ_Icon_", menuName = "Carta/UI/Pnj icon", order = 0)]
public class IconData : ScriptableObject
{
    //It's just data
    public pnj.pnjID id;

    public Sprite spriteLittleIcon;

    public string mainName;
    public string surName;
    public string username;
    public string activity;
    //Relationship  and how to link
    
}
