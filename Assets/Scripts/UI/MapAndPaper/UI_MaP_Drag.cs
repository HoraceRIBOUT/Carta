using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UI_MaP_Drag : UI_MaP_Overing
{
    public CanvasGroup himself;
    [Header("For prog")]
    [SerializeField] protected bool dragOn = false;
    [SerializeField] protected Vector3 lastMouseClickPosition = Vector3.zero;
    [SerializeField] protected Vector2 lastPosition;
    [SerializeField] protected Transform lastParent;
    [SerializeField] protected UI_MaP_Paper lastParent_Paper;
    [SerializeField] protected Vector2 lastOffset;

    public bool fromDragZone = false;
    public bool firstDrag = false;
    public float baseSize = 0.33f;

    protected virtual void BeginDrag()
    {
        //To avoid the multiple icon being take
        if (!himself.blocksRaycasts)
            return;

        UI_MaP_Drag iconToDrag = this;
        if (fromDragZone)
        {
            //If taken from the iconList :
            //  should create a new one, and the new one is the one being dragged.
            //  
            UI_MaP_Drag ic = CreateClone();

            iconToDrag = ic;
        }
        //When we will have a custom mouse for this menu : take the "lastPos" of the mouse , to avoid the "BeginDrag" being to late
        Vector2 mousePos = Input.mousePosition; // for now, only the real mouse (later, the mouse can be move by joystick)
        iconToDrag.lastOffset = (Vector2)this.transform.position - mousePos;
        iconToDrag.lastPosition = this.transform.localPosition;

        iconToDrag.transform.SetParent(GameManager.instance.mapAndPaper.aboveMaP);
        if (iconToDrag.lastParent_Paper != null)
        {
            iconToDrag.lastParent_Paper.RemoveDrag(iconToDrag);
            iconToDrag.lastParent_Paper = null;
        }

        iconToDrag.himself.blocksRaycasts = false;
        iconToDrag.dragOn = true;
        //Debug.Log("Start drag. " + this.name, this.gameObject);

        GameManager.instance.mapAndPaper.currentPaper.ChangeRaycastBlockForIcon(false);
    }
    protected virtual void EndDrag()
    {
        UI_MaP_Paper currentPaper = GameManager.instance.mapAndPaper.currentPaper;
        //Need to see what under it. 
        if (GameManager.instance.mapAndPaper.iconZone.OveringMe())
        {
            TryDestroyAfterDrag();
        }
        else if (currentPaper.OveringMe())
        {
            this.transform.SetParent(currentPaper.iconParent);
            lastParent_Paper = currentPaper;
            currentPaper.AddDrag(this);
        }
        else
        {
            if (lastParent == null)
                TryDestroyAfterDrag();
            this.transform.SetParent(lastParent);
            this.transform.localPosition = lastPosition;
            this.transform.localScale = GameManager.instance.mapAndPaper.currentPaper.transform.localScale * baseSize;
            //Re add the icon on the paper
            lastParent_Paper = currentPaper;
            currentPaper.AddDrag(this);
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
        //Debug.Log("Finish drag." + this.name, this.gameObject);
        GameManager.instance.mapAndPaper.currentPaper.ChangeRaycastBlockForIcon(true);
    }

    void TryDestroyAfterDrag()
    {
        if (fromDragZone)
            Debug.LogError("!!!Should not be Dragging !!! ", this.gameObject);
        else
            Destroy(this.gameObject);
    }

    protected abstract UI_MaP_Drag CreateClone();



}
