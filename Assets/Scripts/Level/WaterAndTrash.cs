using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterAndTrash : MonoBehaviour
{
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
            play.transform.position = Vector3.up * 5f;
            //To a specific point.
        }
    }


}
