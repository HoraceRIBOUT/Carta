using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PNJ_Manager : MonoBehaviour
{
    private Dictionary<Item.tag, int> tagProgression = new Dictionary<Item.tag, int>();
    public List<TagProgress> victoryList = new List<TagProgress>();
    public class TagProgress
    {
        public Item.tag tag;
        public int valueMin = 0;
        public bool finish = false;

        [Header("Effect")]
        public List<pnjFirstChange> pnjToChange = new List<pnjFirstChange>();//may need to struct them in a block tho
        public struct pnjFirstChange
        {
             public pnj.pnjID id;
             public Dialog dialog;
             public int priority;
            public void Use()
            {
                GameManager.instance.dialogMng.GetPNJFromID(id).AddNextDialog(dialog, priority);
            }
        }
        public UnityEvent eventWhenSucceed;

        public void Success()
        {
            finish = true;
            for (int i = 0; i < pnjToChange.Count; i++)
            {
                pnjToChange[i].Use();
            }
            eventWhenSucceed.Invoke();
        }
    }

    public List<ZoneTrigger> allTrigger;

    public class PNJ_Manager_Save
    {
        public List<int> progressFinish;
        public List<int> zoneTriggerActive;

        public PNJ_Manager_Save(List<TagProgress> currentProgress, List<ZoneTrigger> allTrigger)
        {
            progressFinish = new List<int>();
            zoneTriggerActive = new List<int>();
            for (int i = 0; i < currentProgress.Count; i++)
            {
                TagProgress prog = (TagProgress)currentProgress[i];
                if (prog.finish)
                    progressFinish.Add(i);
            }
            for (int i = 0; i < allTrigger.Count; i++)
            {
                if (allTrigger[i].gameObject.activeSelf)
                {
                    zoneTriggerActive.Add(i);
                }
            }
        }
    }

    //TO Do : link that save !
    public PNJ_Manager_Save CreateSave()
    {
        PNJ_Manager_Save saveData = new PNJ_Manager_Save(victoryList, allTrigger);

        return saveData;
    }
    public void LoadSave(PNJ_Manager_Save data)
    {
        foreach(int index in data.progressFinish)
        {
            victoryList[index].finish = true;
        }
        for (int i = 0; i < allTrigger.Count; i++)
        {
            allTrigger[i].gameObject.SetActive(data.zoneTriggerActive.Contains(i));
        }
        UpdateTag();
    }




    public void UpdateTag()
    {
        var inv_all = GameManager.instance.inventory.inventory_all;
        var inv_curr = GameManager.instance.inventory.inventory_current;

        tagProgression.Clear();
        foreach (KeyValuePair<itemID, Item> pair in inv_all)
        {
            if (inv_curr.ContainsKey(pair.Key))
                continue; // not delivered
            foreach (Item.tag tag in Item.GetTags(pair.Value.tags))
            {
                if (tagProgression.ContainsKey(tag))
                {
                    tagProgression[tag]++;
                }
                else
                {
                    tagProgression.Add(tag, 1);
                }
            }
        }

        CheckProgression();

    }

    public void CheckProgression()
    {
        foreach (TagProgress tagProg in victoryList)
        {
            if (tagProg.finish)
                continue;

            Item.tag currTag = tagProg.tag;
            if (tagProgression.ContainsKey(currTag))
            {
                if(tagProgression[currTag] >= tagProg.valueMin)
                {
                    tagProg.Success();
                }
            }
        }
    }


}
