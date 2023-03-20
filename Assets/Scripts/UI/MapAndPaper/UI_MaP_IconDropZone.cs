using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_MaP_IconDropZone : UI_MaP_Overing
{
    public Transform iconParent;
    protected List<UI_MaP_Icon> iconsGO = new List<UI_MaP_Icon>();
    protected List<UI_MaP_Element> elementsGO = new List<UI_MaP_Element>();




    public void ChangeRaycastBlockForDrag(bool value)
    {
        foreach (var icon in iconsGO)
            icon.himself.blocksRaycasts = value;
        foreach (var elem in elementsGO)
            elem.himself.blocksRaycasts = value;
    }
    public void ChangeOveringForDrag(bool value)
    {
        foreach (var icon in iconsGO)
            icon.overing = value;
        foreach (var elem in elementsGO)
            elem.overing = value;
    }
    
    public void ReUpdateIconFromData()                   {        foreach (var icon in iconsGO)            icon.ReUpdateFromData();             }
    public void QuitEditModeForIcon()                    {        foreach (var icon in iconsGO)            icon.QuitEditMode();                 }
    
    public bool AnyDragOvered()                         
    {
        foreach (var icon in iconsGO)
        {
            if (icon.OveringMe()) 
                return true;
        }

        foreach (var elem in elementsGO)
        {
            if (elem.OveringMe())
                return true;
        }

        return false;
    }

}
