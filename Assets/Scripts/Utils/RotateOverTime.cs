using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    public bool on = true;
    public Vector3 rotationPerSecond;

    public float speed = 1;

    // Update is called once per frame
    void Update()
    {
        if (!on)
            return;

        this.transform.Rotate(rotationPerSecond * Time.deltaTime * speed);
    }
}
