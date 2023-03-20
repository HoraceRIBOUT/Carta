using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MaP_Element : UI_MaP_Drag
{
    //Data : 
    //what element it is (just the sprite)
    //what transform rect it is (pos, scale and scale + size in pixel)
    public UI_MaP_Paper.Element data;

    public RectTransform himselfRect;
    [SerializeField] private Image spriteRdr;
    [SerializeField] private Image spriteRdr_shadow;
    RectTransform spriteRect;
    [SerializeField] private List<Sprite> spriteList;//for now, here. TO DO : refacto this later to place it in a more logic place.

    protected override UI_MaP_Drag CreateClone()
    {
        UI_MaP_Element el = Instantiate(this, GameManager.instance.mapAndPaper.iconZone.iconParent);
        el.Create(data, false);
        el.transform.position = this.transform.position;
        el.transform.localScale = Vector3.one;
        el.firstDrag = true;
        return el;
    }

    public void Create(UI_MaP_Paper.Element newData, bool createOnZone)
    {
        fromDragZone = createOnZone;

        himselfRect = this.GetComponent<RectTransform>();
        visualRect = himselfRect.GetChild(0).GetComponent<RectTransform>();
        spriteRdr.sprite = spriteList[(int)newData]; 
        spriteRdr_shadow.sprite = spriteList[(int)newData];
        spriteRect = spriteRdr.GetComponent<RectTransform>();
        data = newData;
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


    public void ReplaceOnpaper(UI_MaP_Paper.ElementPos data)
    {
        spriteRdr.color = data.color;
        spriteRdr.sprite = spriteList[(int)data.data]; 
        spriteRdr_shadow.sprite = spriteList[(int)data.data];
        this.transform.localPosition = data.positionRelative;
        this.transform.localRotation = Quaternion.Euler(data.rotationRelative);
        this.transform.localScale = data.scaleRelative;
    }
}
