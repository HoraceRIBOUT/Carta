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
    public TMPro.TMP_InputField name_textField;
    public TMPro.TMP_InputField desc_textField;
    public RectTransform editBGRect;

    [SerializeField] private bool editMode = false;
    //Somehow, relationship


    public void Create(IconData data, bool onIconZone)
    {
        visualRect = iconImage.transform.parent.GetComponent<RectTransform>();

        this.name = "Icon " + data.id +  (onIconZone?" fromZone":" for page.");
        fromDragZone = onIconZone;
        this.data = data;

        //Icon text
        iconImage.sprite = data.spriteLittleIcon;
        //Icon image
        Debug.Log("Data : " + data.nameText + " and " + data.descText);
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
        Debug.Log("Edit Name !?" + (!name_textField.isFocused ? "return":"continue")  + "(oh, and also :"+(GameManager.instance.mapAndPaper.currentEditText != name_textField?"nope, not this one.": " it's this name who get modify") +" )" + name_textField.text, name_textField.gameObject);

        if (!name_textField.isFocused)
        {
            if(name_textField.text != data.nameText)
                name_textField.SetTextWithoutNotify(data.nameText);
            return; //get ignore, this is probably a callback because of a "SetTextWithoutNotify" who still notify
        }
        GameManager.instance.mapAndPaper.currentEditText = name_textField;
        Debug.Log("Become name :: "+name_textField, this.gameObject);

        data.nameText = name_textField.text;
        //if (!fromIconZone)
        {
            GameManager.instance.mapAndPaper.currentPaper.ReUpdateIconFromData();
            GameManager.instance.mapAndPaper.sideTab.ReUpdateIconFromData();
        }
    }
    public void OnDescInputFieldChange()
    {
        Debug.Log("desc_textField : "+ desc_textField.isFocused + " yep.");
        if (!desc_textField.isFocused)
        {
            if (desc_textField.text != data.descText)
                desc_textField.SetTextWithoutNotify(data.descText);
            return; //get ignore, this is probably a callback because of a "SetTextWithoutNotify" who still notify
        }
        GameManager.instance.mapAndPaper.currentEditText = desc_textField;
        Debug.Log("Become desc :: "+ desc_textField, this.gameObject);

        data.descText = desc_textField.text;
        //if (!fromIconZone)
        {
            GameManager.instance.mapAndPaper.currentPaper.ReUpdateIconFromData();
            GameManager.instance.mapAndPaper.sideTab.ReUpdateIconFromData();
        }
    }

    public void OnSelectCustomField_Name()
    {
        //Debug.Log("Select input field (name)");
        GameManager.instance.mapAndPaper.currentEditText = name_textField;
        Debug.Log("Become name : "+ name_textField, this.gameObject);
    }
    public void OnSelectCustomField_Desc()
    {
        //Debug.Log("Select input field (desc)");
        GameManager.instance.mapAndPaper.currentEditText = desc_textField;
        Debug.Log("Become desc : " + desc_textField, this.gameObject);
    }
    public void OnDeselectCustomField()
    {
        //Debug.Log("Unselect input field.");
        GameManager.instance.mapAndPaper.StopEditingText();
    }



    protected override UI_MaP_Drag CreateClone()
    {
        UI_MaP_Icon ic = Instantiate(this, GameManager.instance.mapAndPaper.sideTab.iconParent);
        ic.Create(data, false);
        ic.transform.position = this.transform.position;
        ic.transform.localScale = Vector3.one;
        ic.firstDrag = true;
        return ic;
    }


    protected override void FinishClickOnIt_WhileOnPaper()
    {
        if (lastMouseClickPosition != Vector3.zero)
        {
            GameManager.instance.mapAndPaper.currentPaper.QuitEditModeForIcon();
            LaunchEditMode();
        }
        base.FinishClickOnIt_WhileOnPaper();

    }


    protected override void InputManagement()
    {
        base.InputManagement();

        if (!fromDragZone && !OveringMe() && editMode)
        {
            QuitEditMode();
            ForceQuitTextEdition();
        }

    }

    protected override void PlacementManagement()
    {
        base.PlacementManagement();

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

    public void TryQuitTextEdition()
    {
        if (GameManager.instance.mapAndPaper.IsEditingText())
        {
            if (GameManager.instance.mapAndPaper.currentEditText == desc_textField || GameManager.instance.mapAndPaper.currentEditText == name_textField)
            {
                GameManager.instance.mapAndPaper.currentEditText.DeactivateInputField();
                GameManager.instance.mapAndPaper.currentEditText = null;
            }
        }
    }
    public void ForceQuitTextEdition()
    {
        Debug.Log("Force quit the text edition");
        GameManager.instance.mapAndPaper.StopEditingText();
    }


    public override bool OveringMe()
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
            minX = visualRect.transform.position.x - visualRect.rect.width  * zoom / 2;
            maxX = visualRect.transform.position.x + visualRect.rect.width  * zoom / 2;
            minY = visualRect.transform.position.y - visualRect.rect.height * zoom / 2;
            maxY = visualRect.transform.position.y + visualRect.rect.height * zoom / 2;

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
