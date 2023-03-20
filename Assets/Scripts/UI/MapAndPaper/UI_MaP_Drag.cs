using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class UI_MaP_Drag : UI_MaP_Overing
{
    public CanvasGroup himself;
    [Header("For prog")]
    [ReadOnly] [SerializeField] protected bool dragOn = false;
    [ReadOnly] [SerializeField] protected Vector3 lastMouseClickPosition = Vector3.zero;
    [ReadOnly] [SerializeField] protected Vector2 lastPosition;
    [ReadOnly] [SerializeField] protected Transform lastParent;
    [ReadOnly] [SerializeField] protected UI_MaP_Paper lastParent_Paper;
    [ReadOnly] [SerializeField] protected Vector2 lastOffset;

    [ReadOnly] [SerializeField] protected bool fromDragZone = false;
    [ReadOnly] [SerializeField] public bool firstDrag = false;

    [Header("Visual aspect")]
    protected RectTransform visualRect;
    [Tooltip("Define by screen percentage")] public Vector2 shadowDistance = new Vector2(0.01f, 0.005f);
    [Range(0, 1)] public float mouseDistanceToCreateClone = 0.03f;
    public float baseSize = 0.33f;



    public bool showDebug = false;
    public void Update()
    {
        if (showDebug)
            Debug.Log(OveringMe() ? "OveringMe " : "NOTvering me.");

        InputManagement();

        PlacementManagement();
    }



    protected virtual void InputManagement()
    {
        if (OveringMe() && Input.GetMouseButtonDown(0))
        {
            //create start point
            lastMouseClickPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (!fromDragZone)
            {
                FinishClickOnIt_WhileOnPaper();
            }
            lastMouseClickPosition = Vector3.zero;
        }


        if (Input.GetMouseButton(0) && lastMouseClickPosition != Vector3.zero)
        {
            float distance = Screen.width * mouseDistanceToCreateClone;
            if ((lastMouseClickPosition - Input.mousePosition).magnitude > distance)
            {
                BeginDrag();
                //And stop trying to see that.
                lastMouseClickPosition = Vector3.zero;
            }
        }
    }

    protected virtual void FinishClickOnIt_WhileOnPaper()
    {
        if (dragOn)
        {
            EndDrag();
        }
    }

    protected virtual void PlacementManagement()
    {
        if (dragOn)
        {
            Vector2 mousePos = Input.mousePosition; // for now, only the real mouse (later, the mouse can be move by joystick)
            this.transform.position = mousePos + lastOffset;

            if (lastOffset.magnitude > 0)
                lastOffset = Vector2.Lerp(lastOffset, Vector2.zero, Time.deltaTime * 4);

            GameManager.instance.mapAndPaper.currentPaper.MoveDependingOnMousePosition(this);
            //Correct Size
            if (GameManager.instance.mapAndPaper.currentPaper.OveringMe())
                this.transform.localScale = GameManager.instance.mapAndPaper.currentPaper.transform.localScale * baseSize;
            else
                this.transform.localScale = Vector3.one;
        }


    }





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

        GameManager.instance.mapAndPaper.currentPaper.ChangeRaycastBlockForDrag(false);
    }
    protected virtual void EndDrag()
    {
        Debug.Log("EndDrag : " + this.name, this.gameObject);
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
        GameManager.instance.mapAndPaper.currentPaper.ChangeRaycastBlockForDrag(true);
    }

    void TryDestroyAfterDrag()
    {
        if (fromDragZone)
            Debug.LogError("!!!Should not be Dragging !!! ", this.gameObject);
        else
            Destroy(this.gameObject);
    }

    protected abstract UI_MaP_Drag CreateClone();




    public virtual bool OveringMe()
    {
        Vector2 mousePos = Input.mousePosition;
        float zoom = this.transform.localScale.x;
        if (!fromDragZone)
        {
            zoom *= GameManager.instance.mapAndPaper.currentPaper.transform.localScale.x;
        }
        
            //Only icon
        float minX = visualRect.transform.position.x - visualRect.rect.width  * zoom / 2;
        float maxX = visualRect.transform.position.x + visualRect.rect.width  * zoom / 2;
        float minY = visualRect.transform.position.y - visualRect.rect.height * zoom / 2;
        float maxY = visualRect.transform.position.y + visualRect.rect.height * zoom / 2;



        if (minX < mousePos.x &&
            maxX > mousePos.x &&
            minY < mousePos.y &&
            maxY > mousePos.y)
        {
            return true;
        }

        return false;
    }


}
