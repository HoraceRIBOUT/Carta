using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MaP_Icon : UI_MaP_Overing
{
    //Data : 
    //  who is it ?                 (pnj)
    //  where is it ?               (vector2, relative to the paper)
    //  / data link to database ?   (name, surname, description etc..?)
    //  / L-> what img to use ?           (sprite) (is in data)
    // 

    //Data is scriptables
    //but the Icon is just a position and so, can be cloned !
    public IconData data;

    //Ok, so there is the clonable part (this)
    //and there is the scriptable part (data)
    //this have mostly function and data related to that
    //while the scriptable have the above info

    public CanvasGroup himself;
    public RectTransform himselfRect;
    public CanvasGroup infoPart;

    public Vector2 lastPosition;
    public Transform lastParent;
    public UI_MaP_Paper lastParent_Paper;
    public bool fromIconZone = false;

    [Header("For populate part")]
    public Image iconImage;
    public TMPro.TMP_Text mainName;
    public TMPro.TMP_Text surName;
    public TMPro.TMP_Text username;
    public TMPro.TMP_Text activity;
    //Somehow, relationship

    public Vector2 lastOffset;

    public bool dragOn = false;

    public void Create(IconData data, bool onIconZone)
    {
        fromIconZone = onIconZone;
        this.data = data;

        //Icon text
        iconImage.sprite = data.spriteLittleIcon;
        //Icon image
        mainName.SetText(data.mainName);
        surName.SetText(data.surName);
        username.SetText(data.username);
        activity.SetText(data.activity);
        //Misc too
        //also have a button to go on more depth on it
        //but later

        himselfRect = this.GetComponent<RectTransform>();
    }


    private void BeginDrag()
    {
        if (fromIconZone)
        {
            //If taken from the iconList :
            //  should create a new one, and the new one is the one being dragged.
            //  
            UI_MaP_Icon ic = Instantiate(this);
            ic.Create(data, false);
            ic.transform.position = this.transform.position;
            ic.transform.localScale = GameManager.instance.mapAndPaper.currentPaper.transform.localScale;
        }
        else
        {
            //When we will have a custom mouse for this menu : take the "lastPos" of the mouse , to avoid the "BeginDrag" being to late
            Vector2 mousePos = Input.mousePosition; // for now, only the real mouse (later, the mouse can be move by joystick)
            lastOffset = (Vector2)this.transform.position - mousePos;

            transform.SetParent(GameManager.instance.canvasGeneral);
            if (lastParent_Paper != null)
            {
                lastParent_Paper.RemoveIcon(this);
                lastParent_Paper = null;
            }

            himself.blocksRaycasts = false;
            dragOn = true;
        }
       

        //if taken from a paper : 
        //should unlinked themself immediately

    }
    private void EndDrag()
    {
        //Need to see what under it. 
        if (GameManager.instance.mapAndPaper.iconZone.overing)
        {
            //Go bakc to the icon zone

            //so technically, destroy this instance


            //for now : just change parent
            this.transform.SetParent(GameManager.instance.mapAndPaper.iconZone.iconParent);
        }
        else if(GameManager.instance.mapAndPaper.currentPaper.overing)
        {
            this.transform.SetParent(GameManager.instance.mapAndPaper.currentPaper.iconParent);
            lastParent_Paper = GameManager.instance.mapAndPaper.currentPaper;
            GameManager.instance.mapAndPaper.currentPaper.AddIcon(this);
        }
        else
        {
            this.transform.SetParent(lastParent);
            this.transform.position = lastPosition;
        }
        lastParent = this.transform.parent;

        //if drag on the icon list : 
        //  delete it.
        //else : if on the paper 
        //  add to the paper list AND register at the paper script that you where add at this adress (transformPos)
        //      (paper will need to made a conversion to taka account of offset + zoom)

        //else : retur to last pos. Immediatly. (or maybe a "SHLIIIIING" in like, half a second (coroutine + lerp))

        himself.blocksRaycasts = true;

        dragOn = false;
    }

    public void Update()
    {
        if (overing && Input.GetMouseButtonDown(0))
        {
            BeginDrag();
        }
        else if (dragOn && Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }

        if (dragOn)
        {
            Vector2 mousePos = Input.mousePosition; // for now, only the real mouse (later, the mouse can be move by joystick)
            this.transform.position = mousePos + lastOffset;

            if (infoPart.alpha >= 0)
                infoPart.alpha -= Time.deltaTime * 4;

            this.transform.localScale = GameManager.instance.mapAndPaper.currentPaper.transform.localScale;
        }
        else if(fromIconZone)
        {
            //if not drag BUT pointerOver : display info
            if (infoPart.alpha <= 1)
                infoPart.alpha += Time.deltaTime * 2;
        }
    }



}
