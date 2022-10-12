using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropagateCollision : MonoBehaviour
{
    public PlayerMove parentToWarn;
    private void OnCollisionEnter(Collision collision)
    {
        parentToWarn.OnCollisionEnter(collision);
    }
    private void OnCollisionExit(Collision collision)
    {
        parentToWarn.OnCollisionExit(collision);
    }
}
