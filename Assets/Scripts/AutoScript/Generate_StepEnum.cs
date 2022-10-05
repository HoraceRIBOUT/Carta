using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class Generate_StepEnum : MonoBehaviour
{

    public static string DETECT_NEWCLASS = "public class Step_";
    public static string NEW_SCRIPT_PATH = "Assets/Scripts/PNJ/";
    public static string SOURCE_SCRIPT_NAME = "Dialog";

    public static string NEW_SCRIPT_NAME = "Dialog_AutoGeneratePart";


    [MenuItem("OrangeLetter/Generate/Step Enum")]
    public static void StepEnumGeneration()
    {
        ReadTheFile();
    }

    public static void ReadTheFile()
    {
        string readPath = NEW_SCRIPT_PATH + SOURCE_SCRIPT_NAME + ".cs";
        Debug.Log("Reading Classfile: " + readPath);
        List<string> eachLineOfTheFiles = new List<string>();

        using (StreamReader infile =
            new StreamReader(readPath))
        {
            string line;
            while ((line = infile.ReadLine()) != null)
            {
                eachLineOfTheFiles.Add(line);
            }
        }

        //Treat the read lines
        List<string> importantLine = new List<string>();
        foreach (string s in eachLineOfTheFiles)
        {
            if (s.Trim() == "")
                continue;
            if (s.Trim().StartsWith(DETECT_NEWCLASS))
            {
                string res = s.Trim().Replace(DETECT_NEWCLASS, "");
                res = res.Replace(": Step_father", "").Trim();
                if (!importantLine.Contains(res))
                    importantLine.Add(res);
            }
        }


        /*
        Debug.Log("Important number =  " + importantLine.Count);
        foreach (string impS in importantLine)
        {
            Debug.Log("imps : " + impS);
        }
        */



        // remove whitespace and minus
        string name = NEW_SCRIPT_NAME;
        name = name.Replace("-", "_");
        string copyPath = NEW_SCRIPT_PATH + name + ".cs";
        string metaPath = NEW_SCRIPT_PATH + name + ".cs.meta";
        FileUtil.DeleteFileOrDirectory(copyPath);
        FileUtil.DeleteFileOrDirectory(metaPath);
        Debug.Log("Creating Classfile: " + copyPath);

        //could be cool to load from there...

        //writing
        using (StreamWriter outfile =
            new StreamWriter(copyPath))
        {
            //Classic start of a class
            outfile.WriteLine("using UnityEngine;");
            outfile.WriteLine("");
            outfile.WriteLine("public class " + NEW_SCRIPT_NAME + " {");
            outfile.WriteLine(" ");
            outfile.WriteLine("}");
            outfile.WriteLine(" ");
            outfile.WriteLine(" ");
            outfile.WriteLine("namespace Step");
            outfile.WriteLine("{");

            //Write all enum (lowercase)
            outfile.WriteLine("\tpublic enum stepType");
            outfile.WriteLine("\t{");
            foreach (string str in importantLine)
            {
                string finalEnumName = str.ToLower();
                outfile.WriteLine("\t     " + finalEnumName + ",");
            }
            outfile.WriteLine("\t}");
            outfile.WriteLine(" ");

            outfile.WriteLine("\t[System.Serializable]");
            outfile.WriteLine("\tpublic class Step");
            outfile.WriteLine("\t{");
            outfile.WriteLine("\t\t[HideInInspector()]");
            outfile.WriteLine("\t\tpublic int index;");
            outfile.WriteLine("\t\tpublic string title { get { return \"Step \" + index; } }");
            outfile.WriteLine("\t\t[Sirenix.OdinInspector.Title(\"$title\")]");
            outfile.WriteLine("\t\t[Sirenix.OdinInspector.GUIColor(\"GetEnumColor\")]");
            outfile.WriteLine("\t\tpublic stepType type;");
            foreach (string str in importantLine)
            {
                outfile.WriteLine("\t\t[Sirenix.OdinInspector.ShowIf(\"type\", stepType." + str.ToLower() + ")]");
                outfile.WriteLine("\t\tpublic Step_" + str + " " + str.ToLower() + "_Data;");
            }
            outfile.WriteLine("\t\t");
            outfile.WriteLine("\t\tpublic Step_father GetData()");
            outfile.WriteLine("\t\t{");
            outfile.WriteLine("\t\t\tswitch (type)");
            outfile.WriteLine("\t\t\t{");
            foreach (string str in importantLine)
            {
                outfile.WriteLine("\t\t\t\tcase stepType." + str.ToLower() + ":");
                outfile.WriteLine("\t\t\t\treturn " + str.ToLower() + "_Data;");
            }
            outfile.WriteLine("\t\t\t\tdefault:");
            outfile.WriteLine("\t\t\t\tDebug.LogError(type + \" not implemented in Dialog.cs(class Step.Step() )\");");
            outfile.WriteLine("\t\t\t\treturn null;");
            outfile.WriteLine("\t\t\t}");
            outfile.WriteLine("\t\t}");
            outfile.WriteLine("\t\t");
            outfile.WriteLine("\t\tpublic Color GetEnumColor()");
            outfile.WriteLine("\t\t{");
            outfile.WriteLine("\t\t\tSirenix.Utilities.Editor.GUIHelper.RequestRepaint();");
            outfile.WriteLine("\t\t\treturn Color.HSVToRGB((int)type * (1f / System.Enum.GetValues(typeof(stepType)).Length), 0.2f, 1);");
            outfile.WriteLine("\t\t}");
            outfile.WriteLine("\t}");
            outfile.WriteLine("}");
        }

        //end of writing


        AssetDatabase.Refresh();
    }
}
#endif
