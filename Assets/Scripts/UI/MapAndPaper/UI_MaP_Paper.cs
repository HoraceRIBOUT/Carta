using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class UI_MaP_Paper : UI_MaP_IconDropZone
{
    private RectTransform rectTr;

    //Normally, it's only data here. (maybe the zoom ? well, no, even that... more like the bound maybe ?)
    private List<IconPos>    iconsPos       = new List<IconPos>();
    private List<ElementPos> elementsPos    = new List<ElementPos>();

    [Header("Input")]
    public bool beingDragAround = false;
    public Vector3 positionWhenStartDragin;
    public Vector3 mousePositionWhenStartDragin;
    public float mouseDragSpeed = 1;
    public float mouseScrollSpeed = 1;
    public Vector2 mouseScrollMinMax = new Vector2(0.8f, 5f);


    public enum Element
    {
        adelphes,
        adelphe,
        couple,
        parent,
        parentWithSon,
        heart,
        text,
        square,
        circle,
        arrow,
        //everythingelse
    }
    [System.Serializable]
    public class IconPos
    {
        [System.NonSerialized]
        public Vector2 positionRelative;
        [System.NonSerialized]
        public IconData data; //here, a reference to the data, not to the point.
        
        //Data just when save
        public float positionRelative_x, positionRelative_y;
        public float sizeDelta_x, sizeDelta_y;
        public void SetSizeDelta(Vector2 sizeDelta) { sizeDelta_x = sizeDelta.x; sizeDelta_y = sizeDelta.y; }
        public Vector2 GetSizeDelta() { return new Vector2(sizeDelta_x, sizeDelta_y); }
        public IconData.Icon_SaveData saveData;

    }
    [System.Serializable]
    public class ElementPos
    {
        public float positionRelative_x, positionRelative_y;
        public void SetPositionRelative(Vector2 pos)
        {
            positionRelative_x = pos.x;
            positionRelative_y = pos.y;
        }
        public Vector2 GetPositionRelative()
        {
            return new Vector2(positionRelative_x, positionRelative_y);
        }
        public float scaleRelative_x, scaleRelative_y, scaleRelative_z;
        public float sizeDelta_x, sizeDelta_y;
        public void SetSizeDelta(Vector2 sizeDelta) { sizeDelta_x = sizeDelta.x; sizeDelta_y = sizeDelta.y; }
        public Vector2 GetSizeDelta() { return new Vector2(sizeDelta_x, sizeDelta_y); }
        public float rotationRelative_x, rotationRelative_y, rotationRelative_z;
        public Element id;
        public float color_r, color_g, color_b, color_a;
        public void SetColor(Color col) { color_r = col.r; color_g = col.g; color_b = col.b; color_a = col.a; }
        public Color GetColor() { return new Color(color_r, color_g, color_b, color_a); }
        public bool showText;
        public string text;
    }

    //[SerializeField] private float iconMove_Amplitude = 1;
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

    public void AddDrag(UI_MaP_Drag newDrag)
    {
        Debug.Log("Add drag : " + newDrag.name);
        if (newDrag is UI_MaP_Icon)
            AddIcon((UI_MaP_Icon)newDrag);
        else //if(newDrag is UI_MaP_Element)
            AddElement((UI_MaP_Element)newDrag);
    }
    public void RemoveDrag(UI_MaP_Drag newDrag)
    {
        if(newDrag is UI_MaP_Icon)
            RemoveIcon((UI_MaP_Icon)newDrag);
        else //if(newDrag is UI_MaP_Element)
            RemoveElement((UI_MaP_Element)newDrag);
    }

    public void AddIcon(UI_MaP_Icon newIcon)
    {
        IconPos info = new IconPos();
        info.data = newIcon.data;
        info.positionRelative = newIcon.himselfRect.anchoredPosition;
        iconsPos.Add(info);

        iconsGO.Add(newIcon);

        currentSpeedIntensity = 0;
    }
    public void RemoveIcon(UI_MaP_Icon toDelete)
    {
        int index = iconsGO.IndexOf(toDelete);
        iconsPos.RemoveAt(index);
        iconsGO.RemoveAt(index);
    }
    public void AddElement(UI_MaP_Element newElement)
    {
        ElementPos info = new ElementPos();
        info.id = newElement.data;
        //and other thing
        //pos
        //rot
        //scale
        info.SetPositionRelative(newElement.transform.position - this.transform.position);
        elementsPos.Add(info);

        elementsGO.Add(newElement);
    }
    public void RemoveElement(UI_MaP_Element toDelete)
    {
        int index = elementsGO.IndexOf(toDelete);
        elementsPos.RemoveAt(index);
        elementsGO.RemoveAt(index);
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
        if (OveringMe() && !GameManager.instance.mapAndPaper.sideTab.OveringMe() && !AnyDragOvered() && Input.GetMouseButtonDown(0))
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
                IconPos icP = iconsPos[i];
                //ic.transform.position = this.transform.position + (Vector3)icP.positionRelative;
                //ic.transform.position = this.transform.position + (Vector3)icP.positionRelative + iconMove_Speed * iconMove_Amplitude * this.transform.localScale.x;
                // * this.transform.localScale.x + iconMove_Speed * iconMove_Amplitude;
            }
        }

        if (Input.mouseScrollDelta.y != 0 && OveringMe())
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

    public void MoveDependingOnMousePosition(UI_MaP_Drag dragObject)
    {
        if (GameManager.instance.mapAndPaper.sideTab.OveringMe())
        {
            if (currentSpeedIntensity > 0)
                currentSpeedIntensity = Mathf.Max(0, currentSpeedIntensity - Time.deltaTime * 4);
            return;
        }
        //When draggedIcon is on the border of the screen, move the paper.
        
        Vector2 mousePos = Input.mousePosition;
        mousePos.x /= Screen.width;
        mousePos.y /= Screen.height;

        float borderX = GameManager.instance.mapAndPaper.sideTab.LeftBorderPositionInScreenPercentage();

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

        if (dragObject.firstDrag)
        {
            dragObject.firstDrag = false;
            if (mousePos.x > .6 && fullSpeedSpeed.x != 0)
            {
                currentSpeedIntensity = -1;
            }
        }
        else if(currentSpeedIntensity < 0)
        {
            if (mousePos.x < .6 || fullSpeedSpeed.x == 0)
            {
                currentSpeedIntensity = Mathf.Max(0, currentSpeedIntensity);
            }
        }


        //Debug.Log("Full speed : " + fullSpeedSpeed + " (with " + mousePos + ")" + currentSpeedIntensity);

        this.transform.position += Vector3.Lerp(Vector3.zero, fullSpeedSpeed * moveSpeedMax, currentSpeedIntensity);

        this.transform.position = ClampedPosition(this.transform.position);
    }




    [System.Serializable]
    public class Paper_SaveData
    {
        public List<IconPos> iconsData = new List<IconPos>();
        public List<ElementPos> elementsData = new List<ElementPos>();
    }

    public Paper_SaveData GetSaveData()
    {
        Paper_SaveData res = new Paper_SaveData();
        for (int i = 0; i < iconsPos.Count; i++)
        {
            IconPos ic = iconsPos[i];
            ic.saveData = ic.data.GetSerialazableIconData();
            ic.positionRelative_x = ic.positionRelative.x;
            ic.positionRelative_y = ic.positionRelative.y;
            Debug.Log("Icon pos = " + ic.positionRelative);

            ic.SetSizeDelta(iconsGO[i].himselfRect.sizeDelta);
        }
        res.iconsData = new List<IconPos>(iconsPos);

        for (int i = 0; i < elementsPos.Count; i++)
        {
            ElementPos el = elementsPos[i];
            el.text = elementsGO[i].GetText();
            el.showText = elementsGO[i].ShowText;
            el.SetSizeDelta(elementsGO[i].himselfRect.sizeDelta);
            el.SetColor(elementsGO[i].Color);
        }
        res.elementsData = new List<ElementPos>(elementsPos);
        return res;
    }

    public void ApplySaveData(Paper_SaveData savedData)
    {
        //TO DO : 
        Debug.Log("SavedData ?" + savedData.iconsData.Count);

        foreach (var ic_data in savedData.iconsData)
        {
            //Create
            UI_MaP_Icon newIcon = Instantiate(GameManager.instance.mapAndPaper.sideTab.iconPrefab);
            ic_data.data = GameManager.instance.mapAndPaper.sideTab.GetDataForThisPJN(ic_data.saveData.id);
            newIcon.Create(ic_data.data, false);

            //then replace : 
            newIcon.transform.SetParent(iconParent);
            newIcon.transform.localScale = this.transform.localScale * newIcon.baseSize;
            //
            ic_data.positionRelative = new Vector2(ic_data.positionRelative_x, ic_data.positionRelative_y);
            Debug.Log("Ic position when load : "+ ic_data.positionRelative);
            newIcon.himselfRect.anchoredPosition = ic_data.positionRelative;
            newIcon.himselfRect.sizeDelta = ic_data.GetSizeDelta(); //MAYBE : get it in dependencies of the papers size, to avoid problem later with different screen size ?
            //newIcon.himselfRect.sizeDelta = el_data.GetSizeDelta(); //MAYBE : get it in dependencies of the papers size, to avoid problem later with different screen size ?


            //Then add on paper code
            AddIcon(newIcon);
        }

        foreach (var el_data in savedData.elementsData)
        {
            //Create
            UI_MaP_Element newElement = Instantiate(GameManager.instance.mapAndPaper.sideTab.elemPrefab);
            newElement.Create(el_data.id, false, el_data.showText);

            //then replace : 
            newElement.transform.SetParent(elementParent);
            newElement.transform.localScale = this.transform.localScale * newElement.baseSize;
            newElement.himselfRect.sizeDelta = el_data.GetSizeDelta(); //MAYBE : get it in dependencies of the papers size, to avoid problem later with different screen size ?
            newElement.ReplaceOnpaper(el_data);
            newElement.ReplaceTextField();//just in case

            //Then add on paper code
            AddElement(newElement);
        }

    }

}
