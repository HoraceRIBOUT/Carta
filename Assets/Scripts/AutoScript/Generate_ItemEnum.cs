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

        //
        //Ok, gather the number for each and then, add new number
        int maxValue = -1;
        foreach(int value in System.Enum.GetValues(typeof(itemID)))
        {
            string nameString = ((itemID)value).ToString();
            for (int i = 0; i < importantLine.Count; i++)
            {
                string str = importantLine[i];
                if (nameString.ToLower() == str.ToLower())
                {
                    importantLine[i] = importantLine[i] + " = " + (value);
                    break;
                }
            }
            if (maxValue < value)
            {
                maxValue = value;
            }
        }

        for (int i = 0; i < importantLine.Count; i++)
        {
            string str = importantLine[i];
            Debug.Log(i + " : " + str + "("+ maxValue +")");
            if (!str.Contains("="))
            {
                maxValue++;
                importantLine[i] = importantLine[i] + " = " + (maxValue);
                break;
            }
        }




        //writing
        using (StreamWriter outfile =
            new StreamWriter(copyPath))
        {
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
