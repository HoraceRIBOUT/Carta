using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MaP_Icon : UI_MaP_Drag
{
    [Header("Data")]
    //Data is scriptables
    //but the Icon is just a position and so, can be cloned !
    public IconData data;

    [Header("Element")]
    //Position and info :
    public RectTransform himselfRect;
    public CanvasGroup editionPart;
    public CanvasGroup editNOTPart;

    [Header("For populate part")]
    public Image iconImage;
    [Tooltip("Define by screen percentage")]public Vector2 shadowDistance = new Vector2(0.01f, 0.005f);
    private RectTransform iconRect;
    public TMPro.TMP_InputField name_textField;
    public TMPro.TMP_InputField desc_textField;
    public RectTransform editBGRect;


    [SerializeField] private bool editMode = false;
    //Somehow, relationship

    [Range(0, 1)] public float mouseDistanceToCreateClone = 0.1f;


    public void Create(IconData data, bool onIconZone)
    {
        iconRect = iconImage.transform.parent.GetComponent<RectTransform>();

        this.name = "Icon " + data.id +  (onIconZone?" fromZone":" for page.");
        fromDragZone = onIconZone;
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

        if (fromDragZone)
            LaunchEditMode();
        else
            QuitEditMode();
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
        //if (!fromIconZone)
        {
            GameManager.instance.mapAndPaper.currentPaper.ReUpdateIconFromData();
            GameManager.instance.mapAndPaper.iconZone.ReUpdateIconFromData();
        }
    }
    public void OnDescInputFieldChange()
    {
        data.descText = desc_textField.text;
        //if (!fromIconZone)
        {
            GameManager.instance.mapAndPaper.currentPaper.ReUpdateIconFromData();
            GameManager.instance.mapAndPaper.iconZone.ReUpdateIconFromData();
        }
    }

    

    protected override UI_MaP_Drag CreateClone()
    {
        UI_MaP_Icon ic = Instantiate(this, GameManager.instance.mapAndPaper.iconZone.iconParent);
        ic.Create(data, false);
        ic.transform.position = this.transform.position;
        ic.transform.localScale = Vector3.one;
        ic.firstDrag = true;
        return ic;
    }


    

    public bool showDebug = false;
    public void Update()
    {
        if (showDebug)
            Debug.Log(OveringMe()?"OveringMe " : "NOTvering me.");

        InputManagement();

        PlacementManagement();
    }

    void InputManagement()
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

        if (!fromDragZone && !OveringMe() && editMode)
        {
            QuitEditMode();
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
            
            GameManager.instance.mapAndPaper.currentPaper.MoveDependingOnMousePosition(this);
            //Correct Size
            if (GameManager.instance.mapAndPaper.currentPaper.OveringMe())
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
                editNOTPart.alpha = 1 - editionPart.alpha;
            }
        }
        else
        {
            if (editionPart.alpha >= 0)
            {
                editionPart.alpha -= Time.deltaTime * 4;
                editNOTPart.alpha = 1 - editionPart.alpha;
            }
        }

        if (dragOn)
        {
            Vector2 targetSize = new Vector2(Screen.width * shadowDistance.x, Screen.height * shadowDistance.y);
            iconImage.transform.localPosition = Vector2.Lerp(iconImage.transform.localPosition, targetSize, Time.deltaTime * 4);
        }
        else
        {
            Vector2 targetSize = Vector2.zero;
            iconImage.transform.localPosition = Vector2.Lerp(iconImage.transform.localPosition, targetSize, Time.deltaTime * 4);
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


    public bool OveringMe()
    {
        Vector2 mousePos = Input.mousePosition;
        float zoom = this.transform.localScale.x;
        if (!fromDragZone)
        {
            zoom *= GameManager.instance.mapAndPaper.currentPaper.transform.localScale.x;
        }
        float minX, maxX, minY, maxY;
        if (editMode)
        {
            //The whole thing
            minX = editBGRect.transform.position.x - editBGRect.rect.width  * zoom / 2;
            maxX = editBGRect.transform.position.x + editBGRect.rect.width  * zoom / 2;
            minY = editBGRect.transform.position.y - editBGRect.rect.height * zoom / 2;
            maxY = editBGRect.transform.position.y + editBGRect.rect.height * zoom / 2;
        }
        else
        {
            //Only icon
            minX = iconRect.transform.position.x - iconRect.rect.width  * zoom / 2;
            maxX = iconRect.transform.position.x + iconRect.rect.width  * zoom / 2;
            minY = iconRect.transform.position.y - iconRect.rect.height * zoom / 2;
            maxY = iconRect.transform.position.y + iconRect.rect.height * zoom / 2;

            //Debug.Log(mousePos + " vs : " + rectTr.rect.x + " , " + rectTr.rect.y + " and " + minY + " ---> " + maxY);
        }


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
