using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThird : MonoBehaviour
{
    //Let's try to compensiate the escalier problem 

    //ok, how we do it? 

    public Rigidbody _rgbd;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MovementManagement();
    }
    void MovementManagement()
    {
        Vector2 inputDir = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
            );

        Vector3 acceleration = inputDir.x * Vector3.right + inputDir.y * Vector3.forward;

        _rgbd.velocity += acceleration * Time.deltaTime;
        Debug.DrawRay(this.transform.position, _rgbd.velocity, Color.blue);
        Debug.DrawRay(this.transform.position + _rgbd.velocity, acceleration, Color.green);
    }
}
