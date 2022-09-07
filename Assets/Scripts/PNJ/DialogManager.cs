using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Deal with the UI
public class DialogManager : MonoBehaviour
{
    public bool inDialog = false;

    public List<pnj> allPNJ = new List<pnj>();

    [Header("UI")]
    public TMPro.TMP_Text dialogText;
    public Animator dialogAnimator;

    public void Start()
    {
        if(allPNJ == null || allPNJ.Count == 0)
        {
            FillAllPNJ();
        }
    }


    public void FillAllPNJ()
    {
        foreach (pnj interac in FindObjectsOfType<pnj>())
        {
            allPNJ.Add(interac);
        }
    }

    public void Update()
    {
        if (!inDialog)
        {
            foreach (pnj interactivePNJ in allPNJ)
            {
                if (interactivePNJ.ReturnUpdate())
                    break;
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            //KeepGoing();
            FinishDialog();
        }
    }


    public void StartDialog(string text)
    {
        //TO DO : add two text. When one sentence is finish, fade away the previous sentence. Then, open the second one, letter by letter
        dialogText.text = text;
        GameManager.instance.playerMove.Talk();
        dialogAnimator.SetBool("Open", true);
        inDialog = true;

        //en vrai, need a delay before you can act/type, so you don't pass the text by accident ! 
        //and when it show the full text, can't be pass before X seconds
    }

    public void FinishDialog()
    {
        StartCoroutine(CloseDialog());
    }

    public IEnumerator CloseDialog()
    {
        inDialog = false;
        dialogAnimator.SetBool("Open", false);
        GameManager.instance.cameraMng.UnSetSecondaryTarget();
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.playerMove.FinishTalk();
    }
}
