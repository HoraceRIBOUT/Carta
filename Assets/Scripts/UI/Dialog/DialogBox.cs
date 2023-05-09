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
        Debug.Log("title = " + title);
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
        Debug.Log("Skip apparition time");
        StopCoroutine(printDial);
        FinishPrint();
        dialogueTextBox.text = printText_inSkipCase;
        //Just display it totally, in one try
    }


    private IEnumerator PrintDialogText(string originalText)
    {
        Debug.Log("Print " + dialogueTextBox.name + " start.");
        int charIndex = 0;

        float minDelay = 1f / 60f;
        while (charIndex < originalText.Length)
        {
            if (originalText[charIndex] == ' ')
            {
                charIndex++;
                continue;
            }
            //Set text : 
            //Add a gradient
            {
                string part1 = originalText.Substring(0, Mathf.Max(0, charIndex - nbrIndxGrad));
                string[] partList = new string[nbrIndxGrad];
                for (int i = 0; i < nbrIndxGrad; i++)
                {
                    int inv = nbrIndxGrad - i;
                    partList[i] = originalText.Substring(Mathf.Max(0, charIndex - inv), charIndex - inv < 0 ? 0 : 1);
                }
                string partFinal = originalText.Substring(Mathf.Max(0, charIndex - 0), originalText.Length - charIndex);

                Color startCol = dialogueTextBox.color;
                Color endCol = dialogueTextBox.color - Color.black;
                string[] colorList = new string[nbrIndxGrad];
                for (int i = 0; i < nbrIndxGrad; i++)
                {
                    int inv = nbrIndxGrad - i;
                    colorList[i] = ColorUtility.ToHtmlStringRGBA(Color.Lerp(startCol, endCol, i * (1f / (nbrIndxGrad + 1))));
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

            yield return new WaitForSeconds(Mathf.Max(minDelay, printDelay));
            //Jump more char if delay is < than Mindelay
            if (printDelay < minDelay)
            {
                charIndex += Mathf.RoundToInt(printDelay / minDelay);
            }
            else
            {
                charIndex++;
            }
        }
        Debug.Log("Print " + dialogueTextBox.name + " finish.");
        dialogueTextBox.text = originalText;
        FinishPrint();
    }

    void FinishPrint()
    {
        if(GameManager.instance.dialogMng.currentPNJ != null)
            GameManager.instance.dialogMng.currentPNJ.LineEnd();
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
