using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionZone : MonoBehaviour
{

    public pnj father;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerMove>() != null)
        {
            father.playerOnReach = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerMove>() != null)
        {
            father.playerOnReach = false;
        }
    }


}
