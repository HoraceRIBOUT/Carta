using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cl_", menuName = "Carta/Clue")]
public class Clue : ScriptableObject
{
    public bool isMet = false;
    public List<LinePos> linePoses = new List<LinePos>();

    public void Test()
    {
        if (isMet)
            return;

        foreach (var pos in linePoses)
        {
            if (pos.dialog != null)
            {
                if (pos.dialog.allSteps != null && pos.dialog.allSteps.Count >= pos.stepNumber)
                {
                    if (pos.dialog.allSteps[pos.stepNumber].alreadyRead)
                    {
                        isMet = true;
                        return;
                    }
                }
                else
                    Debug.LogError("dialog don't have steps (or enough step) : we need " + pos.stepNumber + " steps.");
            }
            else
                Debug.LogError("dialog is not set.");
        }
    }

    [System.Serializable]
    public struct LinePos
    {
        public Dialog dialog;
        public int stepNumber;
    }
}