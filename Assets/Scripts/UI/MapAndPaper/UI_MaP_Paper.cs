using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class UI_MaP_Paper : UI_MaP_IconDropZone
{
    public Transform elementParent;
    private RectTransform rectTr;

    //Normally, it's only data here. (maybe the zoom ? well, no, even that... more like the bound maybe ?)
    private List<ItemAndIconPos>    itemAndIcons    = new List<ItemAndIconPos>();
    private List<ElementPos>        elements        = new List<ElementPos>();
    private List<TextPos>           texts           = new List<TextPos>();

    //And here, it have a reference to the GAMEOBJECT created to represent the data. Seems simple enough.
    private List<UI_MaP_Element>    elementsGO      = new List<UI_MaP_Element>();
    private List<TMPro.TMP_Text>    textsGO         = new List<TMPro.TMP_Text>();

    [Header("Input")]
    public bool beingDragAround = false;
    public Vector3 positionWhenStartDragin;
    public Vector3 mousePositionWhenStartDragin;
    public float mouseDragSpeed = 1;
    public float mouseScrollSpeed = 1;
    public Vector2 mouseScrollMinMax = new Vector2(0.8f, 5f);


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
        public IconData data; //here, a reference to the data, not to the point.
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

    [SerializeField] private float iconMove_Amplitude = 1;
    private Vector3 iconMove_Speed = Vector3.zero;
    private Vector3 lastMovement;

    public void Start()
    {
        rectTr = this.GetComponent<RectTransform>();
        positionWhenStartDragin = this.transform.position;
        centerPos = this.transform.position;
    }

    public void ResetPosAndScale()
    {
        rectTr.localScale = Vector3.one;
        rectTr.localPosition = Vector3.zero;
    }

    public void AddIcon(UI_MaP_Icon newIcon)
    {
        ItemAndIconPos info = new ItemAndIconPos();
        info.data = newIcon.data;
        info.positionRelative = newIcon.transform.position - this.transform.position;
        itemAndIcons.Add(info);

        iconsGO.Add(newIcon);

        currentSpeedIntensity = 0;
    }
    public void RemoveIcon(UI_MaP_Icon newIcon)
    {
        iconsGO.Remove(newIcon);
    }
    public void AddElement(UI_MaP_Element newElement)
    {
        ElementPos info = new ElementPos();
        info.data = newElement.data;
        //and other thing
        //pos
        //rot
        //scale
        info.positionRelative = newElement.transform.position - this.transform.position;//need to have a position correct so ?
        Debug.Log("");
        elements.Add(info);

        elementsGO.Add(newElement);
    }

    //remove the animation so we can move it on LateUpdate
    public void LateUpdate()
    {
        if (!GameManager.instance.mapAndPaper.mapOpen)
            return;
        //For now here :
        MovePaper();
    }

    private void MovePaper()
    {
        //Overing me AND no icon (or element) are overed AND list icon zone is not overred too
        if (OveringMe() && !GameManager.instance.mapAndPaper.iconZone.OveringMe() && !AnyIconOvered() && Input.GetMouseButtonDown(0))
        {
            beingDragAround = true;
            //Start drag the paper
            positionWhenStartDragin = this.transform.position;
            mousePositionWhenStartDragin = Input.mousePosition;
        }
        else if(beingDragAround && Input.GetMouseButtonUp(0))
        {
            beingDragAround = false;
            positionWhenStartDragin = this.transform.position;
        }


        if (beingDragAround)
        {
            //Debug.Log("mousePositionWhenStartDragin = " + mousePositionWhenStartDragin);
            Vector3 desirePosition = positionWhenStartDragin + (Input.mousePosition - mousePositionWhenStartDragin) * mouseDragSpeed;
            desirePosition = ClampedPosition(desirePosition);
            this.transform.position = desirePosition;

            lastMovement = this.transform.position - desirePosition;
        }
        else
        {
            lastMovement = Vector3.zero;
        }

        if(iconMove_Speed != Vector3.zero)
        {
            //Mean it's frame dependant
            iconMove_Speed = Vector3.Lerp(iconMove_Speed, lastMovement, Time.deltaTime);
            //if (iconMove_Speed.magnitude < 0.001f)
            //    iconMove_Speed = Vector3.zero;
            for (int i = 0; i < iconsGO.Count; i++)
            {
                UI_MaP_Icon ic = iconsGO[i];
                ItemAndIconPos icP = itemAndIcons[i];
                //ic.transform.position = this.transform.position + (Vector3)icP.positionRelative;
                ic.transform.position = this.transform.position + (Vector3)icP.positionRelative + iconMove_Speed * iconMove_Amplitude * this.transform.localScale.x;
                // * this.transform.localScale.x + iconMove_Speed * iconMove_Amplitude;
            }
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            this.transform.localScale += Input.mouseScrollDelta.y * mouseScrollSpeed * Time.deltaTime * Vector3.one;

            if(this.transform.localScale.x < mouseScrollMinMax.x)
            {
                this.transform.localScale = Vector3.one * mouseScrollMinMax.x;
            }
            else if (this.transform.localScale.x > mouseScrollMinMax.y)
            {
                this.transform.localScale = Vector3.one * mouseScrollMinMax.y;
            }
            this.transform.position = ClampedPosition(this.transform.position);
        }
    }

    public Vector2 minMaxPos = new Vector2(200, 100);
    private Vector2 centerPos;
    private Vector4 relativeMinMaxPos;
    public Vector3 ClampedPosition(Vector3 position)
    {
        float zoomFactor = (this.transform.localScale.x * this.transform.localScale.x);
        relativeMinMaxPos = new Vector4(centerPos.x + minMaxPos.x * zoomFactor,
                                        centerPos.y + minMaxPos.y * zoomFactor,
                                        centerPos.x - minMaxPos.x * zoomFactor,
                                        centerPos.y - minMaxPos.y * zoomFactor
                                        );


        if (position.x > relativeMinMaxPos.x)
            position.x = relativeMinMaxPos.x;
        if (position.x < relativeMinMaxPos.z)
            position.x = relativeMinMaxPos.z;

        if (position.y > relativeMinMaxPos.y)
            position.y = relativeMinMaxPos.y;
        if (position.y < relativeMinMaxPos.w)
            position.y = relativeMinMaxPos.w;

        return position;
    }



    public bool OveringMe()
    {
        Vector2 mousePos = Input.mousePosition;

        //Now, add the zoom !
        float zoom = this.transform.localScale.x;

        float minX = this.transform.position.x - rectTr.rect.width  * zoom / 2;
        float maxX = this.transform.position.x + rectTr.rect.width  * zoom / 2;
        float minY = this.transform.position.y - rectTr.rect.height * zoom / 2;
        float maxY = this.transform.position.y + rectTr.rect.height * zoom / 2;

        //Debug.Log(mousePos + " vs : " + rectTr.rect.x + " , " + rectTr.rect.y + " and " + minY + " ---> " + maxY);

        if (minX < mousePos.x &&
            maxX > mousePos.x &&
            minY < mousePos.y &&
            maxY > mousePos.y )
        {
            return true;
        }

        return false;
    }


    [Header("Icon move paper by border")]
    public AnimationCurve paperBorderMoveX;
    public AnimationCurve paperBorderMoveY;
    public float moveSpeedMax = 10f;
    [Range(0,1)] public float currentSpeedIntensity = 0;

    public void MoveDependingOnMousePosition()
    {
        if (GameManager.instance.mapAndPaper.iconZone.OveringMe())
        {
            if (currentSpeedIntensity > 0)
                currentSpeedIntensity = Mathf.Max(0, currentSpeedIntensity - Time.deltaTime * 4);
            return;
        }
        //When draggedIcon is on the border of the screen, move the paper.
        
        Vector2 mousePos = Input.mousePosition;
        mousePos.x /= Screen.width;
        mousePos.y /= Screen.height;

        float borderX = GameManager.instance.mapAndPaper.iconZone.LeftBorderPositionInScreenPercentage();

        Vector3 fullSpeedSpeed = Vector2.zero;


        fullSpeedSpeed.y = paperBorderMoveY.Evaluate(mousePos.y);
        mousePos.x /= borderX;
        if (mousePos.x > 1)
            fullSpeedSpeed.x = 0;
        else
            fullSpeedSpeed.x = paperBorderMoveX.Evaluate(mousePos.x);

        if (fullSpeedSpeed != Vector3.zero)
        {
            if(currentSpeedIntensity < 1)
                currentSpeedIntensity = Mathf.Min(1, currentSpeedIntensity + Time.deltaTime * 0.5f);
        }
        else
        {
            if (currentSpeedIntensity > 0)
                currentSpeedIntensity = Mathf.Max(0, currentSpeedIntensity - Time.deltaTime * 4);
        }

        Debug.Log("Full speed : " + fullSpeedSpeed + " (with " + mousePos + ")" + currentSpeedIntensity);

        this.transform.position += Vector3.Lerp(Vector3.zero, fullSpeedSpeed * moveSpeedMax, currentSpeedIntensity);

        this.transform.position = ClampedPosition(this.transform.position);
    }

}
