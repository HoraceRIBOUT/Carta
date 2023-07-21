using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Dialog", menuName = "Carta/Dialog", order = 1)]
public class Dialog : ScriptableObject
{
    public static string CASE_SEPARATOR = "\t";
    [Sirenix.OdinInspector.OnValueChanged("ReIndex")]
    public List<Step.Step> allSteps = new List<Step.Step>();

    public pnj pnj_link = null; //if not null, it's a pnj's dialog
    public Color defaultColor;

    private bool alreadyRead = false;
    public bool IsAlreadyRead()
    {
        if (alreadyRead)
            return true;

        foreach (Step.Step step in allSteps)
        {
            if (!step.alreadyRead)
                return false;
            Debug.Log(step.type + " is already read.");
        }
        Debug.Log("Didnt cross any false !");
        alreadyRead = true;
        return true;
    }
    public bool HaveBeenLaunchedOnce()
    {
        if (alreadyRead)
            return true;
        if (allSteps.Count == 0)
        {
            Debug.LogError(this.name + " have 0 steps.");
            return false;
        }
        return allSteps[0].alreadyRead;
    }

    [Sirenix.OdinInspector.Button()]
    public void ReIndex()
    {
        for (int i = 0; i < allSteps.Count; i++)
        {
            allSteps[i].index = i;
        }
    }

    //#if UNITY_EDITOR
    public Step.stepType AddStep(string line)
    {
        string[] lineSplit = line.Split(CASE_SEPARATOR);
        Step.Step newStep = Step.Step.SetUpStepFromLine(lineSplit);
        if(newStep != null)
            allSteps.Add(newStep);
        return newStep.type;
    }
    //#endif
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
        public pnj.pnjID pnj_override;

        public override string ToCSVLine()
        {
            return Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR 
                + "dialog" + Dialog.CASE_SEPARATOR + text
                + (color_override == Color.clear ?
                Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR :
                Dialog.CASE_SEPARATOR + ColorUtility.ToHtmlStringRGBA(color_override))
                + (pnj_override == pnj.pnjID.None?
                Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR :
                Dialog.CASE_SEPARATOR + pnj_override);
        }

        public Step_Dialog(string[] splitLine)
        {
            //and put the text in it 
            text = splitLine[4];
            //and the color then if it have one
            if (splitLine[5].Trim() != "")
                if (!ColorUtility.TryParseHtmlString(splitLine[5], out color_override))
                    Debug.LogError("Error when parsing color : " + splitLine[5]);
            pnj_override = pnj.pnjID.None;
            if(splitLine[6].Trim() != "")
                pnj_override = CreateCSV.GetPnjIdFromString(splitLine[6]);
        }

