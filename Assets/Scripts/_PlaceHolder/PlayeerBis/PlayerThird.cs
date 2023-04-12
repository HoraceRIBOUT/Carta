using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThird : MonoBehaviour
{
    //Let's try to compensiate the escalier problem 

    //ok, how we do it? 

    public Rigidbody _rgbd;
    public float speed = 10;
    private Vector3 lastSpeed;


    [Header("Jump")]
    public float jumpForce = 10f;
    public float verticalBonusForHorizontalJump = 0.3f;
    public bool canJump = true;

    public Vector3 lastUpVector = Vector3.up;

    public List<PlayerMove.wallAndGround_Info> contacts = new List<PlayerMove.wallAndGround_Info>();

    //[Range(0,1)]
    public float debugValue = 0;

    private void JumpManagement()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            if (canJump)
            {
                _rgbd.velocity = PlayerMove.HorizontalOnly(_rgbd.velocity);
                Vector3 jumpDirection = lastUpVector;
                //Will have to "incline" the jump toward : the koystock direction + the normal of the ground
                _rgbd.AddForce(jumpForce * jumpDirection, ForceMode.Impulse);
                canJump = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateContact();
        MovementManagement();
        JumpManagement();

        GravityManagement();
    }
    void MovementManagement()
    {
        Vector2 inputDir = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
            );

        Vector3 forwardCam = GameManager.instance.cameraMng.mainCamera.transform.forward;
        Vector3 rightCam = GameManager.instance.cameraMng.mainCamera.transform.right;
        Vector3 upVector = lastUpVector;

        //Decide which UpVector we are gonna use
        if (contacts.Count == 0)
        {
            //Normally, coyote time here
            upVector = Vector3.up;
        }
        else if (contacts.Count == 1)
        {
            upVector = contacts[0].lastNormal;
        }
        else
        {
            //ok, so, for now, just take the vector who is touch by joyLook.
            Vector3 direction = forwardCam * inputDir.y + rightCam * inputDir.x;
            upVector = SetVectorUpForMultipleElement(direction);
            Debug.DrawRay(this.transform.position, upVector, Color.red + Color.blue);
        }

        

        Vector3 groundForward = Vector3.ProjectOnPlane(forwardCam, upVector).normalized;
        //this is the ground forward 
        Vector3 wallForward = ConvertForwardToWall(upVector, forwardCam);
        //Change forward if we are on a wall :
        float clampedDotValue = Mathf.Clamp01(Vector3.Dot(upVector, Vector3.up));

        //debugValue = clampedDotValue;
        //Debug.DrawRay(this.transform.position, groundForward, Color.white);
        forwardCam = Vector3.Lerp(wallForward, groundForward, clampedDotValue);
        rightCam = Vector3.Cross(upVector, forwardCam).normalized;

        //
        lastUpVector = upVector;

        Vector3 acceleration = inputDir.x * rightCam + inputDir.y * forwardCam;
        acceleration *= speed;

        if(!Input.GetKey(KeyCode.LeftShift))
            _rgbd.velocity += acceleration * Time.deltaTime;
        

        Debug.DrawRay(this.transform.position, _rgbd.velocity, Color.blue);
        Debug.DrawRay(this.transform.position + _rgbd.velocity, acceleration, Color.green);
        //Debug.DrawRay(this.transform.position, forwardCam, Color.yellow);
        //Debug.DrawRay(this.transform.position, rightCam, Color.yellow / 2f);
        lastSpeed = _rgbd.velocity;
    }

    public Vector3 SetVectorUpForMultipleElement(Vector3 direction)
    {
        Debug.DrawRay(this.transform.position, direction.normalized * maxDist, Color.red);
        if (Physics.Raycast(this.transform.position, direction, out RaycastHit raycastInfo, maxDist, layerMaskContact))
        {
            if (AlreadyInContact(raycastInfo.collider.gameObject))
            {
                //normallly, we goes directly by "'GetContact" then test if it's null or not, but get some bug where the simple "null test" return an error
                PlayerMove.wallAndGround_Info contactInfo = GetContact(raycastInfo.collider.gameObject);
                return contactInfo.lastNormal;
            }
            //else, it is as if you touch nothing.
        }
        //if no vector touch, use the most vertical one ! (and if equality, try the Dot with the joylook ???)
        Vector3 upVector = lastUpVector;
        float maxDot = -1;
        foreach (PlayerMove.wallAndGround_Info info in contacts)
        {
            float newDot = Vector3.Dot(info.lastNormal, Vector3.up);
            if (maxDot < newDot)
            {
                maxDot = newDot;
                upVector = info.lastNormal;
            }
            else if (maxDot == newDot)
            {
                //Mean ?
                upVector = (info.lastNormal + upVector) / 2;//bad way to do it, but really few case where we touch more than 2 surface who are exactly the same dot
            }
        }
        return upVector;
    }

    public Vector3 ConvertForwardToWall(Vector3 wallNormal, Vector3 forwardCam)
    {
        Vector3 res = Vector3.up;
        ///Ok, so , we need to calculate the difference between the -wallNormal ET le current forwardCam
        Vector3 flatenedWallNorm = Vector3.ProjectOnPlane(-wallNormal, Vector3.up).normalized;
        Vector3 flatenedForwardV = Vector3.ProjectOnPlane(forwardCam, Vector3.up).normalized;
        float angle = Vector3.Angle(flatenedWallNorm, flatenedForwardV);
        Debug.DrawRay(this.transform.position, flatenedWallNorm, Color.cyan);
        Debug.DrawRay(this.transform.position, flatenedForwardV, Color.magenta);
        angle *= Mathf.Sign(Vector3.Dot(Vector3.Cross(Vector3.up, flatenedWallNorm), flatenedForwardV));
        ///Ca va nous donner un angle.
        ///Cette angle, on va l'appliquer au UPVECTOR noirmalisé sur le mur 
        Vector3 wallUpVector = Vector3.ProjectOnPlane(Vector3.up, wallNormal).normalized;
        Debug.DrawRay(this.transform.position, wallUpVector, Color.grey);
                    //if wallNormal is Vector3.up, it should be nullify in the global code

        //// autour de l'axe wallNormal !
        Quaternion rotation = Quaternion.AngleAxis(angle, wallNormal); // - wallNormal ????
        debugValue = angle;
        ////// et ça nous donneras le forward  actuel. 
        res = rotation * wallUpVector;
        // tu le cross avec le wallNormal, tu obtiens la droite.
        //gg 

        Debug.DrawRay(this.transform.position, res, Color.black);
        return res;
    }

    public float maxDist = 0.5f;
    public LayerMask layerMaskContact;
    void UpdateContact()
    {
        foreach (PlayerMove.wallAndGround_Info info in contacts)
        {
            Vector3 direction = -info.lastNormal;
            //Debug.DrawRay(this.transform.position, direction.normalized * maxDist, Color.red);
            if (Physics.Raycast(this.transform.position, direction, out RaycastHit raycastInfo, maxDist, layerMaskContact))
            {
                if (raycastInfo.collider.gameObject.GetInstanceID() == info.id)
                {
                    info.lastNormal = raycastInfo.normal;
                }
            }
            //Debug.DrawRay(info.gO.transform.position, info.lastNormal, Color.green);
        }
    }

    public bool AlreadyInContact(GameObject gO)
    {
        foreach (PlayerMove.wallAndGround_Info info in contacts)
        {
            if (info.id == gO.GetInstanceID())
            {
                return true;
            }
        }
        return false;
    }
    public PlayerMove.wallAndGround_Info GetContact(GameObject gO)
    {
        foreach (PlayerMove.wallAndGround_Info info in contacts)
        {
            if (info.id == gO.GetInstanceID())
            {
                return info;
            }
        }
        return null;
    }

    public float gravityIntensity = 9.81f;
    public float wallGravity = 9.81f;
    public float gravityIntensity_FallAdd = 9.81f;
    public void GravityManagement()
    {
        float dotClamped = Mathf.Clamp01(Vector3.Dot(lastUpVector, Vector3.up));
        float gravitySum = Mathf.Lerp(wallGravity, gravityIntensity, dotClamped);
        if (contacts.Count == 0 && _rgbd.velocity.y < 0)
            gravitySum += gravityIntensity_FallAdd;

        _rgbd.velocity += -lastUpVector * gravitySum * Time.deltaTime;
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
                //if object is part of decor 
                //if normal of collision is not too sharp
                PlayerMove.wallAndGround_Info col = new PlayerMove.wallAndGround_Info(collision.collider.gameObject, impactNormal);
                if(!contacts.Contains(col))
                    contacts.Add(col);
                else
                {
                    Debug.LogError("Alreday have " + collision.collider.gameObject.name);
                }

                //Reset Jump
                if (Vector3.Dot(Vector3.down, impactNormal) <= 0)
                    canJump = true;
            }
        }

    }

    public void OnCollisionExit(Collision collision)
    {
        for (int i = 0; i < contacts.Count; i++)
        {
            PlayerMove.wallAndGround_Info info = contacts[i];
            if (info.id == collision.gameObject.GetInstanceID())
            {
                contacts.Remove(info);
                break;
            }
        }
    }
}
