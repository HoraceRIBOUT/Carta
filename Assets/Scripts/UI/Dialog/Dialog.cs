using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


[CreateAssetMenu(fileName = "Dialog", menuName = "Carta/Dialog", order = 1)]
public class Dialog : ScriptableObject
{
    public static string CASE_SEPARATOR = ";";
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
    public abstract class Step_father
    {
        public abstract string ToCSVLine();
    }

    [System.Serializable]
    public class Step_Dialog : Step_father
    {

        [TextArea]
        public string text = "";

        public Color color_override;

        public override string ToCSVLine()
        {
            return "Step_Dialog "+ Dialog.CASE_SEPARATOR+ text + Dialog.CASE_SEPARATOR + color_override;
        }

        public Step_Dialog(string csvLine)
        {
            //cut the line between the "SEPARATOR"
            string[] eachCase = csvLine.Split(Dialog.CASE_SEPARATOR);
            //and put the text in it 
            text = eachCase[1];
            //and the color then if it have one
            if (!ColorUtility.TryParseHtmlString(eachCase[2], out color_override))
                Debug.LogError("Error when parsing color : " + csvLine);

        }
    }

    [System.Serializable]
    public class Step_Camera : Step_father
    {
        public int cameraIndex;
        public bool directTP = false;
        public override string ToCSVLine()
        {
            return "Step_Camera " + Dialog.CASE_SEPARATOR + cameraIndex + Dialog.CASE_SEPARATOR + directTP.ToString();
        }
    }

    [System.Serializable]
    public class Step_AddItem: Step_father
    {
        public itemID itemId;
        public override string ToCSVLine()
        {
            return "Step_AddItem " + Dialog.CASE_SEPARATOR + itemId;
        }
    }

    [System.Serializable]
    public class Step_RemItem : Step_father
    {
        public itemID itemId;
        public override string ToCSVLine()
        {
            return "Step_RemItem " + Dialog.CASE_SEPARATOR + itemId;
        }
    }

    [System.Serializable]
    public class Step_SFX : Step_father
    {
        public AudioClip sfxToPlay;
        public override string ToCSVLine()
        {
            return "Step_SFX " + Dialog.CASE_SEPARATOR + sfxToPlay.name;
        }
    }


    [System.Serializable]
    public class Step_Music : Step_father
    {
        public AudioClip musicToPlay;
        public override string ToCSVLine()
        {
            return "Step_Music " + Dialog.CASE_SEPARATOR + musicToPlay.name;
        }
    }


    [System.Serializable]
    public class Step_ItemInteractivity : Step_father
    {
        public bool itemInvoCanBeOpen;
        public override string ToCSVLine()
        {
            return "Step_ItemInteractivity " + Dialog.CASE_SEPARATOR + itemInvoCanBeOpen.ToString();
        }
    }


    [System.Serializable]
    public class Step_DialogRedirection : Step_father
    {
        public Dialog dialogToGo;
        public override string ToCSVLine()
        {
            return "Step_DialogRedirection " + Dialog.CASE_SEPARATOR + dialogToGo.name;
        }
    }


    [System.Serializable]
    public class Step_SetDefaultDialog : Step_father
    {
        public pnj.pnjID targetID;
        public Dialog newDefaultDial;
        public override string ToCSVLine()
        {
            return "Step_SetDefaultDialog " + Dialog.CASE_SEPARATOR + targetID  + Dialog.CASE_SEPARATOR + newDefaultDial.name;
        }
    }

    [System.Serializable]
    public class Step_SetNextDialog : Step_father
    {
        public pnj.pnjID targetID;
        public Dialog dialToAdd;
        public int priority = 0;
        public override string ToCSVLine()
        {
            return "Step_SetNextDialog " + Dialog.CASE_SEPARATOR + targetID + Dialog.CASE_SEPARATOR + dialToAdd.name + Dialog.CASE_SEPARATOR + priority;
        }
    }

    [System.Serializable]
    public class Step_Animation : Step_father
    {
        public pnj.pnjID targetID;
        public int animIndex = 0;
        public override string ToCSVLine()
        {
            return "Step_Animation " + Dialog.CASE_SEPARATOR + targetID + Dialog.CASE_SEPARATOR + animIndex;
        }
    }

    [System.Serializable]
    public class Step_ChangeFace : Step_father
    {
        public pnj.pnjID targetID;
        public int eyesIndex = 0;
        public int mouthIndex = 0;
        public override string ToCSVLine()
        {
            return "Step_ChangeFace " + Dialog.CASE_SEPARATOR + targetID + Dialog.CASE_SEPARATOR + eyesIndex + Dialog.CASE_SEPARATOR + mouthIndex;
        }
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


        public override string ToCSVLine()
        {
            //TO DO 
            return "Step_Dialog " + Dialog.CASE_SEPARATOR + typeYes + Dialog.CASE_SEPARATOR + typeNo;
        }
    }

}

