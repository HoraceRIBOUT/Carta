using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

//This is the manager for the "map" and "paper" part
public class UI_MapAndPaper : MonoBehaviour
{
    [SerializeField] private List<UI_MaP_Paper> papers = new List<UI_MaP_Paper>();
    public List<int> papersUnlock = new List<int>();
    public List<UnityEngine.UI.Button> paperButtons = new List<UnityEngine.UI.Button>();
    [ReadOnly] public int currentIndex = 0;
    public UI_MaP_SideTab sideTab;
    //So, by default, have one blank paper 
    public bool mapOpen = false;
    public TMPro.TMP_InputField currentEditText = null;
    public bool IsEditingText()
    {
        return currentEditText != null;
    }
    public void StopEditingText()
    {
        if(currentEditText != null)
        {
            Debug.Log("I call you ! Deactivation !");
            currentEditText.DeactivateInputField();
            Debug.Log("I called deactivation ! It's gfinish !");
            currentEditText = null;
        }
    }

    private Coroutine openCloseCorout = null;
    [SerializeField] private Animator _anima;
    [SerializeField] private CanvasGroup wholeOpacity; //use to clear the visual in Editor mode
    [SerializeField] public  RectTransform aboveMaP; //use to clear the visual in Editor mode

    [Header("Data and prefab")]
    public GameObject iconPrefab; 
    //probably also the different paper prefab

    public void Start()
    {
        if (Application.isPlaying && wholeOpacity.alpha == 0)
        {
            wholeOpacity.alpha = 1;
        }
    }

    public UI_MaP_Paper CurrentPaper()
    {
        return papers[currentIndex];
    }

    public void IM_Open(int index)
    {   
        Open(index);
    }
    public void IM_Open()
    {
        Open(currentIndex);
    }
    public void IM_Close()
    {
        Close();
    }

    public void Open(int index)
    {
        if (openCloseCorout != null)
            return;
        if (mapOpen)
            return;

        mapOpen = true;
        SwitchPaper(index);

        openCloseCorout = StartCoroutine(OpenCloseMap());

        sideTab.UpdateIconList();
        sideTab.UpdateElementList();
        //iconZone.Switch(0);   //don't switch, because the button switch some other item too
        GameManager.instance.playerMove.InventoryAndMenu();

        GameManager.instance.dialogMng.InventoryOrMapOpen();

    }

    public void SwitchPaper(int indexNewPaper)
    {
        currentIndex = SeekClosestGoodIndex(indexNewPaper);

        foreach (UI_MaP_Paper paper in papers)
        {
            paper.gameObject.SetActive(false);
        }
        CurrentPaper().gameObject.SetActive(true);
        foreach (UnityEngine.UI.Button button in paperButtons)
        {
            button.gameObject.SetActive(false);
        }

        //Only hsow button if more than one paper in inventory
        if (papersUnlock.Count > 1)
        {
            foreach (int indexUnlock in papersUnlock)
            {
                paperButtons[indexUnlock].gameObject.SetActive(true);
                paperButtons[indexUnlock].interactable = true;
            }
        }
        //Current is deactivate (maybe say "close" ?)
        paperButtons[currentIndex].interactable = false;

    }

    //Prioritize upper index 
    public int SeekClosestGoodIndex(int index)
    {
        if (papersUnlock.Contains(index))
            return index;
        Debug.Log("seek " + index);
        int distanceMax = 10;
        int indexRes = 0; //blank page n°
        foreach(int indexUnlock in papersUnlock)
        {
            int distance = Mathf.Abs(indexUnlock - index);
            Debug.Log("distance = " + distance + " (" + indexRes + ")" + " VS " + distanceMax);
            if (distanceMax > distance)
            {
                distanceMax = distance;
                indexRes = indexUnlock;
            }
            if(distanceMax == distance)
            {
                Debug.Log("take higher");
                if (indexUnlock > index)
                    indexRes = indexUnlock;
            }
        }
        return indexRes;
    }

