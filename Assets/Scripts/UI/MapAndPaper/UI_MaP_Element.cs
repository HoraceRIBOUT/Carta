using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_MaP_Element : UI_MaP_Drag
{
    //Data : 
    //what element it is (just the sprite)
    //what transform rect it is (pos, scale and scale + size in pixel)
    public UI_MaP_Paper.Element data;

    protected override UI_MaP_Drag CreateClone()
    {
        return new UI_MaP_Element();
    }
}
