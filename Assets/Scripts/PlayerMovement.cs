using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Component")]
    public CapsuleCollider _capsule;
    public SphereCollider _sphere;
    public Rigidbody _rgbd;
    private Transform cameraTr;

    [Header("Data")]
    public float speedGain = 10;
    public float speedMax = 5;
    public float slowDownMultiplier = 0.2f;
    private Vector3 lastSpeed;


    [Header("Jump")]
    public float jumpForce = 10f;
    public float jumpForce_FullJump = 10f;
    public AnimationCurve verticalBonusForHorizontalJump;
    public float jumpTiming = 1f;
    [ReadOnly] public float jumpTimer = 0f;
    [ReadOnly] public bool canJump = true;
    [ReadOnly] public bool inJump = false;
    [ReadOnly] public Vector3 jumpDirection;
    public float landingStress = 0f;
    public float landingIntensityMax = 20f;
    public float landingRecover = 4f;

    [Header("Coyote")]
    public float coyoteTiming = 0.1f;
    private float coyoteTimer = 0.1f;

    [Header("Other data")] [ReadOnly]
    public Vector3 lastUpVector = Vector3.up;
    public Vector3 center { get => this.transform.position + 0.7f * 0.2f * Vector3.up; }
    [ReadOnly]
    public List<wallAndGround_Info> contacts = new List<wallAndGround_Info>();

    [Header("Corridor redirector")]
    public float emptyLookDistance = 0.2f;
    public List<Vector3> emptyLookRaycastDirections = new List<Vector3> { Vector3.right, Vector3.down, Vector3.left };
    public float emptyLookIntensity = 0.1f;

    [Header("Talk")]
    [ReadOnly] public bool talking = false;
    [ReadOnly] public Vector3 speedWhenInterupt;
    
    [Header("Grapple mode")]
    public bool grapleMode = true;
    public bool grapleMode_eff = false;
    private bool grapleMode_Mem = false;
    public GameObject grappleVisual_On;
    public GameObject grappleVisual_Off;

    [Header("Gravity and gravities")]
    public float gravityIntensity = 9.81f;
    public float wallGravity = 9.81f;
    public float gravityIntensity_FallAdd = 9.81f;

    [Header("Crouching")]
    public bool crouching = false;
    public float crouchDefaultSize = 1f;
    public float crouchGravity = 10f;
    public Animator characterAnimator;

    [System.Serializable]
    public class wallAndGround_Info
    {
        public int id;
        public GameObject gO;
        public Vector3 lastNormal;

        public wallAndGround_Info(GameObject newWall, Vector3 impactNormal)
        {
            id = newWall.GetInstanceID();
            gO = newWall;
            lastNormal = impactNormal;
        }
        public static bool operator ==(wallAndGround_Info obj1, wallAndGround_Info obj2)
        {
            return obj1.id == obj2.id;
        }
        public static bool operator !=(wallAndGround_Info obj1, wallAndGround_Info obj2)
        {
            return obj1.id != obj2.id;
        }
        public static bool operator ==(wallAndGround_Info obj1, GameObject obj2)
        {
            return obj1.id == obj2.GetInstanceID();
        }
        public static bool operator !=(wallAndGround_Info obj1, GameObject obj2)
        {
            return obj1.id != obj2.GetInstanceID();
        }
        public static bool operator ==(GameObject obj1, wallAndGround_Info obj2)
        {
            return obj2.id == obj1.GetInstanceID();
        }
        public static bool operator !=(GameObject obj1, wallAndGround_Info obj2)
        {
            return obj2.id != obj1.GetInstanceID();
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }



    public void Start()
    {
        cameraTr = GameManager.instance.cameraMng.falseCamera.transform;
    }


    void ResetButton()
    {
        if (GameManager.instance.mapAndPaper != null)
        {
            if (GameManager.instance.mapAndPaper.mapOpen)
            {
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
            ResetAll();
    }

    // Update is called once per frame
    void Update()
    {
        ResetButton();

        if (talking)
            return;
        HandleGrappleWallMode();
        CrouchManagement();

        UpdateContact();
        MovementManagement();
        JumpManagement();

        GravityManagement();


        _ContactSafety();

        if (Input.GetKeyDown(KeyCode.K))
            Application.targetFrameRate = 60;
        if(Input.GetKeyDown (KeyCode.L))
            Application.targetFrameRate = 10;
    }


    private void HandleGrappleWallMode()
    {
        //switch
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            grapleMode = !grapleMode;
        }

        //hold
        if (Input.GetKey(KeyCode.Mouse1))
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

        Vector3 forwardCam_Gr = GameManager.instance.cameraMng.mainCamera.transform.forward;
        Vector3 groundRight = GameManager.instance.cameraMng.mainCamera.transform.right;
        Vector3 upVector = lastUpVector;


        //Decide which UpVector we are gonna use
        if (contacts.Count == 0)
        {
            if(coyoteTimer >= 0)
            {
                coyoteTimer -= Time.deltaTime;
            }
            else
            {
                //Normally, coyote time here
                upVector = Vector3.up;
            }
        }
        else if (contacts.Count == 1)
        {
            upVector = contacts[0].lastNormal;
        }
        else
        {
            //ok, so, for now, just take the vector who is touch by joyLook.
            Vector3 direction = forwardCam_Gr * inputDir.y + groundRight * inputDir.x;
            upVector = SetVectorUpForMultipleElement(direction);
        }


        //Debug.DrawRay(center, upVector, Color.red);


        Vector3 groundForward = Vector3.ProjectOnPlane(forwardCam_Gr, upVector).normalized;
        //this is the ground forward 
        Vector3 wallForward = ConvertForwardToWall(upVector, forwardCam_Gr);
        //Change forward if we are on a wall :
        float clampedDotValue = Mathf.Clamp01(Vector3.Dot(upVector, Vector3.up));

        //debugValue = clampedDotValue;
        //Debug.DrawRay(center, groundForward, Color.white);
        groundRight = Vector3.Cross(upVector, groundForward).normalized;
        Vector3 wallRight = Vector3.Cross(upVector, wallForward).normalized;

        //
        lastUpVector = upVector;

        Vector3 accelerationGround = inputDir.x * groundRight + inputDir.y * groundForward;
        Vector3 accelerationWall   = inputDir.x * wallRight   + inputDir.y * wallForward;
        //Debug.DrawRay(center, accelerationGround, Color.red);

        //Ok, revoir the "grapple mode"
        if (!grapleMode_eff)
        {
            float yAcc = accelerationWall.y;
            if (yAcc > 0)
            {
                //Transform the y part into vertical velocity
                //let's gooo
                accelerationWall.y = 0;
                //ok let's try by giving the ySpeed into the horizontal speed
                accelerationWall *= yAcc;
                Debug.Log("Flattened the acceleration");
            }
        }

        Vector3 acceleration = Vector3.Lerp(accelerationWall, accelerationGround, clampedDotValue);
        Debug.DrawRay(center, acceleration, Color.red);

        acceleration += RayCastToFindAnythingInFrontOfUs(inputDir);
        acceleration *= speedGain * Time.deltaTime;


        _rgbd.velocity += acceleration;

        _rgbd.velocity= Clamp_AxisIgnored(_rgbd.velocity, speedMax, upVector);

        //we can now  make the drag here :
        _rgbd.velocity = _rgbd.velocity * Mathf.Pow(slowDownMultiplier, Time.deltaTime);

        ////////Debug.DrawRay(center, _rgbd.velocity, Color.blue);
        ////////Debug.DrawRay(center + _rgbd.velocity, acceleration, Color.green);
        //Debug.DrawRay(center, forwardCam, Color.yellow);
        //Debug.DrawRay(center, rightCam, Color.yellow / 2f);
        lastSpeed = _rgbd.velocity;





    }



    public Vector3 SetVectorUpForMultipleElement(Vector3 direction)
    {
        //Debug.DrawRay(center, direction.normalized * maxDist, Color.red);
            Debug.DrawRay(center, direction, Color.cyan);
        //////if (Physics.Raycast(center, direction, out RaycastHit raycastInfo, maxDist, layerMaskContact))
        //////{
        //////    if (AlreadyInContact(raycastInfo.collider.gameObject))
        //////    {
        //////        //normallly, we goes directly by "'GetContact" then test if it's null or not, but get some bug where the simple "null test" return an error
        //////        wallAndGround_Info contactInfo = GetContact(raycastInfo.collider.gameObject);
        //////        Debug.DrawRay(center, contactInfo.lastNormal, Color.yellow + Color.green);
        //////        return contactInfo.lastNormal;
        //////    }
        //////    //else, it is as if you touch nothing.
        //////}
        //if no vector touch, use the most vertical one ! (and if equality, try the Dot with the joylook ???)
        Vector3 upVector = lastUpVector;
        //float maxDot = -1;
        Vector3 newUpVector = Vector3.zero;
        float dotSum = 0;
        foreach (wallAndGround_Info info in contacts)
        {
            float newDot = Mathf.Clamp01(Vector3.Dot(info.lastNormal.normalized, -direction.normalized));
            if(newDot > 0)
            {
                dotSum += 1;
                newUpVector += info.lastNormal * 1;
            }
            //if (maxDot < newDot)
            //{
            //    maxDot = newDot;
            //    upVector = info.lastNormal;
            //}
            //else if (maxDot == newDot)
            //{
            //    //Mean ?
            //    upVector = (info.lastNormal + upVector) / 2;//bad way to do it, but really few case where we touch more than 2 surface who are exactly the same dot
            //}
        }
        if (dotSum == 0)
            return upVector;
        upVector = newUpVector / dotSum;
        Debug.DrawRay(center, upVector, Color.yellow);
        return upVector;
    }

    public Vector3 ConvertForwardToWall(Vector3 wallNormal, Vector3 forwardCam)
    {
        Vector3 res = Vector3.up;
        ///Ok, so , we need to calculate the difference between the -wallNormal ET le current forwardCam
        Vector3 flatenedWallNorm = Vector3.ProjectOnPlane(-wallNormal, Vector3.up).normalized;
        Vector3 flatenedForwardV = Vector3.ProjectOnPlane(forwardCam, Vector3.up).normalized;
        float angle = Vector3.Angle(flatenedWallNorm, flatenedForwardV);
        //Debug.DrawRay(center, flatenedWallNorm, Color.cyan);
        //Debug.DrawRay(center, flatenedForwardV, Color.magenta);
        angle *= Mathf.Sign(Vector3.Dot(Vector3.Cross(Vector3.up, flatenedWallNorm), flatenedForwardV));
        ///Ca va nous donner un angle.
        ///Cette angle, on va l'appliquer au UPVECTOR noirmalisé sur le mur 
        Vector3 wallUpVector = GetUpOfTheSurface(wallNormal);
        //Debug.DrawRay(center, wallUpVector, Color.grey);
        //if wallNormal is Vector3.up, it should be nullify in the global code

        //// autour de l'axe wallNormal !
        Quaternion rotation = Quaternion.AngleAxis(angle, wallNormal); // - wallNormal ????
        debugValue = angle;
        ////// et ça nous donneras le forward  actuel. 
        res = rotation * wallUpVector;
        // tu le cross avec le wallNormal, tu obtiens la droite.
        //gg 



        //Debug.DrawRay(center, res, Color.black);
        return res;
    }

    [Header("Contact and ray")]
    public float maxDist = 0.5f;
    public LayerMask layerMaskContact;
    void UpdateContact()
    {
        foreach (wallAndGround_Info info in contacts)
        {
            Vector3 direction = -info.lastNormal;
            //Debug.DrawRay(center, direction.normalized * maxDist, Color.red);
            if (Physics.Raycast(center, direction, out RaycastHit raycastInfo, maxDist, layerMaskContact))
            {
                if (raycastInfo.collider.gameObject == info)
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
        foreach (wallAndGround_Info info in contacts)
        {
            if (info == gO)
            {
                return true;
            }
        }
        return false;
    }
    public wallAndGround_Info GetContact(GameObject gO)
    {
        foreach (wallAndGround_Info info in contacts)
        {
            if (info == gO)
            {
                return info;
            }
        }
        return null;
    }


    public Vector3 RayCastToFindAnythingInFrontOfUs(Vector2 inputDir)
    {
        //if the player don't move much, just ignore this step
        if (inputDir.magnitude < 0.5f)
            return Vector3.zero;

        Vector3 ray = cameraTr.forward * inputDir.y + cameraTr.right * inputDir.x; //so, check where the player "look" and indicate with his joystick
        ray = Vector3.ProjectOnPlane(ray, Vector3.up);
        ray = ray.normalized * emptyLookDistance;
        RaycastHit info;
        //Debug.DrawRay(centerOfCharacter, ray, Color.yellow);
        float distance = emptyLookDistance; 

        if (Physics.Raycast(center, ray, out info, distance, layerMaskContact))
        {
            return Vector3.zero;
        }
        else
        {
            Vector3 res = Vector3.zero;
            //Try to find other collision around, that we already touch too, and so, try to go away from them
            Vector3 startPos = center + ray;
            Quaternion rotationToAdd = Quaternion.FromToRotation(Vector3.forward, ray.normalized);
            
            foreach (Vector3 dir in emptyLookRaycastDirections)
            {
                Vector3 rotatedVector = rotationToAdd * dir;
                Debug.DrawRay(startPos, rotatedVector, Color.blue);
                distance = rotatedVector.magnitude; //need to think about this distance : nextStep ? further ? far away ? half my size maybe ?
                if (Physics.Raycast(startPos, rotatedVector, out info, distance, layerMaskContact))
                {
                    //Ok, so, here, if we touched, change the speed !
                    //to go to the inverse direction ?
                    float intensity01 = (distance - info.distance) / distance; //maybe make it a Pow(x,2) to add more influence?
                    
                    res += intensity01 * -rotatedVector;
                }
            }
            res *= emptyLookIntensity;
            //if(res != Vector3.zero)
            //    Debug.Log("We offset (tota) !" + res);
            //Debug.DrawRay(center, res, Color.red);
            return res;
        }
    }



    //[Range(0,1)]
    public float debugValue = 0;

    private void JumpManagement()
    {
        //if (inJump)
        //{
        //    if (lastSpeed.y < 0)
        //    {
        //        inJump = false;
        //    }
        //    else if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Joystick1Button0))
        //    {
        //        if (jumpTimer > 0)
        //        {
        //            jumpTimer -= Time.deltaTime;
        //            Debug.Log("Add and add and add" + (jumpForce_FullJump * jumpDirection.normalized));
        //            if (jumpTimer < 0)
        //            {
        //                inJump = false;
        //            }
        //            else
        //                _rgbd.AddForce(jumpForce_FullJump * jumpDirection.normalized * jumpTimer / jumpTiming, ForceMode.Impulse);
        //        }
        //    }
        //    else
        //    {
        //        inJump = false;
        //    }
        //}

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            if (canJump)
            {
                _rgbd.velocity = PlayerMovement.HorizontalOnly(_rgbd.velocity);
                //Will have to "incline" the jump toward : the koystock direction + the normal of the ground
                Vector3 newJumpDirection = lastUpVector;
                float value = Vector3.Dot(Vector3.up, lastUpVector.normalized);
                //if the normal is the same as the Up : return zero
                //if the normal is 0 with up : return a up
                //if the normal  is downward (-1) : return a -1
                value = verticalBonusForHorizontalJump.Evaluate(value);
                jumpDirection += Vector3.up * value;
                
                _rgbd.AddForce(jumpForce * jumpDirection.normalized, ForceMode.Impulse);
                inJump = true;
                jumpDirection = newJumpDirection;
                jumpTimer = jumpTiming;
                canJump = false;
            }
        }
    }



    public void GravityManagement()
    {
        Vector3 localUp = Vector3.up;
        if (grapleMode_eff)
        {
            localUp = lastUpVector;
        }

        float dotAbs = Vector3.Dot(localUp, Vector3.up); //abs, so if roof, you fall

        float gravitySum = Mathf.Lerp(wallGravity, gravityIntensity * Mathf.Sign(dotAbs), Mathf.Abs(dotAbs));
        if (contacts.Count == 0 && _rgbd.velocity.y < 0)
            gravitySum += gravityIntensity_FallAdd;

        _rgbd.velocity += -localUp * gravitySum * Time.deltaTime;
        lastSpeed = _rgbd.velocity;

//maybe here : RayCastToFindAnythingInFrontOfUs(dir(2D));
    }




    public void CrouchManagement()
    {
        if (Input.GetKey(KeyCode.Joystick1Button3)
            || Input.GetKey(KeyCode.LeftShift)
            || Input.GetKey(KeyCode.RightShift))
        {
            if (!crouching)
            {
                _capsule.enabled = false;
                _sphere.enabled = true;
                characterAnimator.SetBool("Close", true);
            }

            _rgbd.velocity += Vector3.down * crouchGravity * Time.deltaTime;
            lastSpeed = _rgbd.velocity;

            crouching = true; //for the Quit Wall --> reverse speed
        }
        else if (crouching)
        {
            _capsule.enabled = true;
            _sphere.enabled = false;
            characterAnimator.SetBool("Close", false);
            crouching = false;
        }


        landingStress = Mathf.Clamp01(landingStress - Time.deltaTime * landingRecover);
        characterAnimator.SetLayerWeight(1, landingStress * landingStress);
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
            //Check the layer :
            if (((1 << collision.gameObject.layer) & layerMaskContact) == 0)
            {
                //It is not on the layer mask, so get ignore
                return;
            }

            Vector3 impactNormal = collision.contacts[0].normal;
            //if (Vector3.Dot(Vector3.down, impactNormal) <= 0)//So, we can move in the plafond
            {
                //if object is part of decor 
                //if normal of collision is not too sharp
                wallAndGround_Info col = new wallAndGround_Info(collision.collider.gameObject, impactNormal);
                if (!contacts.Contains(col))
                    AddContactAndTransposeSpeed(col, collision.contacts[0]);
                else
                {
                    Debug.LogError("Alreday have " + collision.collider.gameObject.name);
                }

                //Landing if the collision is flat enough (reset jump, make a sound and launch animation)
                if (Vector3.Dot(Vector3.down, impactNormal) <= 0)
                {
                    float landingIntensity = Vector3.Dot(collision.impulse, Vector3.up) * collision.impulse.y;
                    //Debug.Log("Impact : " + landingIntensity);
                    landingStress += landingIntensity / landingIntensityMax;
                    GameManager.instance.soundMng.HurtGround(landingStress);
                    canJump = true;
                }
            }
        }

    }

    public void AddContactAndTransposeSpeed(wallAndGround_Info col, ContactPoint contact)
    {
        contacts.Add(col);
        coyoteTimer = coyoteTiming;



        
        Vector3 newUpVector = col.lastNormal;

        //Verify the true normal of that surface (and not the collision normal)
        //Debug.DrawRay(contact.point, -col.lastNormal, Color.blue,  5f);
        //Debug.DrawRay(center, -col.lastNormal, Color.cyan,  5f);
        if (Physics.Raycast(center, -col.lastNormal, out RaycastHit raycastInfo, maxDist, layerMaskContact))
        {
            if (col == raycastInfo.collider.gameObject)
            {
                //We touch the same element again, good signe !
                col.lastNormal = raycastInfo.normal;
            }
        }
        //

        //Now, rearrange the upVector depending on what around and the collision position
        newUpVector = RaycastSpeedToSeekSurface(newUpVector, col.lastNormal, contact);
        
    
        Vector3 newSpeed = Vector3.ProjectOnPlane(lastSpeed, newUpVector);
        //the up of the surface
        Vector3 upOfTheWall = GetUpOfTheSurface(newUpVector);
        //the intensity of this up
        //0 if ground, 1 if wall more if roof
        float wallIntensity = 1 - Vector3.Dot(Vector3.up, newUpVector);
        float intensity = -Vector3.Dot(lastSpeed, newUpVector);
        newSpeed += upOfTheWall * intensity * wallIntensity;
        

        lastSpeed = newSpeed;
        //Debug.DrawRay(center, _rgbd.velocity.normalized * maxDist, Color.red + Color.yellow, 5f);
        _rgbd.velocity = newSpeed;
        //Debug.Log("_rgbd.velocity = " + lastSpeed + "newSpeed = " + newSpeed);

        lastUpVector = newUpVector;

        
    }

    //Use the current speed to raycast and if it find a touched surface, return the normal of it. Else, try to understand it.
    public Vector3 RaycastSpeedToSeekSurface(Vector3 collisionVector, Vector3 normal, ContactPoint contact)
    {
        //now, try to see how to change the speed :
        //Debug.DrawRay(center, lastSpeed.normalized * maxDist, Color.red, 5f);
        //Debug.DrawRay(center, _rgbd.velocity.normalized * maxDist, Color.red + Color.yellow, 5f);
        if (Physics.Raycast(center, lastSpeed, out RaycastHit speedcastInfo, maxDist, layerMaskContact))
        {
            if (AlreadyInContact(speedcastInfo.collider.gameObject))
            {
                //if the speed goes on a wall, use the wall as the new upVector.  
                return GetContact(speedcastInfo.collider.gameObject).lastNormal;
            }
        }
        //gonna ignore the change if the speed ray touch a surface, and just take that surface (if it's in the contact list)

        //if no surface touch (or surface not in contact)
        //seek how much we need one of the other
        Vector3 collisionAngle = contact.point - center;
        //can probably counter the stairs effect, with the distance of this ?
        float dotValue = Vector3.Dot(lastSpeed.normalized, collisionAngle.normalized);

        //If on wall, keep more the current normal. Else, you can use the coll
        //problem : strictly use 0 when on wall (I want to be able to move a little) 
        float secondaryIntensity = Vector3.Dot(lastUpVector, Vector3.up);
        //Debug.Log("secondaryIntensity = "+ secondaryIntensity);

        Debug.DrawRay(center, normal         , Color.cyan, 5f);
        Debug.DrawRay(center, collisionVector, Color.blue, 5f);
        Debug.DrawRay(center + Vector3.up * 0.02f, Vector3.Lerp(normal, collisionVector, dotValue * secondaryIntensity), Color.black, 5f);
        //Debug.Log("Ok so " + secondaryIntensity);
        return Vector3.Lerp(normal, collisionVector, dotValue * secondaryIntensity).normalized;

    }



    public void OnCollisionExit(Collision collision)
    {
        for (int i = 0; i < contacts.Count; i++)
        {
            wallAndGround_Info info = contacts[i];
            if (info == collision.gameObject)
            {
                contacts.Remove(info);
                i--;
            }
        }
    }

    private float __safetyTiming = 5f;
    private float __safetyTimer = 5f;
    public void _ContactSafety()
    {
        if (__safetyTiming > __safetyTimer)
        {
            __safetyTimer += Time.deltaTime;
            return;
        }
        __safetyTimer = 0;
        //Debug.Log("Safety check");

        Collider[] cols = Physics.OverlapSphere(center, raycastDist * 2, layerMaskContact);
        List<GameObject> gOTouch = new List<GameObject>();
        foreach (Collider col in cols)
            gOTouch.Add(col.gameObject);

        //foreach(GameObject gO in gOTouch) { Debug.Log(" - we see " + gO.name); }

        for (int i = 0; i < contacts.Count; i++)
        {
            wallAndGround_Info contact = contacts[i];
            if (ContactInGameObjectList(gOTouch, contact))
            {
                continue;
            }
            //else 
            //This means we don't touch this anymore !
            Debug.LogError("Have to get rid of "+ contact.gO.name + " because we don't touch it anymore.");
            contacts.RemoveAt(i);
            i--;
        }

    }

    private bool ContactInGameObjectList(List<GameObject> gOTouch, wallAndGround_Info contact)
    {
        foreach (GameObject gO in gOTouch)
        {
            if (gO == contact)
            {
                //Good ! Can break here
                return true;
            }
        }
        return false;
    }


    #region External method

    public void Talk()
    {
        InventoryAndMenu();
    }
    public void FinishTalk()
    {
        FinishMenuing();
    }
    public void InventoryAndMenu()
    {
        talking = true;
        speedWhenInterupt = _rgbd.velocity;
        _rgbd.velocity = Vector3.zero;
        _rgbd.isKinematic = true;

        Cursor.lockState = CursorLockMode.None;
    }
    public void FinishMenuing()
    {
        talking = false;
        _rgbd.isKinematic = false;
        _rgbd.velocity = speedWhenInterupt;

        Cursor.lockState = CursorLockMode.Locked;
    }


    //Debug

    public void ResetAll()
    {
        this.transform.position = Vector3.up;
        _rgbd.velocity = Vector3.zero;
        lastSpeed = Vector3.zero;
        lastUpVector = Vector3.up;
        canJump = false;
    }

    #endregion






    public static Vector3 Clamp_AxisIgnored(Vector3 vector, float maxLenght, Vector3 axisToIgnore)
    {
        Vector3 projection = Vector3.ProjectOnPlane(vector, axisToIgnore);
        if (projection.magnitude <= maxLenght)
        {
            return vector;
        }
        Vector3 alongAxisValue = vector - projection;
        projection.Normalize();
        projection *= maxLenght;
        projection += alongAxisValue;
        return projection;
    }





    public static Vector3 HorizontalOnly(Vector3 vec)
    {
        vec.y = 0;
        return vec;
    }
    public static float HorizontalMagnitude(Vector3 vec)
    {
        vec.y = 0;
        return vec.magnitude;
    }
    public static Vector3 HorizontalClamp(Vector3 vec, float lenghtMax)
    {
        if (HorizontalMagnitude(vec) <= lenghtMax)
            return vec;
        float yMem = vec.y;
        vec.y = 0;
        vec = vec.normalized * lenghtMax;
        vec.y = yMem;
        return vec;
    }
}
