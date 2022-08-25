using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private Transform cameraTr;
    public Rigidbody _rgbd;

    [Header("Mouvement")]
    [Tooltip("The higher the quickier we reach full speed")]
    public float groundGain = 1f;
    [Tooltip("Max horizontal speed")]
    public float groundSpeed = 1f;
    public float wallSpeed = 0.5f;
    [Tooltip("The higher the quickier we stop")]
    [Range(0, 3)]
    public float drag = 2;
    [Space]
    public Vector3 lastSpeed = Vector3.zero;
    public Vector3 acceleration = Vector3.zero;

    [Header("Jump")]
    public float jumpForce = 10f;
    public float verticalBonusForHorizontalJump = 0.3f;
    public bool canJump = true;




    [Header("Wall and ground")]
    public List<wallAndGround_Info> wallAndGround = new List<wallAndGround_Info>();
    public Vector3 currentNormal = Vector3.up;
    public float checkGroundDistance=0.2f;

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
    }


    // Start is called before the first frame update
    void Start()
    {
        _rgbd.useGravity = false;
        _rgbd.drag = drag;
        cameraTr = GameManager.instance.cameraMng.falseCamera.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAll();
        }

        //Both jump and movement
        MovementManagement();



        DebugMethod();
    }

    public void DebugMethod()
    {
        //Debug.DrawRay(this.transform.position, currentNormal, Color.red);
    }

    private void MovementManagement()
    {
        Vector2 inputDirection = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
            );

        CheckGround(inputDirection);

#if UNITY_EDITOR
        _rgbd.drag = drag;
