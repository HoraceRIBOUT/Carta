using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_MaP_IconDropZone : UI_MaP_Overing
{
    public Transform iconParent;
    protected List<UI_MaP_Icon> iconsGO = new List<UI_MaP_Icon>();


    public void ChangeRaycastBlockForIcon(bool value)    {        foreach (var icon in iconsGO)            icon.himself.blocksRaycasts = value; }
    public void ChangeOveringForIcon     (bool value)    {        foreach (var icon in iconsGO)            icon.overing = value;                }
    public void ReUpdateIconFromData()                   {        foreach (var icon in iconsGO)            icon.ReUpdateFromData();             }
    public void QuitEditModeForIcon()                    {        foreach (var icon in iconsGO)            icon.QuitEditMode();                 }
    
    public bool AnyIconOvered()                         
    {
        foreach (var icon in iconsGO)
        {
            if (icon.OveringMe()) 
                return true;
        }
        
        return false;
    }

}
