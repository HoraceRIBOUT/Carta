using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UI_ItemBox : MonoBehaviour
{
    public bool currentlyMiddle = false;

    public CanvasGroup promptList;
    public GameObject promptGive;
    public List<GameObject> prompt_iconController = new List<GameObject>();
    public List<GameObject> prompt_iconKeyboard = new List<GameObject>();

    public Image itemIcon;
    public TMPro.TMP_Text itemName;

    public RectTransform _rect;
    public Vector3 _startPos;
    

    public AnimationCurve positionCurve;
    public AnimationCurve scaleCurve;

    public void SetUpBox(Item item, bool delivered, bool firstCreation)
    {
        promptList.alpha = 0;
        itemName.text = item.nameDisplay;
        itemIcon.sprite = item.icon;

        promptGive.SetActive(!delivered);
        ChangePromptToCorrectDevice();
        if (delivered)
        {
            itemIcon.color = Color.Lerp(Color.black, Color.white, 0.5f);
        }

        if(firstCreation)
            _startPos = _rect.localPosition;
    }

    public bool debug = false;
    public void Placement(float i, float offsetDistance, bool currentMiddle) 
    {
        _rect.localScale = Vector3.one * scaleCurve.Evaluate((1 - Mathf.Abs(i / 4f)));
        _rect.localPosition = _startPos + (positionCurve.Evaluate(i) * offsetDistance) * Vector3.down;

        if(GameManager.instance.inventory.inventoryDeployed)
        {
            if (currentlyMiddle != currentMiddle)
            {
                if (currentMiddle)
                {
                    Deploy();
                }
                else
                {
                    Retract();
                }
                currentlyMiddle = currentMiddle;
            }
        }
        else
        {
            Retract();
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


    public void ChangePromptToCorrectDevice()
    {
        bool controller = InputManager.controller;
        
        foreach (GameObject gO in prompt_iconKeyboard)
            gO.SetActive(!controller);
        foreach (GameObject gO in prompt_iconController)
            gO.SetActive(controller);
    }
}
