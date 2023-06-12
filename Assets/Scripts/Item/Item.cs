using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "It_", menuName = "Carta/Item", order = 2)]
public class Item : ScriptableObject
{
    public itemID id;  
    public string nameDisplay = "";
    public Sprite icon;
    public tag tags = 0;
    
    [System.Flags]
    public enum tag
    {
        None        = 0,
        Factrice    = 1<<0,
        Temersohn   = 1<<1,
        Ludovico    = 1<<2,
        Leal        = 1<<3,
        Music       = 1<<4,
    }
    public static IEnumerable<Item.tag> GetTags(Item.tag input)
    {
        foreach (Item.tag value in Item.tag.GetValues(input.GetType()))
            if (input.HasFlag(value))
                yield return value;
    }

    [Header("Info ")]
    public List<ItemTarget> targets = new List<ItemTarget>();
    
    [System.Serializable]
    public struct ItemTarget
    {
        public pnj.pnjID pnj;
        public pnj  pnj_InGame;
        public bool finalTarget;

        public ItemTarget(pnj.pnjID _pnj, pnj _pnj_InGame, bool _finalTarget)
        {
            pnj = _pnj;
            pnj_InGame = _pnj_InGame;
            finalTarget = _finalTarget;
        }
    }                     

    [Sirenix.OdinInspector.Button]
    public void SeeAllTarget() 
    {
        targets.Clear();

        pnj[] listPNJ = FindObjectsOfType<pnj>();

        foreach (pnj pnjeez in listPNJ)
        {
            foreach (pnj.ItemReaction react in pnjeez.reactions)
            {
                if (react.itemToReactFrom == id)
                {
                    targets.Add(new ItemTarget(pnjeez.id, pnjeez, react.finalTarget));
                }
            }
        }
        Debug.Log("Data updated.");
    }

}
