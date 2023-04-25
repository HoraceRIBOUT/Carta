using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

//This is the manager for the "map" and "paper" part
public class UI_MapAndPaper : MonoBehaviour
{
    [ReadOnly] [SerializeField] private List<UI_MaP_Paper> papers = new List<UI_MaP_Paper>();
    public UI_MaP_Paper currentPaper;
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

        //For now :
        if(!papers.Contains(currentPaper))
            papers.Add(currentPaper);
    }

    public void IM_Open()
    {
        Open();
    }
    public void IM_Close()
    {
        Close();
    }

    public void Open()
    {
        if (openCloseCorout != null)
            return;
        if (mapOpen)
            return;

        mapOpen = true;
        openCloseCorout = StartCoroutine(OpenCloseMap());

        sideTab.UpdateIconList();
        sideTab.UpdateElementList();
        //iconZone.Switch(0);   //don't switch, because the button switch some other item too
        GameManager.instance.playerMove.InventoryAndMenu();

        GameManager.instance.dialogMng.InventoryOrMapOpen();

        //Set paper 

    }

    public void Paper_ResetPosition()
    {
        currentPaper.ResetPosAndScale();
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
        foreach(UI_MaP_Paper paper in papers)
        {
            res.Add(paper.GetSaveData());
        }
        return res;
    }


    public void ApplySaveData(List<UI_MaP_Paper.Paper_SaveData> papersData, List<IconData.Icon_SaveData> iconsData, List<pnj.pnjID> pnjAlreadyMet)
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

        //Clean all papers
        foreach (UI_MaP_Paper.Paper_SaveData paperData in papersData)
        {
            //TO DO : 
            //reach and destroy all paper (maybe paper are not instantiate and simply all already exists ? Maybe a little simpler)
            //and give data to each loaded paper after that
        }
        //For now : 
        currentPaper.ApplySaveData(papersData[0]);
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
