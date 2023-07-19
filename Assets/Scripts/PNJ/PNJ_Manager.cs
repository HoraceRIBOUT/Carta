using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PNJ_Manager : MonoBehaviour
{
    private Dictionary<Item.tag, int> tagProgression = new Dictionary<Item.tag, int>();
    public List<TagProgress> victoryList = new List<TagProgress>();
    [System.Serializable]
    public class TagProgress
    {
        public Item.tag tag;
        public int valueMin = 0;
        public bool finish = false;

        [Header("Effect")]
        public List<pnjFirstChange> pnjToChange = new List<pnjFirstChange>();//may need to struct them in a block tho

        [System.Serializable]
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
            Debug.Log("Finish " + tag.ToString() + " (" + valueMin + ")" + "\n Unlock " + pnjToChange.Count + " dialog.");
            finish = true;
            for (int i = 0; i < pnjToChange.Count; i++)
            {
                pnjToChange[i].Use();
            }
            eventWhenSucceed.Invoke();
        }
    }

    public List<ZoneTrigger> allTrigger = new List<ZoneTrigger>();
    public List<SFX_Pnj> allPnjSFX = new List<SFX_Pnj>();

    [Sirenix.OdinInspector.Button()]
    public void FillAllTrigger()
    {
        allTrigger.Clear();
        foreach (ZoneTrigger zT in FindObjectsOfType<ZoneTrigger>())
        {
            allTrigger.Add(zT);
        }
    }
    [Sirenix.OdinInspector.Button()]
    public void FillAllSFX_Pnj()
    {
        allPnjSFX.Clear();
        foreach (SFX_Pnj zT in FindObjectsOfType<SFX_Pnj>())
        {
            allPnjSFX.Add(zT);
        }
    }

    public void StartDialog()
    {
        foreach (SFX_Pnj sfx in allPnjSFX)
        {
            sfx.StartDialog();
        }
    }
    public void FinishDialog()
    {
        foreach (SFX_Pnj sfx in allPnjSFX)
        {
            sfx.FinishDialog();
        }
    }

    public void Victory(float timeOfVictory)
    {
        foreach (SFX_Pnj sfx in allPnjSFX)
        {
            sfx.Victory(timeOfVictory);
        }
    }


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

    public void ChangeZoneStatus(string id, bool activate)
    {
        ZoneTrigger zone = GetZoneByID(id);

        zone.gameObject.SetActive(activate);
    }
    public ZoneTrigger GetZoneByID(string id)
    {
        foreach(ZoneTrigger zone in allTrigger)
        {
            if (id == zone.balcony_id)
                return zone;
        }
        return null;
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
            int tagValue = -1;
            if (currTag == Item.tag.None)
            {
                //Mean it's global progress
                tagValue = GameManager.instance.inventory.inventory_all.Count - GameManager.instance.inventory.inventory_current.Count;
            }
            else if (tagProgression.ContainsKey(currTag))
            {
                tagValue = tagProgression[currTag];
            }

            if (tagValue != -1)
            {
                if(tagValue >= tagProg.valueMin)
                {
                    tagProg.Success();
                }
            }
        }
    }


}
