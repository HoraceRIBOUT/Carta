using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterAndTrash : MonoBehaviour
{
    //may add a dial if certain index is reached
    public List<Transform> getBackPoint = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //make the shader movement (or not if trash)

    }

    public void OnCollisionEnter(Collision collision)
    {
        PlayerMove play = collision.gameObject.GetComponent<PlayerMove>();
        if (play != null) 
        {
            Vector3 getBackPos = Vector3.up * 5f; //failsafe
            float minDist = 10000f;
            foreach(Transform backPoint in getBackPoint)
            {
                float distance = (backPoint.position - play.transform.position).magnitude;
                if (minDist > distance)
                {
                    getBackPos = backPoint.position;
                    minDist = distance;
                }
            }

            play.transform.position = getBackPos;
            play._rgbd.velocity = Vector3.zero;
            //camera orientation : point to where the transform is rotate ?
            //and 
            //To a specific point.
        }
    }


}
