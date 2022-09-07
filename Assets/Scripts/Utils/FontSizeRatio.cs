using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FontSizeRatio : MonoBehaviour
{
    [Header("☟ ")]
    [Tooltip("If not, is ratio from the whole screen")]
    public bool ratioFromTheRect;
    [Range(0,1)]
    //[Sirenix.OdinInspector.OnValueChanged("Resize")]
    public float ratio = 0.33f;

    private Vector2 screenSize;

    public RectTransform myRectTransform;
    public Text myText;
    public TMP_Text myTMP_Text;


    // Start is called before the first frame update
    void Start()
    {
        if(myRectTransform == null)
        {
            myRectTransform = this.GetComponent<RectTransform>();
        }

        if (myText == null && myTMP_Text == null)
        {
            myText = this.GetComponent<Text>();
            myTMP_Text = this.GetComponent<TMP_Text>();

        }

        Resize();
        screenSize = new Vector2(Screen.width, Screen.height);
    }

    public void OnEnable()
    {
        Resize();
    }

    private void LateUpdate()
    {
        if(screenSize.x != Screen.width || screenSize.y != Screen.height)
        {
            Resize();
            screenSize = new Vector2(Screen.width, Screen.height);
        }
    }

    //[Sirenix.OdinInspector.Button("Apply size")]
    public void Resize()
    {
        float sizeMultiplier = 1;
        /*if (GameManager.instance != null && GameManager.instance.optionData != null)
            sizeMultiplier = GameManager.instance.optionData.dialogueSizeMultiplier;
        else if (FindObjectOfType<Opening>() != null)
            sizeMultiplier = FindObjectOfType<Opening>().option.dialogueSizeMultiplier;*/

        if (ratioFromTheRect)
        {
            if (myText != null)
            {
                myText.fontSize = (int)(ratio * myRectTransform.rect.size.y * sizeMultiplier);
            }
            if (myTMP_Text != null)
            {
                myTMP_Text.fontSize = ratio * myRectTransform.rect.size.y * sizeMultiplier;
            }
        }
        else
        {
            if (myText != null)
            {
                myText.fontSize = (int)(ratio * Screen.height * sizeMultiplier);
            }
            if (myTMP_Text != null)
            {
                myTMP_Text.fontSize = ratio * Screen.height * sizeMultiplier;
            }
        }
    }
}
