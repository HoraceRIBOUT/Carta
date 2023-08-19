using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_ItemNote : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public TMP_Text txt_title;
    public TMP_Text txt_desc;
    public TMP_InputField txt_desc_custom;
    public TMP_Text txt_difficulty;
    public TMP_Text txt_knowledge;

    private Item currentItem;

    public void SetNote(Item item)
    {
        txt_title.SetText(item.nameDisplay);
        txt_desc.SetText(item.description_fixed);
        txt_desc_custom.DeactivateInputField();
        txt_desc_custom.SetTextWithoutNotify(item.description_custom);
        txt_difficulty.SetText("<b>Difficulty :</b>\n" + item.difficulty + "/10");
        txt_knowledge.SetText("<b>Connaissance :</b>\n" + GetTextFromEnum(item.GetCurrentKnowledgeState()));

        currentItem = item;
    }
    

    //
    public void OnDescriptionCustomChange(string updatedText)
    {
        Debug.Log("change : " + updatedText);
        GameManager.instance.mapAndPaper.currentEditText = txt_desc_custom;
        //save it 
        currentItem.description_custom = updatedText;
        //maybe : le limiter � un certains nombre de charact�res ?
    }
    public void OnStartDescriptionCustom()
    {
        Debug.Log("what did select that ???");
        //start here
        GameManager.instance.mapAndPaper.currentEditText = txt_desc_custom;
    }
    public void OnFinishDescriptionCustom()
    {
        //finish here
        GameManager.instance.mapAndPaper.currentEditText = null;
    }

    public static string GetTextFromEnum(Item.knowledgeState state)
    {
        switch (state)
        {
            case Item.knowledgeState.zero:
                return "Vous n'avez encore aucune information pour savoir o� livrer ce colis.";
            case Item.knowledgeState.talkTo:
                return "Parlez aux habitants pour d�couvrir o� livrer ce colis.";
            case Item.knowledgeState.firstInfo:
                return "Vous manquez d'informations, mais pouvez essayer de trouver le destinataire.";
            case Item.knowledgeState.fewInfo_elim:
                return "Vous manquez d'infos, mais pouvez tenter de trouver le destinataire par �limination.";
            case Item.knowledgeState.lotOfInfo:
                return "Vous avez acc�s � assez d'infos pour livrer ce colis.";
            case Item.knowledgeState.lotOfInfo_elim:
                return "Vous avez acc�s � assez d'infos pour trouver le destinataire par �limination.";
            case Item.knowledgeState.all:
                return "Vous avez croisez toutes les informations n�cessaire pour livre ce colis.";
            default:
                return "Vous �tes seule sur ce coup l�. D�brouillez-vous. Suivez votre instinct.";
        }
    }
}
