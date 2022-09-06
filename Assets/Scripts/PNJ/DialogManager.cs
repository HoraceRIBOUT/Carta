using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Deal with the UI
public class DialogManager : MonoBehaviour
{
    public bool inDialog = false; 

    public void Update()
    {
        if (!inDialog)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
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
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.playerMove.FinishTalk();
    }
}
