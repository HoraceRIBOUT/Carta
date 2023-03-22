using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

//This is the manager for the "map" and "paper" part
public class UI_MapAndPaper : MonoBehaviour
{
    [ReadOnly] [SerializeField] private List<UI_MaP_Paper> papers = new List<UI_MaP_Paper>();
    public UI_MaP_Paper currentPaper;
    public UI_MaP_IconInfoZone iconZone;
    //So, by default, have one blank paper 
    public bool mapOpen = false;
    public bool currentlyEditingText = false;

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

        iconZone.UpdateIconList();
        iconZone.UpdateElementList();
        iconZone.Switch(0);
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
        currentlyEditingText = false;
    }

    private IEnumerator OpenCloseMap()
    {
        Debug.Log("Coroutine");
        _anima.SetBool("Open", mapOpen);
        yield return new WaitForSeconds(0.3f);
        openCloseCorout = null;
    }



    [System.Serializable]
    public class ElementSpec
    {
        public UI_MaP_Paper.Element id;
        public Vector2 minMaxForClickX = Vector2.one;
        public Vector2 minMaxForClickY = Vector2.one;
        public Vector2 center = Vector2.one / 2;
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

}
