using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_MaP_SideTab : UI_MaP_IconDropZone
{
    public RectTransform rect;


    public UI_MaP_Icon iconPrefab;
    public UI_MaP_Element elemPrefab;
    [Header("Scroll")]
    public float mouseScrollSpeed = 1;
    public Vector2 mouseScrollMinMax = new Vector2(0.8f, 5f);
    private Vector3 startLocalPosition_ic = Vector2.zero;
    private Vector3 startLocalPosition_el = Vector2.zero;

    [Header("Normally, set by game")]
    public List<pnj.pnjID> pnjToDeploy = new List<pnj.pnjID>();
    public int currentTabIndex = 0;

    [Header("Data")]
    public List<IconData> dataFromPnj;
    public bool showText = false;

    [Header("For icon position")]
    [Range( 0, 1)] [Sirenix.OdinInspector.OnValueChanged("ChangeSize_Icon")] public float positionX_ic = 0.3f;
    [Range( 0, 1)] [Sirenix.OdinInspector.OnValueChanged("ChangeSize_Icon")] public float positionY_ic = 0.3f;
    [Range( 0, 1)] [Sirenix.OdinInspector.OnValueChanged("ChangeSize_Icon")] public float marginX_ic = 0.1f;
    [Range(-1, 0)] [Sirenix.OdinInspector.OnValueChanged("ChangeSize_Icon")] public float marginY_ic = -0.1f;
    [Range(-1, 1)] [Sirenix.OdinInspector.OnValueChanged("ChangeSize_Icon")] public float size_ic = 0.1f;

    [Header("For element position")]
    [Range(0,  1)] [Sirenix.OdinInspector.OnValueChanged("ChangeSize_Element")] public float positionX_el = 0.3f;
    [Range(0,  1)] [Sirenix.OdinInspector.OnValueChanged("ChangeSize_Element")] public float positionY_el = 0.3f;
    [Range(0,  1)] [Sirenix.OdinInspector.OnValueChanged("ChangeSize_Element")] public float marginX_el   = 0.1f;
    [Range(-1, 0)] [Sirenix.OdinInspector.OnValueChanged("ChangeSize_Element")] public float marginY_el   =-0.1f;
    [Range(-1, 1)] [Sirenix.OdinInspector.OnValueChanged("ChangeSize_Element")] public float size_el      = 0.1f;
    Vector2 fullSize;
    public RectTransform dolly;

    [Sirenix.OdinInspector.Button()]
    public void Switch()
    {
        currentTabIndex = 1 - currentTabIndex; //easy way to switch it because there's only too for now
        Switch(currentTabIndex);
    }

    /// <summary>
    /// Index 0 : icone. Index 1 : element.
    /// </summary>
    /// <param name="tabIndex"></param>
    public void Switch(int tabIndex)
    {
        //TO DO
        foreach(var ic in iconsGO)            ic.gameObject.SetActive(tabIndex==0);
        foreach(var el in elementsGO)         el.gameObject.SetActive(tabIndex==1);
        currentTabIndex = tabIndex;
    }

    public void Start()
    {
        rect = GetComponent<RectTransform>();
        fullSize = dolly.rect.size;
        startLocalPosition_ic = iconParent.transform.localPosition;
        startLocalPosition_el = elementParent.transform.localPosition;
    }

    public void Update()
    {
        if (Input.mouseScrollDelta.y != 0 && OveringMe())
        {
            float ySize = rect.rect.height;

            Transform transformToScroll = (currentTabIndex == 0 ? iconParent.transform  : elementParent.transform);
            Vector3  startLocalPosition = (currentTabIndex == 0 ? startLocalPosition_ic : startLocalPosition_el);
            //define : mouseScrollMinMax by the number of element in it (iconsGO vs elementGO, roughly the same size)

            Debug.Log("Scrool ! " + Input.mouseScrollDelta.y + " rect ? " + ySize);
            transformToScroll.transform.localPosition += Input.mouseScrollDelta.y * -mouseScrollSpeed * ySize * Time.deltaTime * Vector3.up;

            if (transformToScroll.transform.localPosition.y < startLocalPosition.y + mouseScrollMinMax.x * ySize)
            {
                transformToScroll.transform.localPosition = startLocalPosition + Vector3.up * mouseScrollMinMax.x * ySize;
            }
            else if (transformToScroll.transform.localPosition.y > startLocalPosition.y + mouseScrollMinMax.y * ySize)
            {
                transformToScroll.transform.localPosition = startLocalPosition + Vector3.up * mouseScrollMinMax.y * ySize;
            }
        }
    }

    public void ChangeSize_Icon()
    {
        if (!Application.isPlaying)
            return;

        for (int i = 0; i < iconsGO.Count; i++)
        {
            Destroy(iconsGO[i].gameObject);
        }
        iconsGO.Clear();
        UpdateIconList();
    }
    public void ChangeSize_Element()
    {
        if (!Application.isPlaying)
            return;

        for (int i = 0; i < elementsGO.Count; i++)
        {
            Destroy(elementsGO[i].gameObject);
        }
        elementsGO.Clear();
        UpdateElementList();
    }

    public void ApplySaveData(List<pnj.pnjID> pnjAlreadyMet)
    {
        foreach (UI_MaP_Icon iconGO in iconsGO)
        {
            Destroy(iconGO.gameObject);
        }
        iconsGO.Clear();
        pnjToDeploy = pnjAlreadyMet;
    }

    [Sirenix.OdinInspector.Button]
    public void UpdateIconList()
    {
        //fullSize = dolly.rect.size;

        for (int i = 0; i < pnjToDeploy.Count; i++)
        {
            if (IconAlreadyDeployed(pnjToDeploy[i]))
            {   
                continue;
            }

            IconData data = GetDataForThisPJN(pnjToDeploy[i]);
            if (data == null)
                continue;
            UI_MaP_Icon newIcon = Instantiate(iconPrefab);
            newIcon.transform.SetParent(iconParent);
            newIcon.Create(data, true);

            iconsGO.Add(newIcon);
        }
        UpdateIconPosition();
    }

    public void UpdateIconPosition()
    {
        startLocalPosition_ic = iconParent.transform.localPosition;
        for (int i = 0; i < iconsGO.Count; i++)
        {
            UI_MaP_Icon iconToReplace = iconsGO[i];
            int xPos = i % 2;
            int yPos = (int)Mathf.Floor(i / 2);
            iconToReplace.himselfRect.anchoredPosition = new Vector3(
                xPos * positionX_ic * fullSize.x + marginX_ic * fullSize.x,
               -yPos * positionY_ic * fullSize.y + marginY_ic * fullSize.y,
                0);

            Debug.Log(iconToReplace.himselfRect.anchoredPosition + "Repositoon icon : " + -yPos + " * " + positionY_ic + " * " + fullSize.y + " + " + marginY_ic);
            iconToReplace.transform.localRotation = Quaternion.identity;
            iconToReplace.transform.localScale = Vector3.one;
            iconToReplace.himselfRect.sizeDelta = new Vector2(
                fullSize.x * size_ic,
                fullSize.x * size_ic);

            iconToReplace.ReUpdateFromData();
        }
    }



    [Sirenix.OdinInspector.Button]
    public void UpdateElementList()
    {
        //fullSize = dolly.rect.size;

        if (elementsGO.Count == 0) 
        {
            for (int i = 0; i < System.Enum.GetValues(typeof(UI_MaP_Paper.Element)).Length; i++)
            {
                UI_MaP_Paper.Element id = (UI_MaP_Paper.Element)i;

                UI_MaP_Element newElement = Instantiate(elemPrefab);
                newElement.transform.SetParent(elementParent);
                newElement.Create(id, true, showText);

                elementsGO.Add(newElement);
            }
            UpdateElementPosition();
        }
        
    }

    public void UpdateElementPosition()
    {
        startLocalPosition_el = elementParent.transform.localPosition;
        for (int i = 0; i < elementsGO.Count; i++)
        {
            UI_MaP_Element elemToReplace = elementsGO[i];
            int xPos = i % 2;
            int yPos = (int)Mathf.Floor(i / 2);
            elemToReplace.himselfRect.localPosition = new Vector3(
                xPos * positionX_el * fullSize.x + marginX_el * fullSize.x,
               -yPos * positionY_el * fullSize.y + marginY_el * fullSize.y,
                0);
            elemToReplace.transform.localRotation = Quaternion.identity;
            elemToReplace.transform.localScale = Vector3.one;
            elemToReplace.himselfRect.sizeDelta = new Vector2(
                fullSize.x * size_el,
                fullSize.x * size_el);

            elemToReplace.ReplaceTextField();
        }
        Debug.Log("Place at the right place");
    }



    public bool IconAlreadyDeployed(pnj.pnjID pnjToTest)
    {
        foreach (UI_MaP_Icon ic in iconsGO)
        {
            if(ic.data.id == pnjToTest)
            {
                return true;
            }
        }
        return false;
    }

    public IconData GetDataForThisPJN(pnj.pnjID id)
    {
        foreach (IconData data in dataFromPnj)
        {
            if (data.id == id)
            {
                return data;
            }
        }
        Debug.LogError(id + " is not in the data list. Please, add the IconData scriptable in the list on the ListIcon gameObject.");
        return null;
    }

    public List<IconData.Icon_SaveData> GetSaveData()
    {
        List<IconData.Icon_SaveData> res = new List<IconData.Icon_SaveData>();
        foreach (UI_MaP_Icon icon in iconsGO)
        {
            res.Add(icon.data.GetSerialazableIconData());
        }
        return res;
    }


    public void AddIconIfNeeded(pnj.pnjID iconId)
    {
        if (pnjToDeploy.Contains(iconId))
            return;
        foreach (IconData data in dataFromPnj)
        {
            if(data.id == iconId)
            {
                if (DemoPNJ(iconId))
                {
                    pnjToDeploy.Add(iconId);
                }

            }
        }
    }

    public bool DemoPNJ(pnj.pnjID iconId)
    {
        switch (iconId)
        {
            case pnj.pnjID.postWoman:
            case pnj.pnjID.guitar:
            case pnj.pnjID.babiol:
            case pnj.pnjID.flowerMom:
            case pnj.pnjID.climbrDad:
            case pnj.pnjID.stagiaire:
            case pnj.pnjID.flowerKid:
            case pnj.pnjID.crowCool:
            case pnj.pnjID.tomb_grandad:
            case pnj.pnjID.tomb_mom:
            case pnj.pnjID.tomb_infant:
                return true;
            default:
                return false;
        }
    }


    public void ToggleValueChanged(bool value)
    {
        Debug.Log("New Value : " + value);
        showText = value;
        foreach(var element in elementsGO)
        {
            element.DisplayText(value);
        }
    }




    public bool OveringMe()
    {
        Vector3 mousePos = Input.mousePosition;

        if (Screen.width * rect.anchorMin.x < mousePos.x &&
            Screen.width * rect.anchorMax.x > mousePos.x &&
            Screen.height * rect.anchorMin.y < mousePos.y &&
            Screen.height * rect.anchorMax.y > mousePos.y)
        {
            return true;
        }
        return false;
    }

    public float LeftBorderPositionInScreenPercentage()
    {
        return rect.anchorMin.x;
    }

    private void OnDestroy()
    {
        foreach(IconData data in dataFromPnj)
        {
            data.nameText = "";
            data.descText = "";
        }
    }

}