    public void SwitchPaper_Previous()
    {
        papersUnlock.Sort();
        int indexCurr = papersUnlock.IndexOf(currentIndex);
        if(indexCurr == -1)
        {
            Debug.LogError("CurrentIndex not in the list.");
            indexCurr = 0;
        }
        else if (indexCurr == 0)
        {
            indexCurr = papersUnlock.Count;
        }

        int paperIndex = papersUnlock[indexCurr - 1];
        SwitchPaper(paperIndex);
    }
    public void SwitchPaper_Next()
    {
        papersUnlock.Sort();
        int indexCurr = papersUnlock.IndexOf(currentIndex);
        if (indexCurr == -1)
        {
            Debug.LogError("CurrentIndex not in the list.");
            indexCurr = 0;
        }
        else if (indexCurr == papersUnlock.Count - 1)
        {
            indexCurr = -1;
        }

        int paperIndex = papersUnlock[indexCurr + 1];
        SwitchPaper(paperIndex);
    }

    public void Paper_ResetPosition()
    {
        CurrentPaper().ResetPosAndScale();
    }

    public void Close()
    {
        Debug.Log("close ?");
        if (openCloseCorout != null)
            return;
        if (!mapOpen)
            return;

        mapOpen = false;
        openCloseCorout = StartCoroutine(OpenCloseMap());

        if(!GameManager.instance.dialogMng.inDialog && !GameManager.instance.inventory.inventoryDeployed)
        {
            GameManager.instance.playerMove.FinishMenuing();
        }

        GameManager.instance.dialogMng.InventoryOrMapClose();
        StopEditingText();
    }

    private IEnumerator OpenCloseMap()
    {
        Debug.Log("Coroutine");
        _anima.SetBool("Open", mapOpen);
        yield return new WaitForSeconds(0.3f);
        openCloseCorout = null;
    }


    public List<IconData.Icon_SaveData> GetIconSaveData()
    {
        return sideTab.GetSaveData();
    }
    public List<UI_MaP_Paper.Paper_SaveData> GetPaperSaveData()
    {
        List<UI_MaP_Paper.Paper_SaveData> res = new List<UI_MaP_Paper.Paper_SaveData>();
        for (int i = 0; i < papers.Count; i++)
        {
            UI_MaP_Paper paper = papers[i];
            res.Add(paper.GetSaveData(i));
        }
        return res;
    }


    public void ApplySaveData(List<UI_MaP_Paper.Paper_SaveData> papersData, List<int> papersToUnlock, List<IconData.Icon_SaveData> iconsData, List<pnj.pnjID> pnjAlreadyMet)
    {
        if(iconsData == null)
        {
            Debug.LogError("No icons get load");
        }
        //Do for icons before paper
        foreach (var iconData in iconsData)
        {
            //Apply data in them

            IconData data = sideTab.GetDataForThisPJN(iconData.id);
            data.descText = iconData.descText;
            data.nameText = iconData.nameText;
        }

        papersUnlock = papersToUnlock;
        //Clean all papers
        foreach(UI_MaP_Paper paper in papers)
        {
            paper.ClearDataOnIt();
        }
        foreach (UI_MaP_Paper.Paper_SaveData paperData in papersData)
        {
            //TO DO : 
            papers[paperData.indexOfPaper].ApplySaveData(paperData);
            //need to look into the "activate or not" for paper
            if (papersUnlock.Contains(paperData.indexOfPaper))
            {
                papers[paperData.indexOfPaper].gameObject.SetActive(true);
            }
            else
            {
                papers[paperData.indexOfPaper].gameObject.SetActive(false);
            }
        }
        sideTab.ApplySaveData(pnjAlreadyMet);
    }


    #region Element Specificity
    [System.Serializable]
    public class ElementSpec
    {
        public UI_MaP_Paper.Element id;
        public Sprite sprite;
        public Vector2 minMaxForClickX = Vector2.one;
        public Vector2 minMaxForClickY = Vector2.one;
        public Vector2 textPos = Vector2.one / 2;
        public string textContent_Default = "";
    }
    public List<ElementSpec> specList = new List<ElementSpec>();

    public ElementSpec GetSpecFromElement(UI_MaP_Paper.Element id)
    {
        foreach(var spec in specList)
        {
            if(spec.id == id)
            {
                return spec;
            }
        }
        Debug.LogError("Could not find any spec for " + id);
        return null;
    }
    [Sirenix.OdinInspector.Button]
    public void PopulateMissingId()
    {
        for (int i = 0; i < System.Enum.GetValues(typeof(UI_MaP_Paper.Element)).Length; i++)
        {
            UI_MaP_Paper.Element id = (UI_MaP_Paper.Element)i;
            if (GetSpecFromElement(id) == null)
            {
                ElementSpec newSpec = new ElementSpec();
                newSpec.id = id;
                specList.Add(newSpec);
            }
        }
    }
    #endregion
}
