using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MaP_Element : UI_MaP_Drag
{
    //Data : 
    public UI_MaP_Paper.Element data;
    public UI_MapAndPaper.ElementSpec spec;

    public RectTransform himselfRect;
    [SerializeField] private Image spriteRdr;
    [SerializeField] private Image spriteRdr_shadow;
    RectTransform spriteRect;
    [SerializeField] private List<Sprite> spriteList;//for now, here. TO DO : refacto this later to place it in a more logic place.
    [SerializeField] private bool showText;
    [SerializeField] private TMPro.TMP_InputField customText;

    public bool ShowText { get => showText; private set => showText = value; }
    public Color Color { get => spriteRdr.color; private set => spriteRdr.color = value; }

    protected override UI_MaP_Drag CreateClone()
    {
        UI_MaP_Element el = Instantiate(this, GameManager.instance.mapAndPaper.sideTab.iconParent);
        el.Create(data, false, ShowText);
        el.transform.position = this.transform.position;
        el.transform.localScale = Vector3.one;
        el.DisplayText(ShowText);
        el.firstDrag = true;
        return el;
    }

    public void Create(UI_MaP_Paper.Element newData, bool createOnZone, bool textShown)
    {
        fromDragZone = createOnZone;

        himselfRect = this.GetComponent<RectTransform>();
        visualRect = himselfRect.GetChild(0).GetComponent<RectTransform>();
        spriteRdr.sprite = spriteList[(int)newData]; 
        spriteRdr_shadow.sprite = spriteList[(int)newData];
        spriteRect = spriteRdr.GetComponent<RectTransform>();
        data = newData;
        spec = GameManager.instance.mapAndPaper.GetSpecFromElement(newData);
        customText.SetTextWithoutNotify(spec.textContent_Default);
        ReplaceTextField();
        ShowText = textShown;
        DisplayText(ShowText);
    }

    public void ReplaceTextField()
    {
        Vector2 localTextPos;
        localTextPos.x = (spec.textPos.x - 0.5f) * visualRect.rect.width;
        localTextPos.y = (spec.textPos.y - 0.5f) * visualRect.rect.height;
        customText.transform.localPosition = localTextPos;
        //Debug.Log(this.gameObject.name + " is at " + localTextPos);

        if (spec.textPos.x < 0.25f)
        {
            customText.textComponent.alignment = TMPro.TextAlignmentOptions.Right;
        }
        if (spec.textPos.x > 0.75f)
        {
            customText.textComponent.alignment = TMPro.TextAlignmentOptions.Left;
        }
    }

    public void DisplayText(bool on)
    {
        Debug.Log(on ? "show" : "hide");
        customText.interactable = on;
        customText.gameObject.SetActive(on);
        ShowText = on;
    }

    protected override void PlacementManagement()
    {
        base.PlacementManagement();

        if (dragOn)
        {
            Vector2 targetSize = new Vector2(Screen.width * shadowDistance.x, Screen.height * shadowDistance.y);
            spriteRect.anchoredPosition = Vector2.Lerp(spriteRect.anchoredPosition, targetSize, Time.deltaTime * 4);
        }
        else
        {
            Vector2 targetSize = Vector2.zero;
            spriteRect.anchoredPosition = Vector2.Lerp(spriteRect.anchoredPosition, targetSize, Time.deltaTime * 4);
        }
    }

    public string GetText()
    {
        return customText.text;
    }

    public void ReplaceOnpaper(UI_MaP_Paper.ElementPos savedData)
    {
        data = savedData.id;
        spriteRdr.color = savedData.GetColor();
        spriteRdr.sprite = spriteList[(int)savedData.id]; 
        spriteRdr_shadow.sprite = spriteList[(int)savedData.id];
        this.transform.localPosition = savedData.GetPositionRelative();
        //this.transform.localRotation = Quaternion.Euler(savedData.rotationRelative);
        //this.transform.localScale = savedData.scaleRelative;
        
        DisplayText(savedData.showText);
        customText.SetTextWithoutNotify(savedData.text);
    }

    public override bool OveringMe()
    {
        Vector2 mousePos = Input.mousePosition;
        float zoom = this.transform.localScale.x;
        if (!fromDragZone)
        {
            zoom *= GameManager.instance.mapAndPaper.currentPaper.transform.localScale.x;
        }

        //Only icon
        float minX = visualRect.transform.position.x - visualRect.rect.width  * (0.5f - spec.minMaxForClickX.x) * 2 * zoom/2; 
        float maxX = visualRect.transform.position.x + visualRect.rect.width  * (spec.minMaxForClickX.y - 0.5f) * 2 * zoom/2;
        float minY = visualRect.transform.position.y - visualRect.rect.height * (0.5f - spec.minMaxForClickY.x) * 2 * zoom/2;
        float maxY = visualRect.transform.position.y + visualRect.rect.height * (spec.minMaxForClickY.y - 0.5f) * 2 * zoom/2;



        if (minX < mousePos.x &&
            maxX > mousePos.x &&
            minY < mousePos.y &&
            maxY > mousePos.y)
        {
            return true;
        }

        return false;
    }

/*
    public RectTransform dbg_ElementUP;
    public RectTransform dbg_ElementDO;
    public RectTransform dbg_ElementLE;
    public RectTransform dbg_ElementRI;
    [Sirenix.OdinInspector.Button()]
    public void dbg_ResizeRectToBeTheClickingBordure()
    {
        float zoom = this.transform.localScale.x;
        if (!fromDragZone && GameManager.instance != null)
        {
            zoom *= GameManager.instance.mapAndPaper.currentPaper.transform.localScale.x;
        }
        if(visualRect == null)
        {
            visualRect = himselfRect.GetChild(0).GetComponent<RectTransform>();
        }

        //Only icon
        float minX = visualRect.transform.position.x - visualRect.rect.width  * (0.5f - spec.minMaxForClickX.x) * 2 * zoom / 2;
        float maxX = visualRect.transform.position.x + visualRect.rect.width  * (spec.minMaxForClickX.y - 0.5f) * 2 * zoom / 2;
        float minY = visualRect.transform.position.y - visualRect.rect.height * (0.5f - spec.minMaxForClickY.x) * 2 * zoom / 2;
        float maxY = visualRect.transform.position.y + visualRect.rect.height * (spec.minMaxForClickY.y - 0.5f) * 2 * zoom / 2;


        Vector2 newSize = new Vector2(maxX - minX, maxY - minY);
        Vector2 newPos = new Vector2(maxX + minX, maxY + minY); 
        newPos /= 2f;
        dbg_ElementUP.localPosition = new Vector2(maxX, maxY);
        dbg_ElementDO.localPosition = new Vector2(minX, minY);
        dbg_ElementRI.localPosition = new Vector2(maxX, minY);
        dbg_ElementLE.localPosition = new Vector2(minX, maxY);
        //dbg_Element.sizeDelta = newSize;
        Debug.Log("Ok so :" + newPos + " and" + newSize + " mouse is on " + Input.mousePosition);
    }*/

}
