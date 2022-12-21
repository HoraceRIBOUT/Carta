using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_MaP_Element : MonoBehaviour
{
    //Data : 
    //what element it is (just the sprite)
    //what transform rect it is (pos, scale and scale + size in pixel)

    public CanvasGroup himself;

    public Vector2 lastPosition;
    public Transform lastParent;

    public Vector2 lastOffset;

    public bool fromElementZone = false;
    public bool dragOn = false;

    public UI_MaP_Paper.Element data;

    public void Create(/*Scriptable data for icon */ bool onIconZone)
    {
        fromElementZone = onIconZone;


        //Icon text
        //Icon image
        //Different data
        //Misc too
        //also have a button to go on more depth on it
    }


    public void BeginDrag()
    {
        //If taken from the iconList :
        //  should create a new one, and the new one is the one being dragged.
        //  

        //When we will have a custom mouse for this menu : take the "lastPos" of the mouse , to avoid the "BeginDrag" being to late
        Vector2 mousePos = Input.mousePosition; // for now, only the real mouse (later, the mouse can be move by joystick)
        lastOffset = (Vector2)this.transform.position - mousePos;

        transform.SetParent(GameManager.instance.canvasGeneral);

        himself.blocksRaycasts = false;
        dragOn = true;
    }
    public void EndDrag()
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
            GameManager.instance.mapAndPaper.currentPaper.AddElement(this);
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
/*
    public void Update()
    {
        if (dragOn)
        {
            
        }
        else if(fromElementZone)
        {

        }
    }*/



}
