using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DialogTools : MonoBehaviour
{
    public TextAsset csvToLoad;

#if UNITY_EDITOR
    [Sirenix.OdinInspector.Button()]
    public void LoadCSV()
    {

    }









    public static string[,] LoadDocument(string docPath, string docName)
    {
        string allValue;

        string copyPath = docPath + docName;
        if (copyPath.Contains("Resources/"))
        {
            string[] separator = { "Resources/" };
            copyPath = copyPath.Split(separator, StringSplitOptions.None)[1];
            //            Debug.Log("Copy path : "+copyPath);
            TextAsset obj = Resources.Load(copyPath) as TextAsset;
            if (obj == null)
            {
                Debug.LogError("Cannot find the text : " + copyPath + " in the Assets/Resources folder");
                string[,] res = new string[0, 0];
                return res;
            }
            allValue = obj.text;
        }
        else
        {
            if (!Directory.Exists(docPath))
            {
                Directory.CreateDirectory(docPath);
            }
            if (!File.Exists(docPath + docName))
            {
                Debug.LogError("Creation of the files : " + docPath + docName);
                CreateBasicFile(copyPath);
            }

            StreamReader outfile = new StreamReader(copyPath);
            allValue = outfile.ReadToEnd();
        }


        string[,] gridOutput = SplitCsvGrid(allValue);

        DebugOutputGrid(gridOutput);

        return gridOutput;
    }

    static void CreateBasicFile(string copyPath)
    {
        string currentDocumentData = "";
        //preparing
        currentDocumentData += "Id" + "\t";
        currentDocumentData += "Commentaire" + "\t";
        
        //writing
        using (StreamWriter outfile =
            new StreamWriter(copyPath))
        {
            foreach (string s in currentDocumentData.Split('|'))
            {
                outfile.WriteLine(s);
            }
        }
        //end of writing
    }

    // outputs the content of a 2D array, useful for checking the importer
    static public void DebugOutputGrid(string[,] grid)
    {
        /*string textOutput = "";
        for (int y = 0; y < grid.GetUpperBound(1); y++)
        {
            for (int x = 0; x < grid.GetUpperBound(0) - 1; x++)
            {
                textOutput += grid[x, y];
                textOutput += "|";
            }
            textOutput += "\n";
        }
        Debug.Log(textOutput);*/
    }

    // splits a CSV file into a 2D string array
    static public string[,] SplitCsvGrid(string csvText)
    {
        string[] lines = csvText.Split("\n"[0]);

        // finds the max width of row
        int width = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            string[] row = SplitCsvLine(lines[i]);
            width = Mathf.Max(width, row.Length);
        }

        // creates new 2D string grid to output to
        string[,] outputGrid = new string[width + 1, lines.Length + 1];
        for (int y = 0; y < lines.Length; y++)
        {
            string[] row = SplitCsvLine(lines[y]);
            for (int x = 0; x < row.Length; x++)
            {
                outputGrid[x, y] = row[x];

                // This line was to replace "" with " in my output. 
                // Include or edit it as you wish.
                outputGrid[x, y] = outputGrid[x, y].Replace("\"\"", "\"");
            }
        }

        return outputGrid;
    }

    // splits a CSV row 
    static public string[] SplitCsvLine(string line)
    {
        return (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line,
        @"(((?<x>(?=[\t\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^\t\r\n]+))\t?)",
        System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
                select m.Groups[1].Value).ToArray();
    }

#endif
}