#endif 
        lastSpeed = _rgbd.velocity;
        if (inputDirection != Vector2.zero)
        {
            Vector3 groundAcc = GetGroundMove(inputDirection);
            Vector3 wallAcc = GetWallMove(inputDirection);

            float dotNormalToUp = Mathf.Clamp01(Vector3.Dot(currentNormal, Vector3.up)); 

            acceleration =  groundAcc * dotNormalToUp + wallAcc * (1 - dotNormalToUp);
            //Debug.Log("acceleration "+ acceleration + " = " + dotNormalToUp + " + " + (1 - dotNormalToUp) + " . "+ wallAcc + "/"+groundAcc);

            lastSpeed += acceleration * Time.deltaTime;

            //if (dotNormalToUp)
            {
                Vector3 speedAtWallMagnitude = (lastSpeed.magnitude > wallSpeed ? lastSpeed.normalized * wallSpeed : lastSpeed);
                lastSpeed = Vector3.Lerp(speedAtWallMagnitude, lastSpeed, dotNormalToUp);
            }
            if (HorizontalMagnitude(lastSpeed) > groundSpeed)
            {
                lastSpeed = HorizontalClamp(lastSpeed, groundSpeed);
            }
        }

        //will also handle wall sticking

        _rgbd.velocity = lastSpeed;

        //Only for visual aid
        ////9s
        //GetComponent<TrailRenderer>().material.color = Color.green - Color.black * Mathf.Clamp01(10 - Time.timeSinceLevelLoad);   
        ////6s
        //Debug.DrawLine(this.transform.position, this.transform.position+lastSpeed/4, Color.red - Color.black * Mathf.Clamp01(7 - Time.timeSinceLevelLoad));
        ////4s
        //Debug.DrawLine(this.transform.position + lastSpeed / 4, this.transform.position + lastSpeed /4 + acceleration / 32, Color.blue - Color.black * Mathf.Clamp01(5 - Time.timeSinceLevelLoad));
        //Vector3 pos = new Vector3(-2.5f, 2f, -5f);
        ////1s
        //Debug.DrawLine(pos, pos + Vector3.right * inputDirection.x/2 + Vector3.forward * inputDirection.y/2, Color.magenta - Color.black * Mathf.Clamp01(2 - Time.timeSinceLevelLoad));

        JumpManagement();
    }

    private Vector3 GetGroundMove(Vector2 inputDirection)
    {
        Vector3 forwardDir = Vector3.ProjectOnPlane(cameraTr.forward, currentNormal).normalized;
        Vector3 rightDir = Vector3.Cross(currentNormal, forwardDir);
        //Debug.DrawRay(this.transform.position, rightDir, Color.red);

        Vector3 res;
        res = inputDirection.x * rightDir;
        res += inputDirection.y * forwardDir;
        res = groundGain * inputDirection.magnitude * res.normalized;


        return res;
    }
    private Vector3 GetWallMove(Vector2 inputDirection)
    {
        //In this case, we project the cam forward onto a horizontal plane
        //to avoid having to look up to go up. 
        //Creation of this horizontal-ish plane :
        Vector3 upOnPlane = Vector3.ProjectOnPlane(Vector3.up, currentNormal).normalized;
        Vector3 rightOnPlane = Vector3.Cross(currentNormal, upOnPlane).normalized;
        //Debug.DrawRay(this.transform.position, rightOnPlane, Color.yellow);

        //Project the cam on it
        Vector3 camProj = Vector3.ProjectOnPlane(cameraTr.forward, upOnPlane).normalized;
        //Transform this vector at horizontal into the same vector but in the surface plane
        float dotForward = Vector3.Dot(camProj, -currentNormal);
        float dotRight = Vector3.Dot(camProj, rightOnPlane);

        //We return back to same calcul as in groundMove()
        Vector3 forwardDir = upOnPlane * dotForward + rightOnPlane * dotRight;
        Vector3 rightDir = Vector3.Cross(currentNormal, forwardDir);

        Vector3 res;
        res = inputDirection.x * rightDir;
        res += inputDirection.y * forwardDir;
        res = groundGain * inputDirection.magnitude * res.normalized;

        return res;
    }


    private void JumpManagement()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            if (canJump)
            {
                _rgbd.velocity = HorizontalOnly(_rgbd.velocity);
                lastSpeed = _rgbd.velocity;
                Vector3 higherThanNormal = currentNormal + Vector3.up * verticalBonusForHorizontalJump;
                Vector3 jumpDirection = Vector3.Lerp(higherThanNormal.normalized, Vector3.up, Vector3.Dot(currentNormal, Vector3.up));
                //Will have to "incline" the jump toward : the koystock direction + the normal of the ground
                _rgbd.AddForce(jumpForce * jumpDirection, ForceMode.Impulse);
                canJump = false;
            }
        }
    }


    private void FixedUpdate()
    {
        GravityManagement();
    }

    [Header("Gravity")]
    [Range(-1,1)]
    public float gravityMultiplier = 1f;
    public float gravityDefault = 9.81f;
    public float gravityOnFalling = 18.1f;
    public float surfaceAttraction = 2f;

    public void GravityManagement()
    {
        gravityMultiplier = Vector3.Dot(currentNormal, Vector3.up);

        _rgbd.velocity -= currentNormal * gravityMultiplier * surfaceAttraction * Time.fixedDeltaTime;

        gravityMultiplier = Mathf.Abs(gravityMultiplier);
        if (gravityMultiplier <= 0)
            return;
        
        
        if(wallAndGround.Count == 0 && _rgbd.velocity.y < 0)
        {
            _rgbd.velocity += Vector3.down * gravityOnFalling * Time.fixedDeltaTime;
        }
        else
        {
            _rgbd.velocity += Vector3.down * gravityDefault * gravityMultiplier * Time.fixedDeltaTime;
        }

    }



    #region Collision and so

    [Header("Collision and so")]
    public int layerOnCollision = 0;
    //public float offset = 0.3f;
    //public float size = 0.2f;
    public LayerMask layerMask;
    public float raycastDist = 0.15f;
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == (layerOnCollision))
        {
            if (collision.contactCount > 0)
            {
                Vector3 impactNormal = collision.contacts[0].normal;
                //if (Vector3.Dot(Vector3.down, impactNormal) <= 0)//So, we can move in the plafond
                {
                    if (!WallAlreadyTouching(collision.gameObject))
                    {
                        AddWall(collision.gameObject, impactNormal);


                        RecalculateNormal();
                    }

                    //Reset Jump
                    if(Vector3.Dot(Vector3.down, impactNormal) <= 0)
                        canJump = true;
                }
            }
        }

        //if object is part of decor 
        //if normal of collision is not too sharp
        
    }
    public void OnCollisionExit(Collision collision)
    {
        Debug.Log("Exit : " + collision.gameObject.name);
        if (WallAlreadyTouching(collision.gameObject))
        {
            RemoveWall(collision.gameObject);
            RecalculateNormal();
        }
    }

    private bool WallAlreadyTouching(GameObject obj)
    {
        foreach(wallAndGround_Info wall in wallAndGround)
        {
            if (wall.id == obj.GetInstanceID())
                return true; 
        }
        return false;
    }
    private void AddWall(GameObject obj, Vector3 normal)
    {
        Debug.Log("Gain : " + obj.name + " with a normal of " + normal);
        wallAndGround_Info newWall = new wallAndGround_Info(obj, normal);
        wallAndGround.Add(newWall);

        //Ok, just to test it out : 
        _rgbd.velocity = Vector3.ProjectOnPlane(_rgbd.velocity, currentNormal);
        lastSpeed = _rgbd.velocity;

    }
    private void RemoveWall(GameObject obj)
    {
        Debug.Log("Lost : " + obj.name);
        int index = -1;
        for (int i = 0; i < wallAndGround.Count; i++)
        {
            wallAndGround_Info wall = wallAndGround[i];
            if (wall.id == obj.GetInstanceID())
            {
                index = i;
                break;
            }
        }
        wallAndGround.RemoveAt(index);
    }

    private void RecalculateNormal()
    {
        if (wallAndGround.Count == 0)
            currentNormal = Vector3.up;
        else if (wallAndGround.Count == 1)
        {
            currentNormal = wallAndGround[0].lastNormal;
        }
        else
        {
            //if(normal dot vectorUp < 0 then don't count this). Or like, gravity is back 
            //also : if input direction : take that direction to know which surface to use
            currentNormal = wallAndGround[wallAndGround.Count - 1].lastNormal;
        }
    }

    private void ChooseMostVerticalWall()
    {
        currentNormal = Vector3.up;
        float maxDotValue = -1;
        foreach (wallAndGround_Info wallAndGround in wallAndGround)
        {
            float dotVal = Vector3.Dot(Vector3.up, wallAndGround.lastNormal);
            if (maxDotValue < dotVal)
            {
                currentNormal = wallAndGround.lastNormal;
                maxDotValue = dotVal;
            }
        }
    }


    private void CheckGround(Vector2 inputDirection = new Vector2())
    {
        RaycastHit info;

        Vector3 ray = -currentNormal;
        if(inputDirection != Vector2.zero)
        {
            Vector3 forwardDir = Vector3.ProjectOnPlane(cameraTr.forward, Vector3.up).normalized;
            Vector3 rightDir = Vector3.Cross(currentNormal, forwardDir);

            ray = forwardDir * inputDirection.y + rightDir * inputDirection.x;
            ray = ray.normalized * checkGroundDistance;
        }

        Debug.DrawRay(this.transform.position, ray, (inputDirection != Vector2.zero) ? Color.green : Color.blue);
        if (Physics.Raycast(this.transform.position, ray, out info, raycastDist, layerMask.value))
        {
            //what did I hurt ?
            //ok, then, I should follow that think !
            if (WallAlreadyTouching(info.collider.gameObject))
            {
                currentNormal = info.normal;
                //Debug.DrawRay(this.transform.position, -currentNormal.normalized * raycastDist, Color.red);
                return;
            }
            //else : the wall we touch is not touch actually. Consider  it too far away from us. Ignore it.
            //Might create trouble later.

        }
        //else

        //I hurt nothing ? Then, let see what wall have the most UpVector and choose it 
        ChooseMostVerticalWall();
        //Debug.DrawRay(this.transform.position, -currentNormal.normalized * raycastDist, new Color(1, 0, 1));
        
    }

    #endregion

    //Debug

    public void ResetAll()
    {
        this.transform.position = Vector3.up;
        acceleration = Vector3.zero;
        _rgbd.velocity = Vector3.zero;
        lastSpeed = Vector3.zero;
        canJump = false;
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
        float yMem = vec.y;
        vec.y = 0;
        vec = vec.normalized * lenghtMax;
        vec.y = yMem;
        return vec;
    }

}
