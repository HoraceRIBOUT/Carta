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
    [Range(0,10)]
    public int difficulty = 1;
    //
    public enum knowledgeState
    {
        zero = 0,
        talkTo = 1,
        firstInfo = 2,
        fewInfo_elim = 3,
        lotOfInfo = 8,
        lotOfInfo_elim = 9,
        all = 10,
    }
    public struct knowledgeTake
    {
        public Dialog dialog;
        public int stepNumber;
        public knowledgeState unlockState;
    }
    [SerializeField] knowledgeState defaultState = knowledgeState.zero;
    [SerializeField] List<knowledgeTake> allKnowledge = new List<knowledgeTake>();

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
        if (allKnowledge.Count == 0)
            return defaultState;

        knowledgeState res = defaultState;
        bool allTrue = true;
        foreach (knowledgeTake take in allKnowledge)
        {
            if(take.dialog != null && take.dialog.allSteps.Count < take.stepNumber)
            {
                if (take.dialog.allSteps[take.stepNumber].alreadyRead)
                {
                    //we only update the check if 
                    if ((int)res < (int)take.unlockState)
                        res = take.unlockState;
                }
                else
                    allTrue = false;
            }
            else
            {
                if (take.dialog != null)
                    Debug.LogError("Try to seek step " + take.stepNumber + " in " + take.dialog.name + " while it only have " + take.dialog.allSteps.Count + " steps.");
                else
                    Debug.LogError(this.name + " have a knowledge take which are link to no dialog. Please, delete it.");
            }
        }

        //could be better if I could make different dialog ref to the same info in there
        if (allTrue)
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
