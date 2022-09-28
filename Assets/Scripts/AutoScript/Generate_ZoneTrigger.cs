using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class Generate_ZoneTrigger : MonoBehaviour
{
    public static string DETECT_ENTER = "public void Enter";
    public static string DETECT_EXIT = "public void Exit";
    public static string NEW_SCRIPT_PATH = "Assets/Scripts/AutoScript/";
    public static string SOURCE_SCRIPT_NAME = "ZoneTrigger";
    public static string NEW_SCRIPT_NAME = "ZoneTrigger_AutoGeneratePart";


    [MenuItem("OrangeLetter/Generate/Zone Trigger Method")]
    public static void ZoneTriggerGeneration()
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
        List<string> importantLine_Enter = new List<string>();
        List<string> importantLine_Exit = new List<string>();
        foreach (string s in eachLineOfTheFiles)
        {
            if (s.Trim() == "")
                continue;
            if (s.Trim().StartsWith(DETECT_ENTER))
            {
                string res = s.Trim().Replace(DETECT_ENTER, "");
                res = res.Replace("()", "");
                if (!importantLine.Contains(res))
                    importantLine.Add(res);
                importantLine_Enter.Add(res);
            }
            if (s.Trim().StartsWith(DETECT_EXIT))
            {
                string res = s.Trim().Replace(DETECT_EXIT, "");
                res = res.Replace("()", "");
                if(!importantLine.Contains(res))
                    importantLine.Add(res);
                importantLine_Exit.Add(res);
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
            outfile.WriteLine("public class " + NEW_SCRIPT_NAME + " : MonoBehaviour {");
            outfile.WriteLine(" ");
            outfile.WriteLine("\tprivate ZoneTrigger mainComponent;");
            outfile.WriteLine("\t");
            outfile.WriteLine("\tpublic void Start()");
            outfile.WriteLine("\t{");
            outfile.WriteLine("\t    mainComponent = this.GetComponent<ZoneTrigger>();");
            outfile.WriteLine("\t}");
            outfile.WriteLine(" ");
            //Write all enum (lowercase)
            outfile.WriteLine("\tpublic enum ZoneTriggerType");
            outfile.WriteLine("\t{");
            foreach (string str in importantLine)
            {
                string finalEnumName = str;
                finalEnumName = finalEnumName.Replace(" ", "_");
                outfile.WriteLine("\t     " + finalEnumName.ToLower() + ",");
            }
            outfile.WriteLine("\t}");
            outfile.WriteLine(" ");
            //Enter part
            outfile.WriteLine("\tpublic void OnTriggerEnter(Collider collision)");
            outfile.WriteLine("\t{");
            outfile.WriteLine("\t    switch(mainComponent.myType)");
            outfile.WriteLine("\t    {");
            foreach (string str in importantLine_Enter)
            {
                string finalEnumName = str;
                finalEnumName = finalEnumName.Replace(" ", "_");
                outfile.WriteLine("\t        case ZoneTriggerType." + finalEnumName.ToLower() + " :");
                outfile.WriteLine("\t            mainComponent.Enter" + finalEnumName + "();");
                outfile.WriteLine("\t        break;");
            }
            outfile.WriteLine("\t    }");
            outfile.WriteLine("\t}");
            outfile.WriteLine(" ");
            //Exit part
            outfile.WriteLine("\tpublic void OnTriggerExit(Collider collision)");
            outfile.WriteLine("\t{");
            outfile.WriteLine("\t    switch(mainComponent.myType)");
            outfile.WriteLine("\t    {");
            foreach (string str in importantLine_Exit)
            {
                string finalEnumName = str;
                finalEnumName = finalEnumName.Replace(" ", "_");
                outfile.WriteLine("\t        case ZoneTriggerType." + finalEnumName.ToLower() + " :");
                outfile.WriteLine("\t            mainComponent.Exit" + finalEnumName + "();");
                outfile.WriteLine("\t        break;");
            }
            outfile.WriteLine("\t    }");
            outfile.WriteLine("\t}");
            outfile.WriteLine(" ");
            //Finish the class
            outfile.WriteLine("   ");
            outfile.WriteLine("   ");
            outfile.WriteLine("}");
        }

        //end of writing


        AssetDatabase.Refresh();
    }
}
#endif
