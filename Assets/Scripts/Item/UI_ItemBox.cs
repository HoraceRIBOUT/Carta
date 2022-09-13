using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UI_ItemBox : MonoBehaviour
{
    public CanvasGroup promptList;
    public GameObject promptGive;

    public Image itemIcon;
    public TMPro.TMP_Text itemName;

    public void SetUpBox(Item item, bool delivered)
    {
        itemName.text = item.nameDisplay;
        itemIcon.sprite = item.icon;

        promptGive.SetActive(!delivered);
        if (delivered)
        {
            itemIcon.color = Color.Lerp(Color.black, Color.white, 0.5f);
        }
    }


    [Header("Transition deploy")]
    [Range(0,1)]
    public float deployLerp = 0;
    public float trnasitionSpeed = 1.5f;
    public AnimationCurve trnasitionCurve;
    Coroutine deployingRoutine = null;

    public void Deploy()
    {
        if (deployingRoutine != null)
            StopCoroutine(deployingRoutine);
        deployingRoutine = StartCoroutine(Deploy_Corout(true));
    }
    public void Retract()
    {
        if (deployingRoutine != null)
            StopCoroutine(deployingRoutine);
        deployingRoutine = StartCoroutine(Deploy_Corout(false));
    }

    public IEnumerator Deploy_Corout(bool deploy)
    {
        while ((deploy ? deployLerp < 1 : deployLerp > 0))
        {
            deployLerp += Time.deltaTime * trnasitionSpeed * (deploy ? 1 : -1);
            deployLerp = Mathf.Clamp01(deployLerp);
            promptList.alpha = trnasitionCurve.Evaluate(deployLerp);
            yield return new WaitForSeconds(1f / 60f);
        }
    }
}
