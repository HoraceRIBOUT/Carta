using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class ScreenshotHD : MonoBehaviour
{
    public int size = 1;
    public string fileName = "FirstTry.png";
    public string path = "../Screenshot/";

    [Sirenix.OdinInspector.Button]
    void UpdateName()
    {
        if (File.Exists(path+fileName))
        {
            fileName = fileName.Substring(0, fileName.Length - 4);
            fileName += "01";
        }
    }
    [Sirenix.OdinInspector.Button]
    void Screenshot()
    {
        UpdateName();
        ScreenCapture.CaptureScreenshot(fileName, size);
    }
}
