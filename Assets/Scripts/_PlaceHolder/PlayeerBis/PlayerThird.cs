using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerThird : MonoBehaviour
{
    [Header("Component")]
    public CapsuleCollider _capsule;
    public Rigidbody _rgbd;
    private Transform cameraTr;

    [Header("Data")]
    public float speedGain = 10;
    public float speedMax = 5;
    public float slowDownMultiplier = 0.2f;
    private Vector3 lastSpeed;


    [Header("Jump")]
    public float jumpForce = 10f;
    public float verticalBonusForHorizontalJump = 0.3f;
    [ReadOnly] public bool canJump = true;

    [Header("Other data")] [ReadOnly]
    public Vector3 lastUpVector = Vector3.up;
    [ReadOnly]
    public List<PlayerMove.wallAndGround_Info> contacts = new List<PlayerMove.wallAndGround_Info>();

    [Header("Corridor redirector")]
    public float emptyLookDistance = 0.2f;
    public List<Vector3> emptyLookRaycastDirections = new List<Vector3> { Vector3.right, Vector3.down, Vector3.left };
    public float emptyLookIntensity = 0.1f;

    public void Start()
    {
        cameraTr = GameManager.instance.cameraMng.falseCamera.transform;
    }

    // Update is called once per frame
    void Update()
    {
        HandleGrappleWallMode();
        CrouchManagement();

        UpdateContact();
        MovementManagement();
        JumpManagement();

        GravityManagement();
    }


    [Header("Grapple mode")]
    public bool grapleMode = true;
    public bool grapleMode_eff = false;
    private bool grapleMode_Mem = false;
    public GameObject grappleVisual_On;
    public GameObject grappleVisual_Off;
    private void HandleGrappleWallMode()
    {
        //switch
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            grapleMode = !grapleMode;
        }

        //hold
        if (Input.GetKey(KeyCode.Mouse0))
        {
            grapleMode_eff = !grapleMode;
        }
        else
        {
            grapleMode_eff = grapleMode;
        }

        if(grapleMode_eff != grapleMode_Mem)
        {
            grappleVisual_On .SetActive( grapleMode_eff);
            grappleVisual_Off.SetActive(!grapleMode_eff);
            grapleMode_Mem = grapleMode_eff;
        }

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


        //Ok, revoir the "grapple mode"
        if (!grapleMode_eff)
        {
            float ySpeed = acceleration.y;
            if (ySpeed > 0)
            {
                //Transform the y part into vertical velocity
                //let's gooo
                acceleration.y = 0;
                //ok let's try by giving the ySpeed into the horizontal speed
                acceleration *= ySpeed;
            }
        }


        //acceleration += RayCastToFindAnythingInFrontOfUs(inputDir);
        acceleration *= speedGain * Time.deltaTime;

        if(!Input.GetKey(KeyCode.LeftShift))
            _rgbd.velocity += acceleration;

        _rgbd.velocity= Clamp_AxisIgnored(_rgbd.velocity, speedMax, upVector);

        //we can now  make the drag here :
        _rgbd.velocity = _rgbd.velocity * Mathf.Pow(slowDownMultiplier, Time.deltaTime);

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
            Debug.Log("Si si je touche là, ouesh !");
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
        Vector3 wallUpVector = GetUpOfTheSurface(wallNormal);
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

    public static Vector3 GetUpOfTheSurface(Vector3 wallNormal)
    {
        return Vector3.ProjectOnPlane(Vector3.up, wallNormal).normalized;
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


    //public Vector3 RayCastToFindAnythingInFrontOfUs(Vector2 inputDir)
    //{
    //    Vector3 ray = cameraTr.forward * inputDir.y + cameraTr.right * inputDir.x; //so, chjeck where the player "look" and indicate with his joystick
    //    ray = Vector3.ProjectOnPlane(ray, Vector3.up);
    //    ray = ray.normalized * emptyLookDistance;
    //    RaycastHit info;
    //    Debug.DrawRay(this.transform.position, ray, Color.yellow);
    //    float distance = emptyLookDistance; //need to think about this distance : nextStep ? further ? far away ? half my size maybe ?
    //    if (Physics.Raycast(this.transform.position, ray, out RaycastHit raycastInfo, maxDist, layerMaskContact))
    //    {
    //        Debug.Log("Vas y ouesh tu m'vener");
    //    }

    //    if (Physics.Raycast(this.transform.position, ray, out info, distance, layerMaskContact))
    //    {
    //        Debug.Log("TOUUUUCHE ! Pitie ! TOUCHE !!!");
    //        //TOUCHEd !!!
    //        //ok, so, what we do ?
    //        //we use that as a reference ?
    //        //or just don't care and use that as an info ?
    //        return Vector3.zero;
    //    }
    //    else
    //    {
    //        Vector3 res = Vector3.zero;
    //        //OH OH ! Here we comes !
    //        Vector3 startPos = this.transform.position + ray;
    //        Quaternion rotationToAdd = Quaternion.FromToRotation(Vector3.forward, ray.normalized);

    //        int i = 0;
    //        foreach (Vector3 dir in emptyLookRaycastDirections)
    //        {
    //            Vector3 rotatedVector = rotationToAdd * dir;
    //            Debug.DrawRay(startPos, rotatedVector, Color.Lerp(Color.black, Color.white, (i++ * 1f / emptyLookRaycastDirections.Count)));
    //            distance = rotatedVector.magnitude; //need to think about this distance : nextStep ? further ? far away ? half my size maybe ?
    //            if (Physics.Raycast(startPos, rotatedVector, out info, distance, layerMaskContact))
    //            {
    //                Debug.Log("Je vais te... mais TOuUUUUUUUCHE !");
    //                //Ok, so, here, if we touched, change the speed !
    //                //to go to the inverse direction ?
    //                float intensity01 = (distance - info.distance) / distance; //maybe make it a Pow(x,2) to add more influence?

    //                Debug.Log("We offset (ponctuel) !" + intensity01 + " * " + (-rotatedVector));
    //                res += intensity01 * -rotatedVector;
    //            }
    //        }
    //        res *= emptyLookIntensity;
    //        Debug.Log("We offset (tota) !" + res);
    //        Debug.DrawRay(this.transform.position, res, Color.red);
    //        return res;
    //    }
    //}



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



    public float gravityIntensity = 9.81f;
    public float wallGravity = 9.81f;
    public float gravityIntensity_FallAdd = 9.81f;
    public void GravityManagement()
    {
        Vector3 localUp = Vector3.up;
        //Ok, revoir the "grapple mode"
        if (grapleMode_eff)
        {
            localUp = lastUpVector;
        }

        float dotClamped = Mathf.Clamp01(Vector3.Dot(localUp, Vector3.up));
        float gravitySum = Mathf.Lerp(wallGravity, gravityIntensity, dotClamped);
        if (contacts.Count == 0 && _rgbd.velocity.y < 0)
            gravitySum += gravityIntensity_FallAdd;

        _rgbd.velocity += -localUp * gravitySum * Time.deltaTime;

//maybe here : RayCastToFindAnythingInFrontOfUs(dir(2D));
    }



    [Header("Crouching")]
    public bool crouching = false;
    public float crouchDefaultSize = 1f;
    public float crouchGravity = 10f;
    public GameObject crouch_CrGO;
    public GameObject crouch_UpGO;

    public void CrouchManagement()
    {
        if (Input.GetKey(KeyCode.Joystick1Button3)
            || Input.GetKey(KeyCode.LeftShift)
            || Input.GetKey(KeyCode.RightShift))
        {
            if (!crouching)
            {
                _capsule.height = 1;
                _capsule.center = Vector3.down * (crouchDefaultSize - 1f) * 0.5f;
                crouch_CrGO.SetActive(true);
                crouch_UpGO.SetActive(false);
            }

            lastSpeed += Vector3.down * crouchGravity * Time.deltaTime;
            _rgbd.velocity = lastSpeed;

            crouching = true; //for the Quit Wall --> reverse speed
        }
        else if (crouching)
        {
            _capsule.height = crouchDefaultSize;
            _capsule.center = Vector3.zero;
            crouch_CrGO.SetActive(false);
            crouch_UpGO.SetActive(true);
            crouching = false;
        }
    }



    [Header("Collision and so")]
    public List<int> layerOnCollision;
    //public float offset = 0.3f;
    //public float size = 0.2f;
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
                if (!contacts.Contains(col))
                    AddContactAndTransposeSpeed(col);
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

    public void AddContactAndTransposeSpeed(PlayerMove.wallAndGround_Info col)
    {
        contacts.Add(col);
        //ok, for now, we fully transpose the speed !

        //Mauvaise idée : 
        // lorsqu'atterrie sur sol, glisse dans nimporte quelle direction
        // lorsque cogne mur, part parfois sur côté, parfois devant, parfois derrière etc...
        //donc la projection pur et simple, pas ouf
        // et la conservation de vitesse pas une bonne idée 
        
        Vector3 newSpeed = Vector3.ProjectOnPlane(lastSpeed, col.lastNormal);
        //the up of the surface
        Vector3 upOfTheWall = GetUpOfTheSurface(col.lastNormal);
        //the intensity of this up
        //0 if ground, 1 if wall more if roof
        float wallIntensity = 1 - Vector3.Dot(Vector3.up, col.lastNormal);
        float intensity = -Vector3.Dot(lastSpeed, col.lastNormal);
        newSpeed += upOfTheWall * intensity * wallIntensity;
        Debug.DrawRay(this.transform.position, upOfTheWall * intensity * wallIntensity, Color.yellow, 20f);
        _rgbd.velocity = newSpeed;
        Debug.Log("_rgbd.velocity = " + lastSpeed + "newSpeed = " + newSpeed);
        Debug.DrawRay(this.transform.position, lastSpeed, Color.blue, 20f);
        Debug.DrawRay(this.transform.position, newSpeed, Color.cyan, 20f);
        //Vector3 newSpeed = Vector3.ProjectOnPlane(lastSpeed, col.lastNormal).normalized;
        //newSpeed *= lastSpeed.magnitude;
        //
        //
        //float dotVal = Mathf.Abs(Vector3.Dot(lastSpeed, col.lastNormal));
        //_rgbd.velocity = Vector3.Lerp(_rgbd.velocity, newSpeed, dotVal);
        //Debug.Log("_rgbd.velocity = " + lastSpeed + "newSpeed = " + newSpeed + " choose : " + dotVal);


        //later, will probably make a little dot ?
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


    public Vector3 Clamp_AxisIgnored(Vector3 vector, float maxLenght, Vector3 axisToIgnore)
    {
	Debug.DrawRay(Vector3.zero, vector, Color.blue);
	Debug.DrawRay(Vector3.zero, axisToIgnore, Color.green);
        Vector3 projection = Vector3.ProjectOnPlane(vector, axisToIgnore);
	if(projection.magnitude <= maxLenght)
	{
		return vector;
	}
	Vector3 alongAxisValue = vector - projection;
	projection.Normalize();
	projection *= maxLenght;
	projection += alongAxisValue;
	Debug.DrawRay(Vector3.zero, alongAxisValue, Color.yellow);
	Debug.DrawRay(Vector3.zero, projection, Color.cyan);
	return projection;
    }
}
