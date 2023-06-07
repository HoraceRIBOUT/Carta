using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PlaceBetweenTwoPoint : MonoBehaviour
{

    public Transform target1;
    public Transform target2;
    public Vector3 rotationOffset;

    public float scaleReducer = 1;
    public float xyScale = 0.05f;


    // Update is called once per frame
    void Update()
    {
        if (target1 == null || target2 == null)
            return;

        this.transform.position = (target1.position + target2.position) / 2f;
        this.transform.LookAt(target1);
        this.transform.Rotate(rotationOffset);
        this.transform.localScale = new Vector3(xyScale, (target1.position + target2.position).magnitude * scaleReducer, xyScale);

    }
}
