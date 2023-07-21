using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UI_ItemBox : MonoBehaviour
{
    public bool currentlyMiddle = false;
    private bool alreadyDelivered = true;

    public CanvasGroup promptList;
    public TMPro.TMP_Text promptGive;
    public TMPro.TMP_Text promptShow;
    public List<GameObject> prompt_iconController = new List<GameObject>();
    public List<GameObject> prompt_iconKeyboard = new List<GameObject>();

    public Image itemIcon;
    public TMPro.TMP_Text itemName;

    public RectTransform _rect;
    public Vector3 _startPos;

    public Animator delivered_anim;

    [Header("On the side (depending on the Y position)")]
    public AnimationCurve positionCurve;
    public AnimationCurve scaleCurve;

    public void SetUpBox(Item item, bool delivered, bool firstCreation)
    {
        promptList.alpha = 0;
        itemName.text = item.nameDisplay;
        itemIcon.sprite = item.icon;

        alreadyDelivered = delivered;
        promptGive.gameObject.SetActive(!delivered && GameManager.instance.dialogMng.inDialog);
        promptShow.gameObject.SetActive(GameManager.instance.dialogMng.inDialog);
        ChangePromptToCorrectDevice();
        ChangePromptToPNJValue(item);

        //Delivered
        delivered_anim.SetBool("Delivered", alreadyDelivered);
        itemIcon.color = Color.Lerp(Color.black, Color.white, delivered ?0.5f : 1f);
        if (!delivered)
        {
            delivered_anim.SetTrigger("Reset");
        }

        if (firstCreation)
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



    [Header("Delivered transition")]
    public AnimationCurve giveCurve;
    public AnimationCurve showCurve;

    public void Delivered()
    {
        alreadyDelivered = true;
        delivered_anim.SetBool("Delivered", true);
        itemIcon.color = Color.Lerp(Color.black, Color.white, 0.5f);
    }

    public void ChangeGivePrompt(Item item)
    {
        promptShow.gameObject.SetActive(GameManager.instance.dialogMng.inDialog);
        ChangePromptToPNJValue(item);
        if(!alreadyDelivered && GameManager.instance.dialogMng.inDialog)
        {
            if (promptGive.text == "")
                promptGive.gameObject.SetActive(false);
            else
                promptGive.gameObject.SetActive(true);
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

    void ChangePromptToPNJValue(Item item)
    {
        string giveString = "Donner";
        string showString = "Montrer";
        pnj pnj = GameManager.instance.dialogMng.currentPNJ;
        if(pnj != null)
        {
            //Use of letter here because I made the algo on paper and use letter to signify each bool and make array.
            //So I keep them for clarity in term of comparaison on my side.
            //sorry if you have to deal with that , it will now cost time for you
            Dialog giveDial = pnj.GetGiveDialogForThisItem(item.id);
            bool B = (giveDial != null? giveDial.HaveBeenLaunchedOnce() : false);
            //For GIVE 
            if (!alreadyDelivered)
            {
                if (B)
                    giveString = "Donner <b><color=FF0000>X</color></b>";
                else
                    giveString = "Donner";
            }
            else
            {
                bool C = pnj.IsFinalTargetForThisItem(item.id);
                if (C)
                {
                    giveString = "Se souvenir <b><color=00FF33>✓</color></b>";
                }
                else if (B)
                {
                    giveString = "Se souvenir <b>✓</b>";
                }
                else
                {
                    bool A = (giveDial != null);
                    if (A)
                        giveString = "Donner ?"; //maybe : say that you have a "show" to do ? so it show the "give" ? only few time we have both...
                    else
                        giveString = "";

                }
            }

            //For SHOW
            Dialog showDial = pnj.GetShowDialogForThisItem(item.id);
            bool Bs = (showDial != null ? showDial.HaveBeenLaunchedOnce() : false);
            if (Bs)
            {
                showString = "Montrer <b>✓</b>";
            }
            else
            {
                if(alreadyDelivered)
                {
                    bool As = (showDial != null);
                    if (As)
                        showString = "Montrer ?";
                }
            }
        }



        //Resolution
        promptGive.SetText(giveString);
        promptShow.SetText(showString);
    }

}
