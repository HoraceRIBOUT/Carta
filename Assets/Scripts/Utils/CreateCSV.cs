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
        NewDial,
        ZoneInteraction,
        FirstDialog,
        DefaultShow,
        WaitGive,
        DefaultGive,
    }

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
                if (dialog.name.EndsWith(       "_DefaultShowReaction"))
                    dialogDefineLine = (DialogType.DefaultShow      ).ToString();

                else if (dialog.name.EndsWith(  "_DefaultGiveReaction"))
                    dialogDefineLine = (DialogType.DefaultGive      ).ToString();

                else if (dialog.name.EndsWith(  "_GiveReaction"))
                    dialogDefineLine = (DialogType.WaitGive         ).ToString();

                else if (dialog.name.EndsWith(  "_DefaultDialog"))
                    dialogDefineLine = (DialogType.FirstDialog      ).ToString();

                else if (dialog.name.EndsWith(  "_ZoneInteraction"))
                    dialogDefineLine = (DialogType.ZoneInteraction  ).ToString();

                else
                    dialogDefineLine = (DialogType.NewDial          ).ToString();

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




    [MenuItem("OrangeLetter/Generate/Dialog From CSV")]
    public static void LoadCSVToDialog()
    {
        Debug.Log("Selected: " + Selection.activeObject.name);
        TextAsset textAss = (TextAsset)Selection.activeObject;
        if (textAss == null)
            return;

        string wholeText = textAss.text;
        string[] splitedText = wholeText.Split("\n");
        Debug.Log("Load " + splitedText.Length + "lines. The text load is : \n" + wholeText);

        foreach (string textLine in splitedText)
        {
            DialogType dialogLineType = LineTypeOfDialog(textLine);
            if (dialogLineType != DialogType.None)
            {
                string pnjName = textLine.Remove(0, dialogLineType.ToString().Length);

                Debug.Log("New Document : " + dialogLineType + " with : " + pnjName);
            }
            else
            {
                //it's a line for the same file.
                //Debug.Log("Line from doc");
            }
        }

    }

    public static DialogType LineTypeOfDialog(string textLine)
    {
             if (textLine.StartsWith(DialogType.DefaultShow     .ToString())) return DialogType.DefaultShow    ;
        else if (textLine.StartsWith(DialogType.DefaultGive     .ToString())) return DialogType.DefaultGive    ;
        else if (textLine.StartsWith(DialogType.WaitGive        .ToString())) return DialogType.WaitGive       ;
        else if (textLine.StartsWith(DialogType.FirstDialog     .ToString())) return DialogType.FirstDialog    ;
        else if (textLine.StartsWith(DialogType.ZoneInteraction .ToString())) return DialogType.ZoneInteraction;
        else if (textLine.StartsWith(DialogType.NewDial         .ToString())) return DialogType.NewDial        ;
        else                                                                  return DialogType.None           ;
    }


}
