using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Dialog", menuName = "Carta/Dialog", order = 1)]
public class Dialog : ScriptableObject
{
    [Sirenix.OdinInspector.OnValueChanged("ReIndex")]
    public List<Step.Step> allSteps;

    public pnj pnj_link = null; //if not null, it's a pnj's dialog
    public Color defaultColor;

    private bool alreadyRead = false;
    public bool IsAlreadyRead()
    {
        if (alreadyRead)
            return true;

        foreach(Step.Step step in allSteps)
        {
            if (!step.alreadyRead)
                return false;
        }
        return true;
    }

    public void ReIndex()
    {
        for (int i = 0; i < allSteps.Count; i++)
        {
            allSteps[i].index = i;
        }
    }
}

namespace Step
{
    [System.Serializable]
    public abstract class Step_father {}

    [System.Serializable]
    public class Step_Dialog : Step_father
    {
        [TextArea]
        public string text = "";

        public Color color_override;
        public int cameraIndex;
    }

    [System.Serializable]
    public class Step_Camera : Step_father
    {
        public int cameraIndex;
        public bool directTP = false;
    }

    [System.Serializable]
    public class Step_AddItem: Step_father
    {
        public itemID itemId; 
    }

    [System.Serializable]
    public class Step_RemItem : Step_father
    {
        public itemID itemId;
    }

    [System.Serializable]
    public class Step_SFX : Step_father
    {
        public AudioClip sfxToPlay;
    }


    [System.Serializable]
    public class Step_Music : Step_father
    {
        public AudioClip musicToPlay;
    }


    [System.Serializable]
    public class Step_ItemInteractivity : Step_father
    {
        public bool itemInvoCanBeOpen;
    }


    [System.Serializable]
    public class Step_DialogRedirection : Step_father
    {
        public Dialog dialogToGo;
    }


    [System.Serializable]
    public class Step_SetDefaultDialog : Step_father
    {
        public pnj.pnjID targetID;
        public Dialog newDefaultDial;
    }

    [System.Serializable]
    public class Step_SetNextDialog : Step_father
    {
        public pnj.pnjID targetID;
        public Dialog dialToAdd;
        public int priority = 0;
    }

    [System.Serializable]
    public class Step_Choice : Step_father
    {
        public enum choiceType
        {
            dialogUnique,
            redirectNumber,
            redirectDialog,
        }

        public choiceType typeYes;
        [ShowIf("typeYes", choiceType.dialogUnique  )][Indent()]
        public Step_Dialog dialogYes;
        [ShowIf("typeYes", choiceType.redirectNumber)][Indent()][HorizontalGroup("YesRedirect", marginRight: -15)][LabelWidth(45)][LabelText("From")]
        public int redirectNumberIfYes;                                                         
        [ShowIf("typeYes", choiceType.redirectNumber)][Indent()][HorizontalGroup("YesRedirect", marginRight: -15)][LabelWidth(35)][LabelText("To")]
        public int redirectNumberStopYes;                                                     
        [ShowIf("typeYes", choiceType.redirectNumber)][Indent()][HorizontalGroup("YesRedirect", marginRight: -15)][LabelWidth(45)][LabelText("Then")]
        public int redirectNumberAfterYes;
        [ShowIf("typeYes", choiceType.redirectDialog)][Indent()]
        public Step_DialogRedirection redirectYes;

        public choiceType typeNo;
        [ShowIf("typeNo", choiceType.dialogUnique  )][Indent()]
        public Step_Dialog dialogNo;
        [ShowIf("typeNo", choiceType.redirectNumber)][Indent()][HorizontalGroup("NoRedirect", marginRight: -15)][LabelWidth(45)][LabelText("From")]
        public int redirectNumberIfNo;                                                      
        [ShowIf("typeNo", choiceType.redirectNumber)][Indent()][HorizontalGroup("NoRedirect", marginRight: -15)][LabelWidth(35)][LabelText("To")]
        public int redirectNumberStopNo;                                                    
        [ShowIf("typeNo", choiceType.redirectNumber)][Indent()][HorizontalGroup("NoRedirect", marginRight: -15)][LabelWidth(45)][LabelText("Then")]
        public int redirectNumberAfterNo;
        [ShowIf("typeNo", choiceType.redirectDialog)][Indent()]
        public Step_DialogRedirection redirectNo;
    }

}

