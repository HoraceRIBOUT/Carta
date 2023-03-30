using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThird : MonoBehaviour
{
    //Let's try to compensiate the escalier problem 

    //ok, how we do it? 

    public Rigidbody _rgbd;
    public float speed = 10;


    [Header("Jump")]
    public float jumpForce = 10f;
    public float verticalBonusForHorizontalJump = 0.3f;
    public bool canJump = true;


    private void JumpManagement()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            if (canJump)
            {
                _rgbd.velocity = PlayerMove.HorizontalOnly(_rgbd.velocity);
                Vector3 lastSpeed = _rgbd.velocity;
                Vector3 jumpDirection = Vector3.up;
                //Will have to "incline" the jump toward : the koystock direction + the normal of the ground
                _rgbd.AddForce(jumpForce * jumpDirection, ForceMode.Impulse);
                canJump = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        MovementManagement();
        JumpManagement();
    }
    void MovementManagement()
    {
        Vector2 inputDir = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
            );

        Vector3 forwardCam = GameManager.instance.cameraMng.mainCamera.transform.forward;
        Vector3 rightCam = GameManager.instance.cameraMng.mainCamera.transform.right;

        forwardCam = Vector3.ProjectOnPlane(forwardCam, Vector3.up).normalized;
        rightCam = Vector3.ProjectOnPlane(rightCam, Vector3.up).normalized;

        Vector3 acceleration = inputDir.x * rightCam + inputDir.y * forwardCam;
        acceleration *= speed;

        _rgbd.velocity += acceleration * Time.deltaTime;
        Debug.DrawRay(this.transform.position, _rgbd.velocity, Color.blue);
        Debug.DrawRay(this.transform.position + _rgbd.velocity, acceleration, Color.green);
    }






    [Header("Collision and so")]
    public List<int> layerOnCollision;
    //public float offset = 0.3f;
    //public float size = 0.2f;
    public LayerMask layerMask;
    public float raycastDist = 0.15f;
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.contactCount > 0)
        {
            Vector3 impactNormal = collision.contacts[0].normal;
            //if (Vector3.Dot(Vector3.down, impactNormal) <= 0)//So, we can move in the plafond
            {

                //Reset Jump
                if (Vector3.Dot(Vector3.down, impactNormal) <= 0)
                    canJump = true;
            }
        }

        //if object is part of decor 
        //if normal of collision is not too sharp

    }
}
