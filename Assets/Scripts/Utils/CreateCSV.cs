using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateCSV : MonoBehaviour
{
    public enum DialogType
    {
        None,
        idleDial,       //default dial, where you give or show item
        nextDial,      //dial who are only play once 
        showFail,       //the default reaction when show
        showRes,        //PNJ + ITEM : the reaction of PNJ when show ITEM
        giveFail,       //the default reaction when give an item
        giveRes,        //PNJ + ITEM : the reaction of PNJ when give ITEM (plus some data to say it's the win or not ?)
        zone,           //for the dialog who trigger when entering a zone 
        newDial,        //for the rest
    }
    //Special directories : zone , others and item 

    #region CreateCSV
    public static void WriteDialogInCVS(string filePath, List<Dialog> dialogs)
    {
        if (!File.Exists(filePath))
        {
            File.Delete(filePath);
        }


        //Writing
        using (StreamWriter outfile =
            new StreamWriter(filePath))
        {
            foreach (Dialog dialog in dialogs)
            {
                string dialogDefineLine = "";
                //get the type :
                // showDialog
                if (dialog.name.Contains(       "_ShowFail"))
                    dialogDefineLine = (DialogType.showFail      ).ToString();

                else if (dialog.name.Contains(  "_GiveFail"))
                    dialogDefineLine = (DialogType.giveFail      ).ToString();
                
                else if (dialog.name.Contains(  "_DefaultDialog"))
                    dialogDefineLine = (DialogType.idleDial      ).ToString();

                else if (dialog.name.Contains(  "_Once_"))
                    dialogDefineLine = (DialogType.nextDial  ).ToString();

                else if (dialog.name.Contains(  "_zone"))
                    dialogDefineLine = (DialogType.zone  ).ToString();


                else
                    dialogDefineLine = (DialogType.newDial          ).ToString();

                //get the pnj/item/what it concern
                dialogDefineLine += Dialog.CASE_SEPARATOR + dialog.name.Remove(dialog.name.IndexOf("_"));
                Debug.Log("dialog.name.Remove(dialog.name.IndexOf('_') = " + (dialog.name.Remove(dialog.name.IndexOf("_"))));

                outfile.WriteLine(dialogDefineLine);

                foreach ( Step.Step stepDial in dialog.allSteps)
                {
                    string s = stepDial.GetData().ToCSVLine();
                    outfile.WriteLine(s);
                }
            }
            
        }

        if (filePath.Contains('/'))
        {
            string folderPath = filePath.Substring(0, filePath.LastIndexOf('/'));
            folderPath = Directory.GetCurrentDirectory() + "/" + folderPath;
            Application.OpenURL(folderPath);
            Debug.Log("Try open : " + "." + folderPath + ".");
        }

    }
    #endregion










    #region Read CSV To Create Dialog
#if UNITY_EDITOR
    [MenuItem("OrangeLetter/Generate/Dialog From CSV")]
    public static void LoadCSVToDialog()
    {
        Debug.Log("Selected: " + Selection.activeObject.name);
        TextAsset textAss = (TextAsset)Selection.activeObject;
        if (textAss == null)
            return;

        dialogOnThisCSV = new Dictionary<int, Dialog>();
        toFixLater.Clear();

        string wholeText = textAss.text;
        string[] splitedText = wholeText.Split("\n");
        Debug.Log("Load " + splitedText.Length + "lines. The text load is : \n" + wholeText);

        for (int i = 0; i < splitedText.Length; i++)
        {
            string textLine = splitedText[i];
            //Seek what type of dial it is : 
            //if (dialogLineType != DialogType.None) then : treat it depending of the type of dialog we need to create
            //              --> in the function, advance the line position (i)
            //              when finish, return the position (i)
            //  redo it until no more line.
            //every line who cannot proceed is return as error!



            DialogType dialogFileType = TypeOfDialogFileFromLine(textLine);
            if (dialogFileType == DialogType.None)
            {
                Debug.LogError("Can't recognize : " + textLine);
                //then, continue
            }
            else
            {
                Debug.Log("Recognize new dial : " + textLine);
                i = CreateDialogFromLineList(splitedText, i);
                //then, continue

            }

        }
        FixNow();
        //add the dialog when needed.

    }

    static int CreateDialogFromLineList(string[] splitedText, int firstLineIndex)
    {
        Debug.Log("(" + firstLineIndex + ") Create dialogue from : " + splitedText[firstLineIndex]);
        Dialog dial = CreateDialogFromThis(splitedText[firstLineIndex]);
        for (int i = firstLineIndex + 1;  i < splitedText.Length; i++)
        {
            string textLine = splitedText[i];
            DialogType dialogFileType = TypeOfDialogFileFromLine(textLine);
            if (dialogFileType != DialogType.None)
            {
                Debug.Log("Finish dialog creation." + i + " Have "+dial.allSteps.Count +" steps.");
                DialogCreate(dial, firstLineIndex);
                return i - 1;
            }
            else
            {
                //Reach the line type
                Debug.Log("Read : " + textLine);
                var newType = dial.AddStep(textLine);
                switch (newType)
                {
                    case Step.stepType.dialogredirection:
                    case Step.stepType.setnextdialog:
                    case Step.stepType.setdefaultdialog:
                        string[] lineSplit = textLine.Split(Dialog.CASE_SEPARATOR);
                        if (int.TryParse(lineSplit[5], out int dialIndex))
                            AddToFixLater(dial, i - firstLineIndex - 1, dialIndex);
                        else
                            AddToFixLater(dial, i - firstLineIndex - 1, lineSplit[5]);
                        break;
                    case Step.stepType.choice:
                        //what a hell.
                        AddToFixLater(dial, i - firstLineIndex - 1, textLine);
                        break;
                }
            }
        }
        //we reach the end of the file.
        DialogCreate(dial, firstLineIndex);
        return splitedText.Length;
    }

    static Dialog CreateDialogFromThis(string firstLine)
    {
        DialogType dialogLineType = TypeOfDialogFileFromLine(firstLine);
        //
        string[] splittedLine = firstLine.Split(Dialog.CASE_SEPARATOR); 
        
        string finalPath = "Assets/Data/Dialog/";
        string finalName = "";

        Debug.Log("firstLine = " + firstLine + " || splitted = " + splittedLine.Length);
        string itemName = splittedLine[1].Trim();
        string pnjName = splittedLine[2].Trim();

        switch (dialogLineType)
        {
            case DialogType.None:
                break;
            case DialogType.idleDial:
                if (pnjName != ""){ finalPath += pnjName + "/"; }                      
                finalName = pnjName + "_DefaultDialog" /*add if custom text*/+ (splittedLine[1].Trim() == "" ? "" : "_" + splittedLine[1]);
                break;
            case DialogType.nextDial:
                if (pnjName != "") { finalPath += pnjName + "/"; }
                finalName = pnjName + "_Once_" + splittedLine[1];
                break;
            case DialogType.showFail:
                if (pnjName != ""){ finalPath += pnjName + "/"; }                       
                finalName = pnjName + "_ShowFail";                           
                break;
            case DialogType.showRes:
                if (pnjName != "") { finalPath += "item/" + itemName + "/"; }
                finalName = itemName + "_" + pnjName + "_Show";
                break;        
            case DialogType.giveFail:
                if (pnjName != "") { finalPath += pnjName + "/"; }
                finalName = pnjName + "_GiveFail";
                break;
            case DialogType.giveRes:
                if (pnjName != "") { finalPath += "item/" + itemName + "/"; }
                finalName = itemName + "_" + pnjName + "_Give";
                break;
            case DialogType.zone:
                if (pnjName != "")
                {
                    finalPath += pnjName + "/";
                    finalName = pnjName + "_zone_" + splittedLine[1];
                }
                else
                {
                    finalPath += "zone/";
                    finalName = "zone_" + splittedLine[1];
                }
                break;
            case DialogType.newDial:
                if (pnjName != "")
                {
                    finalPath += pnjName + "/";
                    finalName = pnjName + "_" + splittedLine[1];
                }
                else
                {
                    finalPath += "others/";
                    finalName = splittedLine[1];
                }
                break;
            default:
                break;
        }

        if (!Directory.Exists(finalPath))
        {
            Debug.Log("Create directory " + finalPath);
            Directory.CreateDirectory(finalPath);
        }

        Dialog dialogAsset;
        bool alreadyExist = System.IO.File.Exists(finalPath + finalName + ".asset");
        if (alreadyExist)
        {
            //TO DO : rename it for safety and erase the previous one
            //For now : destructive.
            Debug.LogWarning("Already exists : " + finalName + " at " + finalPath + ". We delete it. Goodbye lost file!");
            //File.Delete(finalPath + finalName + ".asset");
            dialogAsset = (Dialog)UnityEditor.AssetDatabase.LoadAssetAtPath(finalPath + finalName + ".asset", typeof(Dialog));
            dialogAsset.allSteps.Clear();
        }
        else
        {
            dialogAsset = ScriptableObject.CreateInstance<Dialog>();
        }

        dialogAsset.name = finalName;
        if(!alreadyExist)
            UnityEditor.AssetDatabase.CreateAsset(dialogAsset, finalPath + finalName + ".asset");

        UnityEditor.Selection.activeObject = dialogAsset;
        Debug.Log("Create : " + finalName + " at " + finalPath + ".", dialogAsset);
        return dialogAsset;        
    }


    static List<DialogToFixLater> toFixLater = new List<DialogToFixLater>();
    static Dictionary<int,Dialog> dialogOnThisCSV = new Dictionary<int, Dialog>();
    class DialogToFixLater
    {
        Dialog mySelf;
        int stepIndex;
        int indexOfTheNeededDialog;
        string nameOfTheNeededDialog;
        public DialogToFixLater(Dialog _dialog, int _index, int _indexOfTheDialog)
        {
            mySelf = _dialog;
            stepIndex = _index;
            indexOfTheNeededDialog = _indexOfTheDialog;
        }
        public DialogToFixLater(Dialog _dialog, int _index, string _nameOfTheDialog)
        {
            mySelf = _dialog;
            stepIndex = _index;
            indexOfTheNeededDialog = -1;
            nameOfTheNeededDialog = _nameOfTheDialog;
        }
        public void Fix(Dictionary<string, Dialog> dicoName, Dictionary<int, Dialog> dicoIndex)
        {
            Debug.Log("Dialog null ?" + (mySelf == null?"Yes":"No"));
            Debug.Log("Dialog (" + mySelf.name + ") steps ?" + mySelf.allSteps.Count + " VS " + stepIndex);
            switch (mySelf.allSteps[stepIndex].type)
            {
                case Step.stepType.dialogredirection:
                    if(indexOfTheNeededDialog != -1)
                    {
                        if (!dicoIndex.ContainsKey(indexOfTheNeededDialog)) Debug.LogError("CSV don't contain a dialog at " + indexOfTheNeededDialog);
                        mySelf.allSteps[stepIndex].dialogredirection_Data.dialogToGo = dicoIndex[indexOfTheNeededDialog];
                    }
                    else
                    {
                        if (!dicoName.ContainsKey(nameOfTheNeededDialog)) Debug.LogError("File don't contain any dialog named " + nameOfTheNeededDialog);
                        mySelf.allSteps[stepIndex].dialogredirection_Data.dialogToGo = dicoName[nameOfTheNeededDialog];
                    }
                    break;
                case Step.stepType.setdefaultdialog:
                    if (indexOfTheNeededDialog != -1)
                    {
                        if (!dicoIndex.ContainsKey(indexOfTheNeededDialog)) Debug.LogError("CSV don't contain a dialog at " + stepIndex);
                        mySelf.allSteps[stepIndex].setdefaultdialog_Data.newDefaultDial = dicoIndex[indexOfTheNeededDialog];
                    }
                    else
                    {
                        if (!dicoName.ContainsKey(nameOfTheNeededDialog)) Debug.LogError("File don't contain any dialog named " + nameOfTheNeededDialog);
                        mySelf.allSteps[stepIndex].setdefaultdialog_Data.newDefaultDial = dicoName[nameOfTheNeededDialog];
                    }
                    break;
                case Step.stepType.setnextdialog:
                    if (indexOfTheNeededDialog != -1)
                    {
                        if (!dicoIndex.ContainsKey(indexOfTheNeededDialog)) Debug.LogError("CSV don't contain a dialog at " + stepIndex);
                        mySelf.allSteps[stepIndex].setnextdialog_Data.dialToAdd = dicoIndex[indexOfTheNeededDialog];
                    }
                    else
                    {
                        if (!dicoName.ContainsKey(nameOfTheNeededDialog)) Debug.LogError("File don't contain any dialog named " + nameOfTheNeededDialog);
                        mySelf.allSteps[stepIndex].setnextdialog_Data.dialToAdd = dicoName[nameOfTheNeededDialog];
                    }
                    break;
                case Step.stepType.choice:
                    int indexOfMySelf = -1;
                    foreach (var pair in dicoIndex)
                    {
                        if (mySelf.name == pair.Value.name)
                            indexOfMySelf = pair.Key;
                    }
                    if (indexOfMySelf == -1)
                    {
                        Debug.LogError("Choice cannot find himself in the list");
                        indexOfMySelf = 0;//failsafe to avoid another error after that.
                    }

                    mySelf.allSteps[stepIndex].choice_Data.FixChoice(dicoName, dicoIndex, nameOfTheNeededDialog, indexOfMySelf);
                    break;
            }
        }
    }
    static void DialogCreate(Dialog dialog, int firstIndex)
    {
        dialogOnThisCSV.Add(firstIndex + 1, dialog); //PLUS ONE because the excel start at 1, when the array start at 0
    }
    public static void AddToFixLater(Dialog dialog, int currentIndex, int indexOfDialog)
    {
        toFixLater.Add(new DialogToFixLater(dialog, currentIndex, indexOfDialog));
    }
    public static void AddToFixLater(Dialog dialog, int currentIndex, string nameOfDialog)
    {
        toFixLater.Add(new DialogToFixLater(dialog, currentIndex, nameOfDialog));
    }
    static void FixNow()
    {
        if (toFixLater == null || toFixLater.Count == 0)
        {
            dialogOnThisCSV = new Dictionary<int, Dialog>();
            return;
        }
        Dictionary<string, Dialog> dico = new Dictionary<string, Dialog>();
        //here, make the string, Dialog list, and then, go fetch them. Trim().ToLower() 

        string largestPath = "Assets/Data/Dialog/";
        Debug.Log("Go seek in  " + largestPath);
        foreach (var filePath in Directory.EnumerateFiles(largestPath, "*.asset", SearchOption.AllDirectories))
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath.Trim());
            Debug.Log("What filename are you : " + fileName + " (fullPath = " + filePath + ")");
            //Get rid of the directory : only the filename ! (and test if it's a dialog, please, by the mother of god)

            Dialog dial = (Dialog)UnityEditor.AssetDatabase.LoadAssetAtPath(filePath, typeof(Dialog));
            if(dial != null)
                dico.Add(fileName, dial);
        }
        Debug.Log("Finish seek but found " + dico.Count + " files.");

        foreach (DialogToFixLater dialStruc in toFixLater)
        {
            dialStruc.Fix(dico, dialogOnThisCSV);
        }

        foreach (KeyValuePair<int, Dialog> pair in dialogOnThisCSV)
        {
            Debug.Log("List of the asset created : " + pair.Value.name + " .", pair.Value);
        }

        toFixLater.Clear();
        dialogOnThisCSV.Clear();
    }
