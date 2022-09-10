using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "It_", menuName = "Carta/Item", order = 2)]
public class Item : ScriptableObject
{
    public itemID id;  
    public string nameDisplay = "";
    public Sprite icon;
}
