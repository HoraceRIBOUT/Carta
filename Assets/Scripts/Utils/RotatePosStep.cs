using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePosStep : MonoBehaviour
{
    public List<GameObject> allItem = new List<GameObject>();

    public Vector3 posOffset = Vector3.zero;
    public Vector3 rotOffset = Vector3.zero;
    public Vector3 sizOffset = Vector3.zero;

    [Sirenix.OdinInspector.Button()]
    void Reposition()
    {
        Vector3 startPos = allItem[0].transform.localPosition;
        Vector3 startRot = allItem[0].transform.localRotation.eulerAngles;
        Vector3 startSiz = allItem[0].transform.localScale;

        for (int i = 0; i < allItem.Count; i++)
        {
            GameObject gO = allItem[i];
            gO.transform.localPosition = startPos + posOffset * i;
            gO.transform.localRotation = Quaternion.Euler(startRot + rotOffset * i);
            gO.transform.localScale = startSiz + sizOffset * i;
        }
    }
}
