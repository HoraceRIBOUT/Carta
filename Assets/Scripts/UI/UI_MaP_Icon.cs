using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_MaP_Icon : MonoBehaviour
{
    //Data : 
    //  who is it ?                 (pnj)
    //  where is it ?               (vector2, relative to the paper)
    //  / data link to database ?   (name, surname, description etc..?)
    //  / L-> what img to use ?           (sprite) (is in data)
    // 

    //Data is scriptables
    //but the Icon is just a position and so, can be cloned !

    //Ok, so there is the clonable part (this)
    //and there is the scriptable part (data)
    //this have mostly function and data related to that
    //while the scriptable have the above info

    public CanvasGroup infoPart;

    public Vector2 lastPosition;
    public GameObject lastParent;

    public Vector2 lastOffset;

    public bool dragOn = false;
    public void BeginDrag()
    {
        //If taken from the iconList :
        //  should create a new one, and the new one is the one being dragged.
        //  

        Vector2 mousePos = Input.mousePosition; // for now, only the real mouse (later, the mouse can be move by joystick)
        lastOffset = (Vector2)this.transform.position - mousePos;


        dragOn = true;
    }
    public void EndDrag()
    {
        //Need to see what under it. 
        //How ?

        //if drag on the icon list : 
        //  delete it.
        //else : if on the paper 
        //  add to the paper list AND register at the paper script that you where add at this adress (transformPos)
        //      (paper will need to made a conversion to taka account of offset + zoom)

        //else : retur to last pos. Immediatly. (or maybe a "SHLIIIIING" in like, half a second (coroutine + lerp))


        dragOn = false;
    }

    public void Update()
    {
        if (dragOn)
        {
            Vector2 mousePos = Input.mousePosition; // for now, only the real mouse (later, the mouse can be move by joystick)
            this.transform.position = mousePos + lastOffset;

            if (infoPart.alpha >= 0)
                infoPart.alpha -= Time.deltaTime * 4;
        }
        else
        {
            //if not drag BUT pointerOver : display info
            if (infoPart.alpha <= 1)
                infoPart.alpha += Time.deltaTime * 2;
        }
    }



}
