using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera mainCamera;
    public Transform target = null;
    //[Range(0, 1)]
    //public float clampSpeed = 0.9f;

    public Vector2 rotationSpeed = Vector2.one;

    public bool blockXRotation = false;

    [Header("CameraPlacement")]
    [Header("   PlayerCam")]
    [Tooltip("X = up / Y = Mid / Z = down")]
    public Vector3 camRadius = new Vector3(-5, -10, -6);
    public Vector3 camHeight = new Vector3(4, 0, -3);
    public Vector3 camRot = new Vector3(-20, -5, 30);
    public List<Transform> circles;
    public List<Transform> cameraVirtuals;
    public Transform falseCamera;
    public Transform playerCamPoint;

    [Header("   Secondary cam")]
    public bool onSecondary = false;
    [Range(0,1)]
    public float lerpSecondaryTarget = 0;
    public float lerpSecondaryTargetSpeed_In = 2;
    public float lerpSecondaryTargetSpeed_Out = 4;
    public AnimationCurve lerpCurve;
    public Transform currentSecondaryTarget;

    [Header("   Thirdaries cam")]
    public bool onThirdaries = false;
    public float lerpThirdaryTarget = 0;
    public float lerpThirdaryTargetSpeed_In = 2;
    public float lerpThirdaryTargetSpeed_Out = 4;
    public Transform secondCamPointSave;
    public Transform currentThirdaryTarget;

    [Header("Transition")]
    public AnimationCurve transitionXCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve transitionYCurve = AnimationCurve.Linear(-1, -1, 1, 1);
    public AnimationCurve transitionRotCurve = AnimationCurve.Linear(-1, -1, 1, 1);
    public AnimationCurve transitionFalseCamRotCurve = AnimationCurve.Linear(-1, -1, 1, 1);

    [Header("Distance from player")]
    [Sirenix.OdinInspector.ReadOnly] public float distanceCurrent = 1f;//it's a lerp value
    [Sirenix.OdinInspector.ReadOnly] public float distanceTarget = 1f; //it's in "meter"
    public float distance_min = 0.2f;
    public float distance_LerpSpeed = 0.2f;
    private float zoomTarget = 1;
    private float zoomCurrent = 1;
    private float zoomDelay = 1f;
    List<Vector3> directions = new List<Vector3>();

    [Range(0, 1)]
    public float lastXAxisValue = 0.5f;
    [Range(-1, 1)]
    public float lastYAxisValue = 0;


    // Start is called before the first frame update
    void Start()
    {
        ReplaceCameraFromRadius();

        if(playerCamPoint == null)
        {
            playerCamPoint = Instantiate(new GameObject(), this.transform).transform;
            playerCamPoint.name = "PlayerCam Point";
        }
        if (secondCamPointSave == null)
        {
            secondCamPointSave = Instantiate(new GameObject(), this.transform).transform;
            secondCamPointSave.name = "SecondCam SavePoint";
        }

        directions = new List<Vector3>();
        directions.Add(new Vector3(0, 0, 1));
        directions.Add(new Vector3(0, 0, -1));
        directions.Add(new Vector3(1, 0, 0));
        directions.Add(new Vector3(-1, 0, 0));
        directions.Add(new Vector3(0, 1, 0));
        directions.Add(new Vector3(0, -1, 0));
    }

    // Update is called once per frame
    void Update()
    {
        GlobalPlacement();


        InputManagement();
        SecondaryManagement();
        UpdateCamPosition();
    }

    public void GlobalPlacement()
    {
        //float timedClampSpeed = clampSpeed * Time.deltaTime * staticValue.defaultFramerate;
        this.transform.position = target.transform.position;
                                 //  (1 - timedClampSpeed ) * this.transform.position 
                                 //+ (timedClampSpeed     ) * target.transform.position;
    }

    public void InputManagement()
    {
//        Debug.Log("Pitch = "+ Input.GetAxis("Mouse X") + " Rool == "+ Input.GetAxis("Mouse Y"));
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 joystickMove = new Vector2(
                Input.GetAxis("Joystick X"),
                -Input.GetAxis("Joystick Y")
            );
            Vector2 mouseMove = new Vector2(
                Input.GetAxis("Mouse X"),
                -Input.GetAxis("Mouse Y")
            );
            //TO DO : Probably need to add a lerp to help if use by joystick. So might use two separate term
            //joystickMove
            Vector2 currentInput = (mouseMove.magnitude > joystickMove.magnitude ? mouseMove : joystickMove);


            lastXAxisValue += rotationSpeed.x * currentInput.x;
            lastXAxisValue = (lastXAxisValue + 1) % 1; //sometimes, modulo doesn't work on negative value, so, adding one clear this.
            lastYAxisValue += rotationSpeed.y * currentInput.y;
            lastYAxisValue = Mathf.Clamp(lastYAxisValue, -1, 1);

            //ok value inputed, rotate it
            UpdatePlayerCamPosition(lastXAxisValue, lastYAxisValue);
        }
    }

    public void UpdatePlayerCamPosition(float xAxis, float yAxis)
    {
        xAxis = xAxis % 1;//to overcome overflow 
        xAxis -= 0.5f;
        foreach (Transform trans in circles)
        {
            trans.localRotation = Quaternion.Euler(0, 360 * xAxis, 0);
        }
        Transform secondCam = cameraVirtuals[2];
        if (yAxis < 0)//mean that we are going between midCam and downCam and also need to take the inverse.
        {
            secondCam = cameraVirtuals[0];
            yAxis = -yAxis;
        }
        //        Debug.Log("yAxis = " + yAxis);
        Vector3 transitionPos;
        transitionPos = Vector3.Lerp(cameraVirtuals[1].position, secondCam.position, transitionXCurve.Evaluate(yAxis));
        transitionPos.y = Vector3.Lerp(cameraVirtuals[1].position, secondCam.position, transitionYCurve.Evaluate(yAxis)).y;
        playerCamPoint.transform.position = transitionPos;
        playerCamPoint.transform.rotation = Quaternion.Lerp(cameraVirtuals[1].rotation, secondCam.rotation, transitionRotCurve.Evaluate(yAxis));

        //falseCamera.transform.position = mainCamera.transform.position;
        //the camera need to be rotate a little higher than the true one, to avoid some wall problem  (if this fix work)
        //falseCamera.transform.rotation = ;
    }

    public void SecondaryManagement()
    {
        if (onSecondary)
        { 
            if (lerpSecondaryTarget < 1)
                lerpSecondaryTarget += Time.deltaTime * lerpSecondaryTargetSpeed_In;
        }
        else
        { 
            if (lerpSecondaryTarget > 0)
                lerpSecondaryTarget -= Time.deltaTime * lerpSecondaryTargetSpeed_Out;
        }
        lerpSecondaryTargetSpeed_In = Mathf.Clamp01(lerpSecondaryTargetSpeed_In);


        if (onThirdaries)
        {
            if (lerpThirdaryTarget < 1)
                lerpThirdaryTarget += Time.deltaTime * lerpThirdaryTargetSpeed_In;
        }
        else
        {
            if (lerpThirdaryTarget > 0)
                lerpThirdaryTarget -= Time.deltaTime * lerpThirdaryTargetSpeed_Out;
        }
        lerpThirdaryTarget = Mathf.Clamp01(lerpThirdaryTarget);
    }

    public void SetSecondaryTarget(Transform secondaryPoint, bool directTP = false)
    {
        //if currentSecondaryTarget != null : how to react???
        currentSecondaryTarget = secondaryPoint;
        if (directTP)
            lerpSecondaryTarget = 1;
        onSecondary = true;
    }
    public void UnSetSecondaryTarget()
    {
        onSecondary = false;
    }

    public void SetThirdariesTarget(Transform thirdPoint, bool directTP = false)
    {
        secondCamPointSave.transform.position = mainCamera.transform.position;
        secondCamPointSave.transform.rotation = mainCamera.transform.rotation;
        currentThirdaryTarget = thirdPoint;

        if (directTP)
            lerpThirdaryTarget = 1;
        else
            lerpThirdaryTarget = 0;

        onThirdaries = true;
    }
    public void UnSetThirdariesTarget()
    {
        secondCamPointSave.transform.position = mainCamera.transform.position;
        secondCamPointSave.transform.rotation = mainCamera.transform.rotation;
        lerpThirdaryTarget = 1;
        currentThirdaryTarget = null;

        onThirdaries = false;
    }

    public LayerMask layerMask;
    public void UpdateCamPosition()
    {
        zoomCurrent = Mathf.Lerp(zoomCurrent, zoomTarget, 1 - Mathf.Pow(2, -Time.deltaTime * zoomDelay));
        mainCamera.fieldOfView = zoomCurrent * 60f;

        //Replace playerCamPoint from distance

        Vector3 ray = playerCamPoint.position - target.transform.position;
        float raycastDist = ray.magnitude;
        RaycastHit info;
        //Debug.DrawRay(target.transform.position, ray, Color.yellow, 5f);
        if (Physics.Raycast(target.transform.position, ray, out info, raycastDist, layerMask.value))
        {
            distanceTarget = Mathf.Clamp(info.distance, distance_min, raycastDist);
            distanceTarget /= raycastDist; //to have a nice lerp
            //ratio to maxDist to have the right distance value
            //distance_min
        }
        else
        {
            distanceTarget = 1;
        }

        float meanDist = SurroundingMeanDistance(raycastDist);
        Debug.Log("MeanDist = " + meanDist + " so i t make : " + raycastDist + " / " + (meanDist / raycastDist));

        distanceCurrent = Mathf.Lerp(distanceCurrent, distanceTarget, Time.deltaTime * distance_LerpSpeed);

        Vector3 playerPosLerped = Vector3.Lerp(target.transform.position, playerCamPoint.transform.position, distanceCurrent);
        if (currentSecondaryTarget == null)
        {
            mainCamera.transform.position = playerPosLerped;
            mainCamera.transform.rotation = playerCamPoint.transform.rotation;
            return;
        }

        mainCamera.transform.position =     Vector3.Lerp(playerPosLerped                  , currentSecondaryTarget.transform.position, lerpCurve.Evaluate(lerpSecondaryTarget));
        mainCamera.transform.rotation =  Quaternion.Lerp(playerCamPoint.transform.rotation, currentSecondaryTarget.transform.rotation, lerpCurve.Evaluate(lerpSecondaryTarget));

        if (onThirdaries)
        {
            mainCamera.transform.position =     Vector3.Lerp(secondCamPointSave.transform.position, currentThirdaryTarget.transform.position, lerpCurve.Evaluate(lerpThirdaryTarget));
            mainCamera.transform.rotation =  Quaternion.Lerp(secondCamPointSave.transform.rotation, currentThirdaryTarget.transform.rotation, lerpCurve.Evaluate(lerpThirdaryTarget));
        }
        else
        {
            mainCamera.transform.position =     Vector3.Lerp(mainCamera.transform.position, secondCamPointSave.transform.position, lerpCurve.Evaluate(lerpThirdaryTarget));
            mainCamera.transform.rotation =  Quaternion.Lerp(mainCamera.transform.rotation, secondCamPointSave.transform.rotation, lerpCurve.Evaluate(lerpThirdaryTarget));
        }
    }

    float SurroundingMeanDistance(float raycastDist)
    {
        float sum = 0;
        float min = 100;
        float max = -1;

        float localDistance = 0;
        foreach (Vector3 direction in directions)
        {
            RaycastHit info;
            //Debug.DrawRay(target.transform.position, ray, Color.yellow, 5f);
            if (Physics.Raycast(target.transform.position, direction, out info, raycastDist, layerMask.value))
            {
                localDistance = Mathf.Clamp(info.distance, distance_min, raycastDist);
                sum += localDistance;
            }
            else
            {
                localDistance = raycastDist;
                sum += localDistance;
            }

            if (min > localDistance)
                min = localDistance;
            if (max < localDistance)
                max = localDistance;
        }
        //we get rid of the two extremum 
        sum -= min;
        sum -= max;

        return sum / 4;
    }


    public void ReplaceCameraFromRadius()
    {
        for (int i = 0; i < 3; i++)
        {
            cameraVirtuals[i].localPosition = new Vector3(0, 0, camRadius[i]);
            cameraVirtuals[i].localRotation = Quaternion.Euler(camRot[i], 0, 0);
            circles[i].localPosition = new Vector3(0, camHeight[i], 0);
        }
    }
    
    public void ZoomCamera(float zoomValue, float zoomSpeed = 1)
    {
        zoomTarget = zoomValue;
        zoomDelay = zoomSpeed;
    }



    public Camera GetCurrentCamera()
    {
        return mainCamera;
    }
}
