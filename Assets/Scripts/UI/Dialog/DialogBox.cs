using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogBox : MonoBehaviour
{
    //Global : 
    //private Coroutine fadeDial = null; //useless now
    private Coroutine printDial = null;
    private string printText_inSkipCase = "";
    [SerializeField] private float printDelay = 0.05f;
    [SerializeField] private int nbrIndxGrad = 5;
    //

    [Header("Local only : ")]
    [SerializeField] private TMPro.TMP_Text dialogueTextBox;
    [SerializeField] private Animator animator;
    [SerializeField] [Sirenix.OdinInspector.ReadOnly()] private int step = 0;
    [Header("Title card : ")]
    [SerializeField] private CanvasGroup titleCardAlpha;
    [SerializeField] private TMPro.TMP_Text titleCardTextBox;

    
    public void Open(string text, Color col, string title)
    {
        //Debug.Log("title = " + title);
        dialogueTextBox.color = col;
        printDial = StartCoroutine(PrintDialogText(text));
        printText_inSkipCase = text;
        animator.SetBool("Open", true);
        this.transform.SetSiblingIndex(GameManager.instance.dialogMng.dialogTexts.Count - 1);
        step = 1; // Open

        UpdateTitle(title);
    }

    public void Next()
    {
        if (Available())
            return;
        //Do "Next" only if not ready

        animator.SetTrigger("Next");
        //this.transform.SetSiblingIndex(2-step);
        step++; 

        if(step > 2)//Step 0 : available / Step 1 : openMiddle / Step 2 : goesOnBack / Step 3 : quit out and loop back
        {
            Close();
        }

    }

    public void Close()
    {
        if (printDial != null)
            StopCoroutine(printDial);
        animator.ResetTrigger("Next");
        animator.SetBool("Open", false);
        step = 0;
    }


    public bool Available()
    {
        return step == 0;
    }
    
    public bool Printing()
    {
        return (printDial != null);
    }



    public void SkipPrinting()
    {
        //Debug.Log("Skip apparition time");
        StopCoroutine(printDial);
        FinishPrint();
        dialogueTextBox.text = printText_inSkipCase;
        //Just display it totally, in one try
    }
    

    private IEnumerator PrintDialogText(string originalText)
    {
        //Debug.Log("Print " + dialogueTextBox.name + " start.");
        int charIndex = 0;
        float charProgress = 0;

        //do it better.
        float minDelay = 1f / 60f;
        while (charProgress - nbrIndxGrad < originalText.Length)
        {
            charIndex = Mathf.FloorToInt(charProgress);
            //if(charIndex < originalText.Length && originalText[charIndex] == ' ')
            //{
            //    charProgress++;
            //    continue;
            //}
            //Set text : 
            //Add a gradient
            {
                string part1 = originalText.Substring(0, Mathf.Max(0, charIndex - nbrIndxGrad));//a fiorcori , toujours à fond
                string[] partList = new string[nbrIndxGrad];
                for (int i = 0; i < nbrIndxGrad; i++)
                {
                    int inv = nbrIndxGrad - i;
                    int lenght = 1;
                    if (charIndex - inv < 0)
                        lenght = 0;
                    else if (charIndex - inv >= originalText.Length)
                        lenght = 0;
                    partList[i] = originalText.Substring(Mathf.Clamp(charIndex - inv, 0, originalText.Length), lenght);

                }

                string partFinal = originalText.Substring(Mathf.Clamp(charIndex - 0, 0, originalText.Length), Mathf.Max(0, originalText.Length - charIndex));

                Color startCol = dialogueTextBox.color;
                Color endCol = dialogueTextBox.color - Color.black;//to have a transparent version
                string[] colorList = new string[nbrIndxGrad];
                for (int i = 0; i < nbrIndxGrad; i++)
                {
                    float lerpColValue = ((i+1) * (1f / nbrIndxGrad))  -  ((charProgress - charIndex)) * (1f / nbrIndxGrad);
                    lerpColValue = Mathf.Clamp(lerpColValue, 0, 1);
                    colorList[i] = ColorUtility.ToHtmlStringRGBA(Color.Lerp(startCol, endCol, lerpColValue));
                }
                string colorGradientFinal = ColorUtility.ToHtmlStringRGBA(endCol);

                System.Text.StringBuilder build = new System.Text.StringBuilder(part1);
                for (int i = 0; i < nbrIndxGrad; i++)
                {
                    build.Append("<color=#" + colorList[i] + ">" + partList[i] + "</color>");
                }
                build.Append("<color=#" + colorGradientFinal + ">" + partFinal + "</color>");
                dialogueTextBox.text = build.ToString();
            }

            yield return 0;
            charProgress += Time.deltaTime / printDelay;
        }
        Debug.Log("Print " + dialogueTextBox.name + " finish.");
        dialogueTextBox.text = originalText;
        FinishPrint();
    }

    void FinishPrint()
    {
        GameManager.instance.dialogMng.FinishTalk();

        printDial = null;
    }


    public string GetCurrentTitle()
    {
        return titleCardTextBox.text.Trim();
    }
    public Color GetCurrentColor()
    {
        return dialogueTextBox.color;
    }

    public void UpdateTitle(string newName)
    {
        titleCardAlpha.gameObject.SetActive(newName.Trim() != "");
        titleCardTextBox.SetText(newName);
    }

}
