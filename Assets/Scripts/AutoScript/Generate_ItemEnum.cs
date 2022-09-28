using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class Generate_ItemEnum : MonoBehaviour
{
    public static string DETECT_NEW_ITEM = "It_";
    public static string DETECT_PATH = "Assets/Data/Item/";

    public static string NEW_SCRIPT_PATH = "Assets/Scripts/Item/";
    public static string NEW_SCRIPT_NAME = "ItemEnum";

    [MenuItem("OrangeLetter/Generate/ Item Enum")]
    public static void ItemEnumGeneration()
    {
        ReadTheItemList();
    }

    public static void ReadTheItemList()
    {
        List<string> importantLine = new List<string>();
        foreach (string fileName in Directory.GetFiles(DETECT_PATH))
        {
            if (fileName.EndsWith(".meta"))
                continue;
            string betterFileName = fileName.Replace(DETECT_PATH, "");
            Debug.Log("Detect : " + betterFileName);
            if (betterFileName.StartsWith(DETECT_NEW_ITEM))
            {
                importantLine.Add(betterFileName.Replace(DETECT_NEW_ITEM, "").Replace(".asset", ""));
            }
        }


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
            /*outfile.WriteLine("using UnityEngine;");
            outfile.WriteLine("");
            outfile.WriteLine("public class " + NEW_SCRIPT_NAME + "");
            outfile.WriteLine("{");*/

            //Write all enum (lowercase)
            outfile.WriteLine("public enum itemID");
            outfile.WriteLine("{");
            outfile.WriteLine("     none = 0,");
            foreach (string str in importantLine)
            {
                string finalEnumName = str.ToLower();
                outfile.WriteLine("     " + finalEnumName + ",");
            }
            outfile.WriteLine("}");
        }

        //end of writing


        AssetDatabase.Refresh();
    }
}
#endif
