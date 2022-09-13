using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Inventory : MonoBehaviour
{
    public GameObject prefabItemBox;

    [Header("Deploy")]
    public bool inventoryDeployed = false;
    [Range(0,1)]
    public float deployLerp = 0;
    public CanvasGroup deployPrompt;
    public RectTransform mainRect;

    [Header("Transition deploy")]
    public float trnasitionSpeed = 1.5f;
    public AnimationCurve trnasitionCurve;
    Coroutine deployingRoutine = null;

    [Header("List item box")]
    public List<UI_ItemBox> allBox;
    public RectTransform boxParent;

    public int currentPlace = 0;


    [Header("Inventory")]
    public Dictionary<itemID, Item> inventory_all;
    public Dictionary<itemID, Item> inventory_current;
    public List<Item> currentDeployList = new List<Item>();
    public int indexOfNotInList = 0;
    [Tooltip("For debug purpose only")]
    public List<Item> startItem = new List<Item>(); 


    public void Start()
    {
        inventoryDeployed = false;
        //Generate 6 box max. //one will go ont the side if needed
        // - deactivate the 6 box.
        //  - populate box dynamicly (add icon, populate text with name)
        //   - when on main, text appear with 
        //   - button to give / to show / to go back 
        // Too many thing.

        //Go to simple : 
        inventory_all = new Dictionary<itemID, Item>();
        inventory_current = new Dictionary<itemID, Item>();

        if (startItem != null)
        {
            foreach (var item in startItem)
            {
                AddItem(item);
            }
        }

        currentDeployList = new List<Item>();
        if (inventory_current != null && inventory_current.Count != 0)
        {
            foreach (KeyValuePair<itemID, Item> item in inventory_current)
            {
                currentDeployList.Add(item.Value);
            }
            foreach (KeyValuePair<itemID, Item> item in inventory_all)
            {
                if(!currentDeployList.Contains(item.Value))
                currentDeployList.Add(item.Value);
            }
            indexOfNotInList = inventory_current.Count;
        }

        for (int i = 0; i < 1/*currentDeployList.Count*/; i++)
        {
            UI_ItemBox box = Instantiate(prefabItemBox, boxParent.transform).GetComponent<UI_ItemBox>();
            box.SetUpBox(currentDeployList[i], i > indexOfNotInList);
            allBox = new List<UI_ItemBox>();
            allBox.Add(box);
        }

        Retract();
    }

    public bool InputManagement_MoveUpDown(Vector2 direction)
    {
        if (inventoryDeployed)
        {
            if(Mathf.Abs(direction.y) > 0.1f)
            {
                MoveChoice(direction.y > 0);
            }
            return true;
        }
        return false;
    }

    public bool InputManagement_GiveItem()
    {
        if (inventoryDeployed)
        {
            //Launch Item dialog from that PNJ !
            //Et si pas d'item, dialog par défaut
            return true;
        }
        return false;
    }

    public void UpdateVisual()
    {
    }

    public void MoveChoice(bool up)
    {
        //if up then --
        //else ++ 
    }


    public void AddItem(Item it)
    {
        inventory_all.Add(it.id, it);
        inventory_current.Add(it.id, it);


        UpdateVisual();
    }
    public void RemItem(Item it)
    {
        inventory_current.Remove(it.id);

        UpdateVisual();
    }


    public bool EUpdate()
    {
        //if (Transition en cours) ;
            //return false;
        if (inventoryDeployed)
            Retract();
        else
            Deploy();

        return true;
    }



    public void Deploy()
    {
        Debug.Log("    public void Deploy inv");
        mainRect.anchorMin = new Vector2(0.8f, 0);
        mainRect.anchorMax = new Vector2(1.0f, 1);
        mainRect.anchoredPosition = Vector2.zero;

        deployPrompt.alpha = 0;
        inventoryDeployed = true;

        if (deployingRoutine != null)
            StopCoroutine(deployingRoutine);
        deployingRoutine = StartCoroutine(Deploy_Corout(true));

        foreach (UI_ItemBox item in allBox)
        {
            item.Deploy();
        }
    }

    public void Retract()
    {
        Debug.Log("Retract inv");

        inventoryDeployed = false;

        if (deployingRoutine != null)
            StopCoroutine(deployingRoutine);
        deployingRoutine = StartCoroutine(Deploy_Corout(false));

        foreach (UI_ItemBox item in allBox)
        {
            item.Retract();
        }
    }


    public IEnumerator Deploy_Corout(bool deploy)
    {
        Debug.Log("Cortou start : " + (deploy ? "deploy" : "retract"));
        while((deploy?deployLerp < 1 : deployLerp > 0))
        {
            deployLerp += Time.deltaTime * trnasitionSpeed * (deploy ? 1 : -1);
            deployLerp = Mathf.Clamp01(deployLerp);

            float effectifLerp = trnasitionCurve.Evaluate(deployLerp);

            mainRect.anchorMin = new Vector2(Mathf.Lerp(0.8f, 1.0f, effectifLerp), 0);
            mainRect.anchorMax = new Vector2(Mathf.Lerp(1.0f, 1.2f, effectifLerp), 1);
            mainRect.anchoredPosition = Vector2.zero;

            deployPrompt.alpha = effectifLerp;
            yield return new WaitForSeconds(1f / 60f);
        }
    }

}
