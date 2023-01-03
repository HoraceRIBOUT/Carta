using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UI_MaP_Overing : MonoBehaviour
{
    public bool overing = false;

    public void MouseEnterZone()
    {
        overing = true;
    }
    public void MouseExitZone()
    {
        overing = false;
    }
}
