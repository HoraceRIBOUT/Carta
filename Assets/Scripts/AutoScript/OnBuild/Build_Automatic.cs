using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif

public class Build_Automatic
#if UNITY_EDITOR
    : IPreprocessBuildWithReport, IPostprocessBuildWithReport
#endif
{
#if UNITY_EDITOR
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {

    }

    public void OnPostprocessBuild(BuildReport report)
    {
        //Get the data back on them ?
    }
#endif
}
