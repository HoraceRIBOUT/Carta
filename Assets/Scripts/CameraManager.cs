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
    public Transform currentSecondaryTarget;
    public Transform secondCamPoint;

    [Header("Transition")]
    public AnimationCurve transitionXCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve transitionYCurve = AnimationCurve.Linear(-1, -1, 1, 1);
    public AnimationCurve transitionRotCurve = AnimationCurve.Linear(-1, -1, 1, 1);
    public AnimationCurve transitionFalseCamRotCurve = AnimationCurve.Linear(-1, -1, 1, 1);


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

            lastXAxisValue += rotationSpeed.x * mouseMove.x;
            lastXAxisValue = lastXAxisValue % 1;
            lastYAxisValue += rotationSpeed.y * mouseMove.y;
            lastYAxisValue = Mathf.Clamp(lastYAxisValue, -1, 1);

            //ok value inputed, rotate it
            UpdatePlayerCamPosition(lastXAxisValue, lastYAxisValue);
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Joystick1Button2))
        {
            if (Cursor.lockState == CursorLockMode.Locked) 
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
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
                lerpSecondaryTarget += Time.deltaTime;
            else
                lerpSecondaryTarget = 1;
        }
        else
        {
            if (lerpSecondaryTarget > 0)
                lerpSecondaryTarget -= Time.deltaTime;
            else
                lerpSecondaryTarget = 0;
        }
    }

    public void SetSecondaryTarget(Transform secondaryPoint)
    {
        /*
        if(currentSecondaryTarget != null)
        {
            secondCamPoint.transform.position = mainCamera.transform.position;
            secondCamPoint.transform.rotation = mainCamera.transform.rotation;
              n
        }*/

        currentSecondaryTarget = secondaryPoint;
        onSecondary = true;
    }
    public void UnSetSecondaryTarget()
    {
        onSecondary = false;
    }


    public void UpdateCamPosition()
    {
        mainCamera.transform.position =    Vector3.Lerp(playerCamPoint.transform.position, playerCamPoint.transform.position, lerpSecondaryTarget);
        mainCamera.transform.rotation = Quaternion.Lerp(playerCamPoint.transform.rotation, playerCamPoint.transform.rotation, lerpSecondaryTarget);
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

    public Camera GetCurrentCamera()
    {
        return mainCamera;
    }
}
