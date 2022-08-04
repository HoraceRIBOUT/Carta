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
    [Tooltip("The higher the quickier we stop")]
    [Range(0, 3)]
    public float drag = 2;
    [Space]
    public Vector3 lastSpeed = Vector3.zero;
    public Vector3 acceleration = Vector3.zero;

    [Header("Jump")]
    public float jumpForce = 10f;
    public bool canJump = true;




    [Header("Wall and ground")]
    public List<wallAndGround_Info> wallAndGround = new List<wallAndGround_Info>();

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

    public Vector3 currentNormal = Vector3.up;

    // Start is called before the first frame update
    void Start()
    {
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

        CheckGround();

        //Both jump and movement
        MovementManagement();



        DebugMethod();
    }

    public void DebugMethod()
    {
        Debug.DrawRay(this.transform.position, currentNormal, Color.red);
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
            Vector3 groundAcc = GetGroundMove(inputDirection);
            Vector3 wallAcc = GetWallMove(inputDirection);

            float dotNormalToUp = Vector3.Dot(currentNormal, Vector3.up); //cannot be under 1 normaly 

            acceleration =  groundAcc * dotNormalToUp + wallAcc * (1 - dotNormalToUp);
            //Debug.Log("acceleration "+ acceleration + " = " + dotNormalToUp + " + " + (1 - dotNormalToUp) + " . "+ wallAcc + "/"+groundAcc);

            lastSpeed += acceleration * Time.deltaTime;

            if (HorizontalMagnitude(lastSpeed) > groundSpeed)
            {
                lastSpeed = HorizontalClamp(lastSpeed, groundSpeed);
            }
        }

        //will also handle wall sticking

        _rgbd.velocity = lastSpeed;

        JumpManagement();
    }

    private Vector3 GetGroundMove(Vector2 inputDirection)
    {
        Vector3 forwardDir = Vector3.ProjectOnPlane(cameraTr.forward, currentNormal).normalized;
        Vector3 rightDir = Vector3.Cross(currentNormal, forwardDir);
        Debug.DrawRay(this.transform.position, rightDir, Color.red);

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
        Debug.DrawRay(this.transform.position, rightOnPlane, Color.yellow);

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
                //Will have to "incline" the jump toward : the koystock direction + the normal of the ground
                _rgbd.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
                canJump = false;
            }
        }
    }


    #region Collision and so

    public int layerOnCollision = 0;
    public float offset = 0.3f;
    public float size = 0.2f;
    public LayerMask layerMask;
    public float raycastDist = 0.15f;
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == (layerOnCollision))
        {
            if (collision.contactCount > 0)
            {
                Vector3 impactNormal = collision.contacts[0].normal;
                if (Vector3.Dot(Vector3.down, impactNormal) <= 0)
                {
                    if (!WallAlreadyTouching(collision.gameObject))
                    {
                        ////Verify if the collision is cool : else, correct it 
                        //Vector3 pointToStartFrom = collision.contacts[0].point + impactNormal * size;
                        //Vector3 dirRight = Vector3.Cross(impactNormal, Vector3.right);//a random perpendicular vector
                        //Vector3 dirUp    = Vector3.Cross(impactNormal, dirRight);
                        //Vector3 pointToStartFromRIGHT   = pointToStartFrom + dirRight * offset;
                        //Vector3 pointToStartFromLEFT    = pointToStartFrom - dirRight * offset;
                        //Vector3 pointToStartFromUP      = pointToStartFrom + dirUp * offset;
                        //Vector3 pointToStartFromDOWN    = pointToStartFrom - dirUp * offset;
                        //Debug.DrawRay(pointToStartFromRIGHT , -impactNormal, Color.blue, 1f);
                        //Debug.DrawRay(pointToStartFromLEFT  , -impactNormal, Color.blue, 1f);
                        //Debug.DrawRay(pointToStartFromUP    , -impactNormal, Color.blue, 1f);
                        //Debug.DrawRay(pointToStartFromDOWN  , -impactNormal, Color.blue, 1f);
                        ////take each one of these and see if they return also the "same-ish" normal

                        CheckGround();//??

                        AddWall(collision.gameObject, impactNormal);


                        RecalculateNormal();
                    }
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
            currentNormal = wallAndGround[wallAndGround.Count - 1].lastNormal;
        }
    }

    private void CheckGround()
    {
        RaycastHit info;
        if (Physics.Raycast(this.transform.position, -currentNormal, out info, raycastDist, layerMask.value))
        {
            currentNormal = info.normal;
            Debug.DrawRay(this.transform.position, -currentNormal.normalized * raycastDist, Color.red);
        }
        else
        {
            RecalculateNormal();
            Debug.DrawRay(this.transform.position, -currentNormal.normalized * raycastDist, new Color(1, 0, 1));
        }
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
