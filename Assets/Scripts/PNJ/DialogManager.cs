using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Deal with the UI
public class DialogManager : MonoBehaviour
{
    public bool inDialog = false;

    public List<pnj> allPNJ = new List<pnj>();

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


    public void StartDialog()
    {
        GameManager.instance.playerMove.Talk();
        inDialog = true;
    }

    public void FinishDialog()
    {
        StartCoroutine(CloseDialog());
    }

    public IEnumerator CloseDialog()
    {
        inDialog = false;
        GameManager.instance.cameraMng.UnSetSecondaryTarget();
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.playerMove.FinishTalk();
    }
}
