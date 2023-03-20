using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UI_MaP_Overing : MonoBehaviour
{
    /// <summary>
    /// [OBSOLETE] use OveringMe() when you can
    /// </summary>icon
    public bool overing = false; //this value is now obsolet : use OveringMe()

    public void MouseEnterZone()
    {
        overing = true; 
        //Debug.Log("MouseEnterZone " + this.gameObject.name, this.gameObject);
    }
    public void MouseExitZone()
    {
        overing = false; 
        //Debug.Log("MouseExitZone" + this.gameObject.name, this.gameObject);
    }



}