#endif

    #endregion








    #region Get Data From String
    public static DialogType TypeOfDialogFileFromLine(string textLine)
    {
             if (textLine.StartsWith(DialogType.idleDial .ToString())) return DialogType.idleDial ;
        else if (textLine.StartsWith(DialogType.nextDial .ToString())) return DialogType.nextDial ;
        else if (textLine.StartsWith(DialogType.showFail .ToString())) return DialogType.showFail ;
        else if (textLine.StartsWith(DialogType.showRes  .ToString())) return DialogType.showRes  ;
        else if (textLine.StartsWith(DialogType.giveFail .ToString())) return DialogType.giveFail ;
        else if (textLine.StartsWith(DialogType.giveRes  .ToString())) return DialogType.giveRes  ;
        else if (textLine.StartsWith(DialogType.zone     .ToString())) return DialogType.zone     ;
        else if (textLine.StartsWith(DialogType.newDial  .ToString())) return DialogType.newDial  ;
        else                                                           return DialogType.None     ;

    }                                           
    
    public static Step.stepType GetStepTypeFromLine(string lineExtract)
    {
        if(lineExtract.Trim() == "")    //to avoid to add "dialog" in each line, when it's the default.
            return Step.stepType.dialog;
        lineExtract = lineExtract.Trim().ToLower();

             if (lineExtract == Step.stepType.dialog            .ToString().Trim().ToLower() ) return Step.stepType.dialog              ;
        else if (lineExtract == Step.stepType.camera            .ToString().Trim().ToLower() ) return Step.stepType.camera              ; 
        else if (lineExtract == Step.stepType.additem           .ToString().Trim().ToLower() ) return Step.stepType.additem             ;
        else if (lineExtract == Step.stepType.remitem           .ToString().Trim().ToLower() ) return Step.stepType.remitem             ;
        else if (lineExtract == Step.stepType.sfx               .ToString().Trim().ToLower() ) return Step.stepType.sfx                 ;
        else if (lineExtract == Step.stepType.music             .ToString().Trim().ToLower() ) return Step.stepType.music               ;
        else if (lineExtract == Step.stepType.iteminteractivity .ToString().Trim().ToLower() ) return Step.stepType.iteminteractivity   ;
        else if (lineExtract == Step.stepType.dialogredirection .ToString().Trim().ToLower() ) return Step.stepType.dialogredirection   ;
        else if (lineExtract == Step.stepType.setdefaultdialog  .ToString().Trim().ToLower() ) return Step.stepType.setdefaultdialog    ;
        else if (lineExtract == Step.stepType.setnextdialog     .ToString().Trim().ToLower() ) return Step.stepType.setnextdialog       ;
        else if (lineExtract == Step.stepType.animation         .ToString().Trim().ToLower() ) return Step.stepType.animation           ;
        else if (lineExtract == Step.stepType.changeface        .ToString().Trim().ToLower() ) return Step.stepType.changeface          ;
        else if (lineExtract == Step.stepType.choice            .ToString().Trim().ToLower() ) return Step.stepType.choice              ;
        else
        {
            Debug.LogError("!!!Not a valid line type : " + lineExtract);
            return Step.stepType.dialog;
        }                                                                                
    }

    public static itemID GetItemIDFromString(string lineExtract)
    {
        foreach (int value in System.Enum.GetValues(typeof(itemID)))
        {
            itemID id = (itemID)value;
            string nameString = id.ToString().Trim().ToLower();
            
            if (lineExtract.Trim().ToLower() == nameString)
            {
                return id;
            }
        }
        Debug.LogError("ERROR : " + lineExtract + " is not an item registred.");
        return itemID.none;
    }

    public static pnj.pnjID GetPnjIdFromString(string lineExtract)
    {
        foreach (int value in System.Enum.GetValues(typeof(pnj.pnjID)))
        {
            pnj.pnjID id = (pnj.pnjID)value;
            string nameString = id.ToString().Trim().ToLower();

            if (lineExtract.Trim().ToLower() == nameString)
            {
                return id;
            }
        }
        Debug.LogError("ERROR : " + lineExtract + " is not a pnj registred.");
        return pnj.pnjID.None;
    }

    public static Dialog GetDialogFromString(string dialogName)
    {
        dialogName = dialogName.Trim();

        //Ok, comment on fait ?
        //on suis le nom du "dialogName" pour voir si y a un nom en particulier à suivre ???
        //où on fouille tout ?
        //... compliqué !
#if UNITY_EDITOR
        string largestPath = "Assets/Data/Dialog/";

        foreach(var filePath in Directory.EnumerateFiles(largestPath))
        {
            string fileName = filePath.Remove(0, largestPath.Length).Trim();
            Debug.Log("Seek (" + dialogName + ") : " + fileName);
            if(fileName == dialogName)
            {
                return (Dialog)UnityEditor.AssetDatabase.LoadAssetAtPath(filePath, typeof(Dialog));
            }
        }
#else
        Debug.LogError("GetDialogFromString() should not be call on runtime.");
#endif
        Debug.LogError("Cannot find any dialog named "+ dialogName);
        return null;
    }
    #endregion

}
