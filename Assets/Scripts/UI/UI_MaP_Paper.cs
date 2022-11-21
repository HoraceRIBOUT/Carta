using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class UI_MaP_Paper : MonoBehaviour
{
    //Normally, it's only data here. (maybe the zoom ? well, no, even that... more like the bound maybe ?)
    private List<ItemAndIconPos>    itemAndIcons    = new List<ItemAndIconPos>();
    private List<ElementPos>        elements        = new List<ElementPos>();
    private List<TextPos>           texts           = new List<TextPos>();

    public enum Element
    {
        line,
        crochet,
        upCrochet,
        crochet_part,
        line_part,
        //everythingelse
    }
    class ItemAndIconPos
    {
        public Vector2 positionRelative;
        public pnj data; //here, a reference to the data, not to the point.
    }
    class ElementPos
    {
        public Vector2 positionRelative;
        public Vector3 scaleRelative;
        public Vector3 rotationRelative;
        public Element data;
        public Color color;
    }
    class TextPos
    {
        public Vector2 positionRelative;
        public Vector3 scaleRelative;
        public Vector3 rotationRelative;
        public string textItself;
        public Color color;
    }

}
