using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

public class HouseBlock : MonoBehaviour
{
#if UNITY_EDITOR
    public Vector3 startingPos;
    [ReadOnly]
    public bool startPos_alreadySet = false;

    public Vector3 randomOffset_Min = new Vector3();
    public Vector3 randomOffset_Max = new Vector3();

    public string lastTimeOnEarth = "";

    [Button]
    public void RePosition()
    {
        Undo.RegisterCompleteObjectUndo(this, "Move game object via Shortcut");
        if (!startPos_alreadySet)
        {
            startingPos = this.transform.position;
            startPos_alreadySet = true;
        }
        float x_off = Random.Range(randomOffset_Min.x, randomOffset_Max.x);
        float y_off = Random.Range(randomOffset_Min.y, randomOffset_Max.y);
        float z_off = Random.Range(randomOffset_Min.z, randomOffset_Max.z);

        Vector3 newPos = startingPos;
        newPos.x += x_off;
        newPos.y += y_off;
        newPos.z += z_off;
        this.transform.position = newPos;

        int randomRot = Random.Range(0, 16);
        this.transform.rotation = Quaternion.Euler(0, randomRot * 90, 0);
    }
    [Button]
    public void ResetPosition()
    {
        if (startPos_alreadySet)
        {
            this.transform.position = startingPos;
        }
    }

    [Button]
    public void ResetFirstPosition()
    {
        startPos_alreadySet = false;
    }
#endif
}
