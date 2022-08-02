using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Mouvement")]
    [Tooltip("The higher the quickier we reach full speed")]
    public float groundGain = 1f;
    [Tooltip("Max horizontal speed")]
    public float groundSpeed = 1f;
    [Tooltip("The higher the quickier we stop")]
    [Range(0, 3)]
    public float drag = 2;

    [Header("Jump")]
    public float jumpForce = 10f;
    public bool canJump = true;

    public Vector3 lastSpeed = Vector3.zero;
    public Vector3 acceleration = Vector3.zero;

    public Rigidbody _rgbd;


    // Start is called before the first frame update
    void Start()
    {
        _rgbd.drag = drag;
    }

    // Update is called once per frame
    void Update()
    {
        //Both jump and movement
        MovementManagement();
    }

    private void MovementManagement()
    {
        Vector2 inputDirection = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
            );

#if UNITY_EDITOR
        _rgbd.drag = drag;
#endif 
        lastSpeed = _rgbd.velocity;
        if (inputDirection != Vector2.zero)
        {
            Vector3 forwardDir = Vector3.forward;
            Vector3 rightDir = Vector3.right;

            acceleration = inputDirection.x * rightDir;
            acceleration += inputDirection.y * forwardDir;
            acceleration = groundGain * inputDirection.magnitude * acceleration.normalized;

            lastSpeed += acceleration * Time.deltaTime;

            if(HorizontalMagnitude(lastSpeed) > groundSpeed)
            {
                lastSpeed = HorizontalClamp(lastSpeed, groundSpeed);
            }
        }

        //will also handle wall sticking

        _rgbd.velocity = lastSpeed;

        JumpManagement();
    }

    private void JumpManagement()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            if (canJump)
            {
                _rgbd.velocity = HorizontalOnly(_rgbd.velocity);
                //Will have to "incline" the jump toward : the koystock direction + the normal of the ground
                _rgbd.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
                canJump = false;
            }
        }
    }


    #region Collision and so

    public void OnCollisionEnter(Collision collision)
    {
        //if object is part of decor 
        //if normal of collision is not too sharp
        if(collision.contactCount > 0)
        {
            if(Vector3.Dot(Vector3.down, collision.contacts[0].normal) <= 0)
                canJump = true;
        }
    }
    public void OnCollisionExit(Collision collision)
    {
    }


    #endregion





    public static Vector3 HorizontalOnly(Vector3 vec)
    {
        vec.z = 0;
        return vec;
    }
    public static float HorizontalMagnitude(Vector3 vec)
    {
        vec.y = 0;
        return vec.magnitude;
    }
    public static Vector3 HorizontalClamp(Vector3 vec, float lenghtMax)
    {
        float yMem = vec.y;
        vec.y = 0;
        vec = vec.normalized * lenghtMax;
        vec.y = yMem;
        return vec;
    }

}
