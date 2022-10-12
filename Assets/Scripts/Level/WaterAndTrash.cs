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
        Debug.LogWarning("Oh !");
        PlayerMove play = collision.gameObject.GetComponent<PlayerMove>();
        if (play != null) 
        {
            Debug.Log("Get out");
            play.transform.position = Vector3.up * 5f;
        }
    }


}
