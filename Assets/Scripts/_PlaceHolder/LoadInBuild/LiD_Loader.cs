using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LiD_Loader : MonoBehaviour
{
    public LiD_Effecter effecter;
    //This will be in the same folder as the .exe
    public string path = "./LiD_testTest.txt";

    public void Start()
    {
        if(effecter == null)
        {
            effecter = this.GetComponent<LiD_Effecter>();
            effecter.loader = this;
        }

        CreateBaseFilesIfNotExistent();
    }

    public void Load()
    {
        if (!File.Exists(path))
        {
            Debug.LogError(path + " file have been deleted. Please, recreate it.");
            return;
        }


        StreamReader outfile = new StreamReader(path);
        string allValue = outfile.ReadToEnd();
        string[] allValueArray = allValue.Split('\n');

        //A bit of repetition, but it's for a jam
        for (int i = 0; i < allValueArray.Length; i++)
        {
            if(allValueArray[i].Trim().ToLower() == "JumpForce".ToLower())
               if( float.TryParse(allValueArray[i + 1], out float res))
                    effecter.jumpForce = res;

            if (allValueArray[i].Trim().ToLower() == "Strenght".ToLower())
                if (float.TryParse(allValueArray[i + 1], out float res))
                    effecter.strenght = res;

            if (allValueArray[i].Trim().ToLower() == "speed".ToLower())
                if (float.TryParse(allValueArray[i + 1], out float res))
                    effecter.speed = res;
        }

        outfile.Close();
    }



    public void CreateBaseFilesIfNotExistent()
    {

        if (!File.Exists(path))
        {
            Debug.Log("Creation of the files : " + path);
            effecter.text.SetText("Creation of the files: " + path);

            //writing
            using (StreamWriter outfile =
                new StreamWriter(path))
            {
                outfile.WriteLine("JumpForce");
                outfile.WriteLine("2");
                outfile.WriteLine("Strenght");
                outfile.WriteLine("6.3");
                outfile.WriteLine("speed");
                outfile.WriteLine("2");

                outfile.Close();
            }
            //end of writing
        }
        else
        {
            effecter.text.SetText("Exist somewhere ! ");
        }

    }
}
