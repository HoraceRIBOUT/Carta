using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

//This is the manager for the "map" and "paper" part
public class UI_MapAndPaper : MonoBehaviour
{
    [ReadOnly] private List<UI_MaP_Paper> papers = new List<UI_MaP_Paper>();
    //So, by default, have one blank paper
    public bool mapOpen = false;

    private Coroutine openCloseCorout = null;
    [SerializeField] private Animator _anima;

    public void IM_Open()
    {
        Open();
    }
    public void IM_Close()
    {
        Close();
    }

    public void Open()
    {
        if (openCloseCorout != null)
            return;
        if (mapOpen)
            return;

        mapOpen = true;
        openCloseCorout = StartCoroutine(OpenCloseMap());

    }

    public void Close()
    {
        Debug.Log("close ?");
        if (openCloseCorout != null)
            return;
        if (!mapOpen)
            return;

        mapOpen = false;
        openCloseCorout = StartCoroutine(OpenCloseMap());

    }

    private IEnumerator OpenCloseMap()
    {
        Debug.Log("Coroutine");
        _anima.SetBool("Open", mapOpen);
        yield return new WaitForSeconds(0.3f);
        openCloseCorout = null;
    }
}
