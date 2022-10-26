using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnToTarget : MonoBehaviour
{
    public Transform target;
    public bool playerIsTarget = false;
    public bool cameraIsTarget = false;

    public Vector3 rotOffset = Vector3.zero;

    public bool onlyXY = false;

    public void Start()
    {
        if (playerIsTarget)
        {
            target = GameManager.instance.playerMove.transform;
        }
        if (cameraIsTarget)
        {
            target = GameManager.instance.cameraMng.mainCamera.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.LookAt(target, Vector3.up);
        this.transform.Rotate(rotOffset);

        if (onlyXY)
        {
            Vector3 euler = this.transform.rotation.eulerAngles;
            euler.z = 0;
            this.transform.rotation = Quaternion.Euler(euler);
        }
    }
}