        public Step_Dialog(string uniqueLine)
        {
            //and put the text in it 
            text = uniqueLine;
        }
    }

    [System.Serializable]
    public class Step_Camera : Step_father
    {
        public int cameraIndex;
        public bool directTP = false;
        public override string ToCSVLine()
        {
            return Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR
                + "camera" + Dialog.CASE_SEPARATOR + cameraIndex + Dialog.CASE_SEPARATOR + directTP.ToString();
        }

        public Step_Camera(string[] splitLine)
        {
            //and put the index here 
            if (int.TryParse(splitLine[4], out int res))
                cameraIndex = res;
            else
                Debug.LogError("CAMERA CANNOT PARSE THE NEXT PART : " + splitLine[4]);
            //and the bool then if it have one
            if (splitLine[5].Trim() != "")
                if (!bool.TryParse(splitLine[5], out directTP))
                    Debug.LogError("Error when parsing boolean : " + splitLine[5]);
        }
    }

    [System.Serializable]
    public class Step_AddItem : Step_father
    {
        public itemID itemId;
        public override string ToCSVLine()
        {
            return Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR
                + "additem" + Dialog.CASE_SEPARATOR + itemId;
        }

        public Step_AddItem(string[] splitLine)
        {
            itemId = CreateCSV.GetItemIDFromString(splitLine[4]);
        }
    }

    [System.Serializable]
    public class Step_RemItem : Step_father
    {
        public itemID itemId;
        public override string ToCSVLine()
        {
            return Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR
                + "remitem" + Dialog.CASE_SEPARATOR + itemId;
        }

        public Step_RemItem(string[] splitLine)
        {
            itemId = CreateCSV.GetItemIDFromString(splitLine[4]);
        }
    }

    [System.Serializable]
    public class Step_SFX : Step_father
    {
        public AudioClip sfxToPlay;
        public override string ToCSVLine()
        {
            return Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR
                + "sfx" + Dialog.CASE_SEPARATOR + sfxToPlay.name;
        }

        public Step_SFX(string[] splitLine)
        {
            //Should create a cool way to launch SFX (maybe a list by character ? that we have to register ?)
        }
    }


    [System.Serializable]
    public class Step_Music : Step_father
    {
        public AudioClip musicToPlay;
        public override string ToCSVLine()
        {
            return Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR
                + "music" + Dialog.CASE_SEPARATOR + musicToPlay.name;
        }

        public Step_Music(string[] splitLine)
        {
            //Should create a cool way to launch MUSIC (maybe a worldly list? launch by index ?)
        }
    }


    [System.Serializable]
    public class Step_ItemInteractivity : Step_father
    {
        public bool itemInvoCanBeOpen;
        public override string ToCSVLine()
        {
            return Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR
                + "iteminteractivity" + Dialog.CASE_SEPARATOR + itemInvoCanBeOpen.ToString();
        }

        public Step_ItemInteractivity(string[] splitLine)
        {
            if (splitLine[4].Trim() != "")
                if (!bool.TryParse(splitLine[4], out itemInvoCanBeOpen))
                    Debug.LogError("Error when parsing boolean : " + splitLine[4]);
        }
    }


    [System.Serializable]
    public class Step_DialogRedirection : Step_father
    {
        public Dialog dialogToGo;
        public override string ToCSVLine()
        {
            return Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR
                + "dialogredirection" + Dialog.CASE_SEPARATOR + dialogToGo.name;
        }

        public Step_DialogRedirection(string[] splitLine)
        {
            //The "dialogToGo" will be set automaticly by the FixLater() code
        }
        public Step_DialogRedirection(Dialog dialog)
        {
            dialogToGo = dialog;
        }
    }


    [System.Serializable]
    public class Step_SetDefaultDialog : Step_father
    {
        public pnj.pnjID targetID;
        public Dialog newDefaultDial;
        public override string ToCSVLine()
        {
            return Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR
                + "setdefaultdialog" + Dialog.CASE_SEPARATOR + targetID + Dialog.CASE_SEPARATOR + newDefaultDial.name;
        }

        public Step_SetDefaultDialog(string[] splitLine)
        {
            targetID = CreateCSV.GetPnjIdFromString(splitLine[4]);//Get pnj from string ?

            //The "newDefaultDial" will be set automaticly by the FixLater() code
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
            return Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR
                + "setnextdialog" + Dialog.CASE_SEPARATOR + targetID + Dialog.CASE_SEPARATOR + dialToAdd.name + Dialog.CASE_SEPARATOR + priority;
        }

        public Step_SetNextDialog(string[] splitLine)
        {
            targetID = CreateCSV.GetPnjIdFromString(splitLine[4]);

            //The "dialToAdd" will be set automaticly by the FixLater() code

            if (int.TryParse(splitLine[6], out int res))
                priority = res;
            else
                Debug.LogError("Need a priority for the SetNextDialog '"+ splitLine[5] + "'. CANNOT PARSE THIS PART : " + splitLine[6]);

        }
    }

    [System.Serializable]
    public class Step_Animation : Step_father
    {
        public pnj.pnjID targetID;
        public int animIndex = 0;
        public override string ToCSVLine()
        {
            return Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR
                + "animation" + Dialog.CASE_SEPARATOR + targetID + Dialog.CASE_SEPARATOR + animIndex;
        }

        public Step_Animation(string[] splitLine)
        {
            targetID = CreateCSV.GetPnjIdFromString(splitLine[4]);

            if (int.TryParse(splitLine[5], out int res))
                animIndex = res;
            else
                Debug.LogError("CAMERA CANNOT PARSE THE NEXT PART : " + splitLine[5]);
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
            return Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR
                + "changeface" + Dialog.CASE_SEPARATOR + targetID + Dialog.CASE_SEPARATOR + eyesIndex + Dialog.CASE_SEPARATOR + mouthIndex;
        }

        public Step_ChangeFace(string[] splitLine)
        {
            targetID = CreateCSV.GetPnjIdFromString(splitLine[4]);

            if (int.TryParse(splitLine[5], out int res))
                eyesIndex = res;
            else
                Debug.LogError("CAMERA CANNOT PARSE THE NEXT PART : " + splitLine[5]);

            if (int.TryParse(splitLine[6], out res))
                mouthIndex = res;
            else
                Debug.LogError("CAMERA CANNOT PARSE THE NEXT PART : " + splitLine[6]);
        }
    }

    [System.Serializable]
    public class Step_ChangeVisual : Step_father
    {
        public pnj.pnjID targetID;
        public int visualIndex = 0;
        public override string ToCSVLine()
        {
            return Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR
                + "changevisual" + Dialog.CASE_SEPARATOR + targetID + Dialog.CASE_SEPARATOR + visualIndex + Dialog.CASE_SEPARATOR;
        }

        public Step_ChangeVisual(string[] splitLine)
        {
            targetID = CreateCSV.GetPnjIdFromString(splitLine[4]);

            if (int.TryParse(splitLine[5], out int res))
                visualIndex = res;
            else
                Debug.LogError("Change visual : the index is not a number : " + splitLine[5]);

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
        [ShowIf("typeYes", choiceType.dialogUnique)]
        [Indent()]
        public Step_Dialog dialogYes;
        [ShowIf("typeYes", choiceType.redirectNumber)]
        [Indent()]
        [HorizontalGroup("YesRedirect", marginRight: -15)]
        [LabelWidth(45)]
        [LabelText("From")]
        public int redirectNumberIfYes;
        [ShowIf("typeYes", choiceType.redirectNumber)]
        [Indent()]
        [HorizontalGroup("YesRedirect", marginRight: -15)]
        [LabelWidth(35)]
        [LabelText("To")]
        public int redirectNumberStopYes;
        [ShowIf("typeYes", choiceType.redirectNumber)]
        [Indent()]
        [HorizontalGroup("YesRedirect", marginRight: -15)]
        [LabelWidth(45)]
        [LabelText("Then")]
        public int redirectNumberAfterYes;
        [ShowIf("typeYes", choiceType.redirectDialog)]
        [Indent()]
        public Step_DialogRedirection redirectYes;

        public choiceType typeNo;
        [ShowIf("typeNo", choiceType.dialogUnique)]
        [Indent()]
        public Step_Dialog dialogNo;
        [ShowIf("typeNo", choiceType.redirectNumber)]
        [Indent()]
        [HorizontalGroup("NoRedirect", marginRight: -15)]
        [LabelWidth(45)]
        [LabelText("From")]
        public int redirectNumberIfNo;
        [ShowIf("typeNo", choiceType.redirectNumber)]
        [Indent()]
        [HorizontalGroup("NoRedirect", marginRight: -15)]
        [LabelWidth(35)]
        [LabelText("To")]
        public int redirectNumberStopNo;
        [ShowIf("typeNo", choiceType.redirectNumber)]
        [Indent()]
        [HorizontalGroup("NoRedirect", marginRight: -15)]
        [LabelWidth(45)]
        [LabelText("Then")]
        public int redirectNumberAfterNo;
        [ShowIf("typeNo", choiceType.redirectDialog)]
        [Indent()]
        public Step_DialogRedirection redirectNo;


        public override string ToCSVLine()
        {
            //TO DO : redirect number + return value corrected
            return Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR
                + "choice" + Dialog.CASE_SEPARATOR
                + typeYes + Dialog.CASE_SEPARATOR
                + (typeYes == choiceType.dialogUnique ? dialogYes.text : (typeYes == choiceType.redirectDialog ? redirectYes.dialogToGo.name : "NUMBER_TO_RECALCULATE : " + redirectNumberIfYes))
                + Dialog.CASE_SEPARATOR
                + typeNo + Dialog.CASE_SEPARATOR
                + (typeNo == choiceType.dialogUnique ? dialogNo.text : (typeNo == choiceType.redirectDialog ? redirectNo.dialogToGo.name : "NUMBER_TO_RECALCULATE : " + redirectNumberIfNo))
                + Dialog.CASE_SEPARATOR
                + (typeYes == choiceType.redirectNumber ? "RETURN : " + Dialog.CASE_SEPARATOR + redirectNumberStopYes
                : (typeNo == choiceType.redirectNumber ? "RETURN : " + Dialog.CASE_SEPARATOR + redirectNumberStopNo
                : Dialog.CASE_SEPARATOR))
                ;
        }

        public Step_Choice(string[] splitLine)
        {
            //will be deal later by "Fix Choice" to have access to both dictionnary (by index, and by name)
        }

        public void FixChoice(Dictionary<string, Dialog> dicoName, Dictionary<int, Dialog> dicoIndex, string wholeLine, int firstLineIndex)
        {
            //Big function incoming
            string[] splitLine = wholeLine.Split(Dialog.CASE_SEPARATOR);

            //For the case where we redirect to a number : need a "return" value at place 9
            redirectNumberAfterYes = -1;
            redirectNumberAfterNo = -1;
            if (splitLine.Length > 9)
            {
                if (int.TryParse(splitLine[9].Trim(), out int returnResult))
                {
                    redirectNumberAfterYes = returnResult;
                    redirectNumberAfterNo = returnResult;
                }
            }

            //No part
            string noPart = splitLine[7].Trim();
            Debug.Log("Try parse to int : ." + noPart + ".");
            if (int.TryParse(noPart, out int res))                //it's a number !
            {
                typeNo = choiceType.redirectNumber;
                //So : the res number is dependant of the lineStart of the dialog here. How can I have it ?

                redirectNumberIfNo = res - firstLineIndex - 1; //maybe will need a -1
                redirectNumberStopNo = redirectNumberAfterNo - 1;//by default.
            }
            else if (noPart.Contains(" ") || noPart.Contains('.'))                //it's a line !
            {
                typeNo = choiceType.dialogUnique;

                dialogNo = new Step_Dialog(noPart);
            }
            else            //it's a name !
            {
                typeNo = choiceType.redirectDialog;
                redirectNo = new Step_DialogRedirection(dicoName[noPart]);
            }

            //Yes part:
            string yesPart = splitLine[5].Trim();
            if(int.TryParse(yesPart, out  res))                //it's a number !
            {
                typeYes = choiceType.redirectNumber;
                //So : the res number is dependant of the lineStart of the dialog here. How can I have it ?
            
                redirectNumberIfYes = res - firstLineIndex - 1; //maybe will need a -1
                if (typeNo == choiceType.redirectNumber)
                    redirectNumberStopYes = redirectNumberIfNo - 1;
                else
                    redirectNumberStopYes = redirectNumberAfterYes - 1;
            }
            else if(yesPart.Contains(" ") || yesPart.Contains('.'))                //it's a line !
            {
                typeYes = choiceType.dialogUnique;
            
                dialogYes = new Step_Dialog(yesPart);
            }
            else            //it's a name !
            {
                typeYes = choiceType.redirectDialog;
                redirectYes = new Step_DialogRedirection(dicoName[yesPart]);
            }
        }


    }


    [System.Serializable]
    public class Step_UnlockPaper : Step_father
    {
        public int papersIndex = 0;
        public override string ToCSVLine()
        {
            return Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR
                + "unlock : " + Dialog.CASE_SEPARATOR + papersIndex;
        }

        public Step_UnlockPaper(string[] splitLine)
        {
            if (int.TryParse(splitLine[4], out int res))
                papersIndex = res;
            else
                Debug.LogError("Unlock paper but index CANNOT PARSE TO INT : " + splitLine[4]);
        }
    }

    [System.Serializable]
    public class Step_ZoneChange : Step_father
    {
        public string zoneName = "";
        public bool change;
        public override string ToCSVLine()
        {
            return Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR + Dialog.CASE_SEPARATOR
                + zoneName + Dialog.CASE_SEPARATOR + change.ToString();
        }

        public Step_ZoneChange(string[] splitLine)
        {
            zoneName = splitLine[4].Trim();

            if (!bool.TryParse(splitLine[5].Trim(), out change))
                Debug.LogError("Error when parsing boolean : " + splitLine[5]);
        }
    }
}

