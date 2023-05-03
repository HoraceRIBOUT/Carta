using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// For now, only horizontal
/// </summary>
public class UI_EvenSpacing : MonoBehaviour
{
    [Range(0,0.15f)] [OnValueChanged("OnSizeChanged")] public float spacingSize = 0.05f;
    [Range(0,0.15f)] [OnValueChanged("OnSizeChanged")] public float buttonSize  = 0.1f;
    [Range(0,0.15f)] [OnValueChanged("OnSizeChanged")] public float margingSize = 0.1f;
    public bool autoNormalize = false;
    public bool autoCompute = false;

    public List<RectTransform> elements = new List<RectTransform>();
    public List<RectTransform> elementsToOnlyResize = new List<RectTransform>();
    // Start is called before the first frame update
    void Start()
    {
        Normalize();
        Compute();
    }

    void OnSizeChanged()
    {
        if (autoNormalize)
            Normalize();
        if (autoCompute)
            Compute();
    }

    void Normalize()
    {
        float sum = spacingSize * (elements.Count - 1) + buttonSize * (elements.Count) + margingSize * 2;

        spacingSize = Mathf.InverseLerp(0, sum, spacingSize * 5f) / 5f;
        buttonSize  = Mathf.InverseLerp(0, sum, buttonSize  * 6f) / 6f;
        margingSize = Mathf.InverseLerp(0, sum, margingSize * 2f) / 2f;
    }

    [Button]
    void Compute()
    {
        float lastPos = 0;
        for (int i = 0; i < elements.Count; i++)
        {
            Vector2 anchorMin = elements[i].anchorMin;
            Vector2 anchorMax = elements[i].anchorMax;

            anchorMin.x = (i==0?margingSize:lastPos + spacingSize);
            anchorMax.x = anchorMin.x + buttonSize;
            lastPos = anchorMax.x;
            
            elements[i].anchorMin = anchorMin;
            elements[i].anchorMax = anchorMax;
        }

        for (int i = 0; i < elementsToOnlyResize.Count; i++)
        {
            Vector2 anchorMin = elementsToOnlyResize[i].anchorMin;
            Vector2 anchorMax = elementsToOnlyResize[i].anchorMax;

            anchorMin.x = 0;
            anchorMax.x = anchorMin.x + buttonSize;

            elementsToOnlyResize[i].anchorMin = anchorMin;
            elementsToOnlyResize[i].anchorMax = anchorMax;
        }
    }
}
