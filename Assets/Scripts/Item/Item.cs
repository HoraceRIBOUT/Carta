using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "It_", menuName = "Carta/Item", order = 2)]
public class Item : ScriptableObject
{
    public itemID id;
    public string nameDisplay = "";
    public Sprite icon;
    public string description_fixed = "";
    public string description_custom = "";
    [Range(0, 10)]
    public int difficulty = 1;
    //
    public enum knowledgeState
    {
        zero = 0,
        talkTo = 1,
        enviro = 2,
        firstInfo = 3,
        fewInfo_elim = 4,
        lotOfInfo = 8,
        lotOfInfo_elim = 9,
        all = 10,
    }

    [System.Serializable]
    public struct ClueBundle
    {
        public Clue id;
        public bool necessary;
        public knowledgeState unlockState;
        
        public bool IsMet { get => id.isMet; set => id.isMet = value; }
        public void SetState(bool value)
        {
            id.isMet = value;
        }
    }
    [SerializeField] knowledgeState defaultState = knowledgeState.zero;
    [SerializeField] knowledgeState finalState = knowledgeState.all;
    public List<ClueBundle> allClue = new List<ClueBundle>();

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
    
    public knowledgeState GetCurrentKnowledgeState()
    {
        if (allClue.Count == 0)
            return defaultState;

        knowledgeState res = defaultState;
        bool allMet = true;
        bool necessaryAllMet = true;
        bool didAnyNecessary = false;
        foreach (ClueBundle clue in allClue)
        {
            if (clue.necessary)
                didAnyNecessary = true;

            if (clue.IsMet)
            {
                if ((int)res < (int)clue.unlockState)
                {
                    res = clue.unlockState;
                }
            }
            else
            {
                allMet = false;
                if (clue.necessary)
                {
                    necessaryAllMet = false;
                }
            }
        }
        
        if (didAnyNecessary && necessaryAllMet)
            res = (knowledgeState)8;
        if (allMet)
            res = (knowledgeState)10;


        return res;
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



