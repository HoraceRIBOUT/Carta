using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MaP_Icon : UI_MaP_Overing
{
    [Header("Data")]
    //Data is scriptables
    //but the Icon is just a position and so, can be cloned !
    public IconData data;

    [Header("Element")]
    //Position and info :
    public CanvasGroup himself;
    public RectTransform himselfRect;
    public CanvasGroup editionPart;

    [Header("For populate part")]
    public Image iconImage;
    public TMPro.TMP_InputField name_textField;
    public TMPro.TMP_InputField desc_textField;

    [Header("For prog")]
    [SerializeField] private Vector2 lastPosition;
    [SerializeField] private Vector3 lastMouseClickPosition = Vector3.zero;
    [SerializeField] private Transform lastParent;
    [SerializeField] private UI_MaP_Paper lastParent_Paper;
    [SerializeField] private Vector2 lastOffset;
    [SerializeField] private bool editMode = false;
    [SerializeField] private bool dragOn = false;
    //Somehow, relationship

    public bool fromIconZone = false; 
    public float baseSize = 0.33f;
    [Range(0, 1)] public float mouseDistanceToCreateClone = 0.1f;


    public void Create(IconData data, bool onIconZone)
    {
        this.name = "Icon " + data.id +  (onIconZone?" fromZone":" for page.");
        fromIconZone = onIconZone;
        this.data = data;

        //Icon text
        iconImage.sprite = data.spriteLittleIcon;
        //Icon image
        name_textField.SetTextWithoutNotify(data.nameText);
        desc_textField.SetTextWithoutNotify(data.descText);
        //Misc too
        //also have a button to go on more depth on it
        //but later

        himselfRect = this.GetComponent<RectTransform>();

        if (fromIconZone)
            LaunchEditMode();
    }

    public void ReUpdateFromData()
    {
        //Icon text
        iconImage.sprite = data.spriteLittleIcon;
        //Icon image
        name_textField.SetTextWithoutNotify(data.nameText);
        desc_textField.SetTextWithoutNotify(data.descText);
    }

    public void OnNameInputFieldChange()
    {
        data.nameText = name_textField.text;
        if (!fromIconZone)
        {
            GameManager.instance.mapAndPaper.currentPaper.ReUpdateIconFromData();
        }
    }
    public void OnDescInputFieldChange()
    {
        data.descText = desc_textField.text;
        if (!fromIconZone)
        {
            GameManager.instance.mapAndPaper.currentPaper.ReUpdateIconFromData();
        }
    }

    private void BeginDrag()
    {
        UI_MaP_Icon iconToDrag = this;
        if (fromIconZone)
        {
            //If taken from the iconList :
            //  should create a new one, and the new one is the one being dragged.
            //  
            UI_MaP_Icon ic = Instantiate(this, GameManager.instance.mapAndPaper.iconZone.iconParent);
            ic.Create(data, false);
            ic.transform.position = this.transform.position;
            ic.transform.localScale = Vector3.one;

            iconToDrag = ic;
        }
        //When we will have a custom mouse for this menu : take the "lastPos" of the mouse , to avoid the "BeginDrag" being to late
        Vector2 mousePos = Input.mousePosition; // for now, only the real mouse (later, the mouse can be move by joystick)
        iconToDrag.lastOffset = (Vector2)this.transform.position - mousePos;
        iconToDrag.lastPosition = this.transform.localPosition;

        iconToDrag.transform.SetParent(GameManager.instance.mapAndPaper.aboveMaP);
        if (iconToDrag.lastParent_Paper != null)
        {
            iconToDrag.lastParent_Paper.RemoveIcon(iconToDrag);
            iconToDrag.lastParent_Paper = null;
        }

        iconToDrag.himself.blocksRaycasts = false;
        iconToDrag.dragOn = true;
        //Debug.Log("Start drag. " + this.name, this.gameObject);

        GameManager.instance.mapAndPaper.currentPaper.ChangeRaycastBlockForIcon(false);
    }
    private void EndDrag()
    {
        UI_MaP_Paper currentPaper = GameManager.instance.mapAndPaper.currentPaper;
        //Need to see what under it. 
        if (GameManager.instance.mapAndPaper.iconZone.overing)
        {
            TryDestroyAfterDrag();
        }
        else if(currentPaper.overing)
        {
            this.transform.SetParent(currentPaper.iconParent);
            lastParent_Paper = currentPaper;
            currentPaper.AddIcon(this);
        }
        else
        {
            if (lastParent == null)
                TryDestroyAfterDrag();
            this.transform.SetParent(lastParent);
            this.transform.localPosition = lastPosition;
            //this.transform.localScale = lastScale;
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
        if (fromIconZone)
            Debug.LogError("!!!Should not be Dragging !!! ", this.gameObject);
        else
            Destroy(this.gameObject);
    }

    public void Update()
    {
        //Ok so 

        InputManagement();

        PlacementManagement();
    }

    void InputManagement()
    {
        if (overing && Input.GetMouseButtonDown(0))
        {
            //create start point
            lastMouseClickPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (!fromIconZone)
            {
                if (lastMouseClickPosition != Vector3.zero)
                {
                    GameManager.instance.mapAndPaper.currentPaper.QuitEditModeForIcon();
                    LaunchEditMode();
                }
                if (dragOn)
                {
                    EndDrag();
                }
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

    void PlacementManagement()
    {
        if (dragOn)
        {
            Vector2 mousePos = Input.mousePosition; // for now, only the real mouse (later, the mouse can be move by joystick)
            this.transform.position = mousePos + lastOffset;


            if (lastOffset.magnitude > 0)
                lastOffset = Vector2.Lerp(lastOffset, Vector2.zero, Time.deltaTime * 4);


            if (GameManager.instance.mapAndPaper.currentPaper.overing)
                this.transform.localScale = GameManager.instance.mapAndPaper.currentPaper.transform.localScale * baseSize;
            else
                this.transform.localScale = Vector3.one;
        }

        //Update visual for EDIT MODE
        if (editMode)
        {
            if (editionPart.alpha <= 1)
            {
                editionPart.alpha += Time.deltaTime * 2;
            }
        }
        else
        {
            if (editionPart.alpha >= 0)
            {
                editionPart.alpha -= Time.deltaTime * 4;
            }
        }
    }



    void LaunchEditMode()
    {
        Debug.Log("Edit mode start");
        editMode = true;
        editionPart.interactable = true;
        editionPart.blocksRaycasts = true;
    }

    public void QuitEditMode()
    {
        Debug.Log("Edit mode finish");
        editMode = false;
        editionPart.interactable = false;
        editionPart.blocksRaycasts = false;
    }


}
