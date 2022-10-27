using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "It_", menuName = "Carta/Item", order = 2)]
public class Item : ScriptableObject
{
    public itemID id;  
    public string nameDisplay = "";
    public Sprite icon;


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
