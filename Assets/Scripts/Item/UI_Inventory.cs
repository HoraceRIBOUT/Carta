using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Inventory : MonoBehaviour
{
    public GameObject prefabItemBox;
    public List<Item> allItem = new List<Item>();

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
    public float transparencyGoal = 0;
    public float transparencySpeed = 3;

    [Header("List item box")]
    public List<UI_ItemBox> allBox;
    public RectTransform boxParent;

    public int currentItemIndex = 0;

    [Header("Box Movement")]
    public float offsetBetweenBox = 500;
    public float moveSpeed = 5f;
    public AnimationCurve moveSpeedCurve;
    public float currentMoveValue = 0;
    public float currentMoveSpeed = 0;

    [Header("Inventory")]
    public Dictionary<itemID, Item> inventory_all;
    public Dictionary<itemID, Item> inventory_current;
    public List<Item> currentDeployList = new List<Item>();
    public int indexOfNotInList = 0;
    [Tooltip("For debug purpose only")]
    public List<Item> startItem_Current = new List<Item>(); 
    public List<Item> startItem_All = new List<Item>(); 


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

        //For debug only
        foreach (Item item in startItem_Current)
            if(!startItem_All.Contains(item))
                startItem_All.Add(item);
        foreach (Item item in startItem_All)
        {
            AddItem(item, true);
            if (startItem_Current != null)
                if (!startItem_Current.Contains(item))
                    RemItem(item, true);
        }
        //End debug only

        PopulateCurrentItemList();
        CreateBox();
        Retract();
    }

    public void PopulateCurrentItemList()
    {
        currentDeployList = new List<Item>();
        foreach (KeyValuePair<itemID, Item> item in inventory_current)
        {
            currentDeployList.Add(item.Value);
        }
        foreach (KeyValuePair<itemID, Item> item in inventory_all)
        {
            if (!currentDeployList.Contains(item.Value))
                currentDeployList.Add(item.Value);
        }
        indexOfNotInList = inventory_current.Count;
        Debug.Log("Finish  adding element" + currentDeployList.Count + " (bonus : " + indexOfNotInList + ")");

    }

    public void CreateBox()
    {
        if(allBox == null || allBox.Count == 0)
            allBox = new List<UI_ItemBox>();
        for (int i = 0; i < currentDeployList.Count; i++)
        {
            UI_ItemBox box;
            if (i >= allBox.Count) //reuse previous box. 
                box = Instantiate(prefabItemBox, boxParent.transform).GetComponent<UI_ItemBox>();
            else
                box = allBox[i];
            box.SetUpBox(currentDeployList[i], i >= indexOfNotInList, i >= allBox.Count);
            //Position : 
            box.Placement(i, offsetBetweenBox, i == currentItemIndex);
            if (i >= allBox.Count)
                allBox.Add(box);
        }

        //Normally, number of boxes never diminish
        if (allBox.Count > currentDeployList.Count)
        {
            int toDestroy = allBox.Count - currentDeployList.Count;
            Debug.LogError("Wow, when did we get fewer box ??? Destroy them ! (" + toDestroy + ")");
            //for (int i = currentDeployList.Count; i < allBox.Count; i++)
            //{
            //    Destroy(allBox[i].gameObject);
            //}
            //allBox.RemoveRange(currentDeployList.Count, toDestroy);
        }

    }

    public void Update()
    {
        if (GameManager.instance.dialogMng.inDialog)
            deployPrompt.alpha = Mathf.Lerp(deployPrompt.alpha, transparencyGoal, Time.deltaTime * transparencySpeed);
        else
            deployPrompt.alpha = Mathf.Lerp(deployPrompt.alpha, 0, Time.deltaTime * transparencySpeed);
    }

    public void IM_MoveUpDown(Vector2 direction)
    {
        if (Mathf.Abs(direction.y) < 0.1f)
        {
            if (currentMoveSpeed > 0)
            {
                currentMoveSpeed = Mathf.Clamp01(currentMoveSpeed - Time.deltaTime * 3f);
            }

            currentMoveValue = Mathf.Lerp(currentMoveValue, currentItemIndex, Time.deltaTime * 4);
        }
        MoveChoice(direction.y);
        UpdateBoxPlacement();
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
        CreateBox();
    }

    public void MoveChoice(float up)
    {
        bool edgeSlowEffect = false;
        if (up < 0)
        {
            if(currentMoveValue > allBox.Count - 1)
            {
                currentMoveSpeed = Mathf.Clamp01(currentMoveSpeed - Time.deltaTime * 3f);
                edgeSlowEffect = true;
            }

            if(currentMoveValue < allBox.Count - 0.5f)
                currentMoveValue += Time.deltaTime * moveSpeedCurve.Evaluate(currentMoveSpeed) * moveSpeed;
        }

        if (up > 0)
        {
            if (currentMoveValue < 0)
            {
                currentMoveSpeed = Mathf.Clamp01(currentMoveSpeed - Time.deltaTime * 3f);
                edgeSlowEffect = true;
            }

            if (currentMoveValue > -0.5f)
                    currentMoveValue -= Time.deltaTime * moveSpeedCurve.Evaluate(currentMoveSpeed) * moveSpeed;
        }
        currentMoveValue = Mathf.Clamp(currentMoveValue, -0.499f, allBox.Count - 0.499f); //to avoid rounding too high or too low
        currentItemIndex = Mathf.RoundToInt(currentMoveValue);

        if (up != 0 && currentMoveSpeed < 1 && !edgeSlowEffect)
        {
            currentMoveSpeed = Mathf.Clamp01(currentMoveSpeed + Time.deltaTime);
        }
    }

    public void UpdateBoxPlacement()
    {
        for (int i = 0; i < allBox.Count; i++)
        {
            UI_ItemBox box = allBox[i];
            box.Placement(i - currentMoveValue, offsetBetweenBox, i == currentItemIndex);
        }
        //Debug.Log("currentMoveValue "+ currentMoveValue);
    }



    #region External Call

    public void AddItem(Step.Step_AddItem data)
    {
        Item res = null;
        foreach (Item it in GameManager.instance.inventory.allItem)
        {
            if(it.id == data.itemId)
            {
                res = it;
                break;
            }
        }
        if (res == null)
            Debug.LogError("Did not add the "+ data.itemId + " item in the inventory pool.");
        else
            AddItem(res);
    }

    public void AddItem(Item it, bool debugAdd = false)
    {
        Debug.Log("Add !" + it.id);
        inventory_all.Add(it.id, it);
        inventory_current.Add(it.id, it);

        if (debugAdd)
            return;
        
        PopulateCurrentItemList();
        UpdateVisual();
    }
    public void RemItem(Step.Step_RemItem data)
    {
        if (!inventory_current.ContainsKey(data.itemId))
        {
            Debug.LogError("Did not have the " + data.itemId + " item in the inventory curent.");
            //May be already gave ? Or not taken in the first place ?
            return;
        }
        Item res = inventory_current[data.itemId];
        RemItem(res);
    }
    public void RemItem(Item it, bool debugAdd = false)
    {
        Debug.Log("Remove !" + it.id);
        inventory_current.Remove(it.id);

        if (debugAdd)
            return;

        PopulateCurrentItemList();
        UpdateVisual();
    }


    public void IM_Give()
    {
        Give();
    }

    public void IM_Show()
    {
        Show();
    }

    public void Give()
    {
        //Because it's a give, we need to include a "drum roll moment"
        //before saying either it's a good catch or not 
        Item itemSelected = currentDeployList[currentItemIndex];
        pnj currentPNJ = GameManager.instance.dialogMng.currentPNJ;

        if (currentPNJ != null)
        {
            foreach (pnj.ItemReaction react in currentPNJ.reactions)
            {
                if(react.itemToReactFrom == itemSelected.id)
                {
                    if (react.finalTarget)
                    {
                        //Vicotry music !
                        RemItem(itemSelected);
                    }
                    else
                    {
                        //Loose music...
                    }
                    GameManager.instance.dialogMng.StartDialog(react.responseGive);
                    Retract();
                    return;
                }
            }
            GameManager.instance.dialogMng.StartDialog(currentPNJ.defaultReaction);
            Retract();
        }
    }

    public void Show()
    {

    }

    #endregion


    public void IM_Open()
    {
        Deploy();
    }
    public void IM_Close()
    {
        Retract();
    }



    public void Deploy()
    {
        if (allBox.Count == 0)
        {
            //Cannot deploy an empty inventory
            return;
        }

        Debug.Log("Deploy inv");

        //For other:
        if (!GameManager.instance.dialogMng.inDialog)
            GameManager.instance.playerMove.InventoryAndMenu();

        //For itself
        currentItemIndex = 0;
        currentMoveValue = 0;

        mainRect.anchorMin = new Vector2(0.8f, 0);
        mainRect.anchorMax = new Vector2(1.0f, 1);
        mainRect.anchoredPosition = Vector2.zero;

        inventoryDeployed = true;

        if (deployingRoutine != null)
            StopCoroutine(deployingRoutine);
        deployingRoutine = StartCoroutine(Deploy_Corout(true));

        allBox[0].Deploy();
    }

    public void Retract()
    {
        Debug.Log("Retract inv");

        //For other:
        if(!GameManager.instance.dialogMng.inDialog)
            GameManager.instance.playerMove.FinishMenuing();

        //For itself
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
        //Debug.Log("Cortou start : " + (deploy ? "deploy" : "retract"));
        while((deploy?deployLerp < 1 : deployLerp > 0))
        {
            deployLerp += Time.deltaTime * trnasitionSpeed * (deploy ? 1 : -1);
            deployLerp = Mathf.Clamp01(deployLerp);

            float effectifLerp = trnasitionCurve.Evaluate(deployLerp);

            mainRect.anchorMin = new Vector2(Mathf.Lerp(0.8f, 1.0f, effectifLerp), 0);
            mainRect.anchorMax = new Vector2(Mathf.Lerp(1.0f, 1.2f, effectifLerp), 1);
            mainRect.anchoredPosition = Vector2.zero;

            transparencyGoal = effectifLerp;
            yield return new WaitForSeconds(1f / 60f);
        }
    }

}
