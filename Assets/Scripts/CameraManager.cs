using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera mainCamera;
    public Transform target = null;
    [Range(0, 1)]
    public float clampSpeed = 0.9f;

    public Vector2 rotationSpeed = Vector2.one;

    public bool blockXRotation = false;

    [Header("CameraPlacement")]
    [Tooltip("X = up / Y = Mid / Z = down")]
    public Vector3 camRadius = new Vector3(-5, -10, -6);
    public Vector3 camHeight = new Vector3(4, 0, -3);
    public Vector3 camRot = new Vector3(-20, -5, 30);
    public List<Transform> circles;
    public List<Transform> cameraVirtuals;


    [Range(0, 1)]
    public float lastXAxisValue = 0.5f;
    [Range(-1, 1)]
    public float lastYAxisValue = 0;
    



    // Start is called before the first frame update
    void Start()
    {
        ReplaceCameraFromRadius();
    }

    // Update is called once per frame
    void Update()
    {
        GlobalPlacement();


        InputManagement();
    }

    public void GlobalPlacement()
    {
        float timedClampSpeed = clampSpeed * Time.deltaTime * staticValue.defaultFramerate;
        this.transform.position = target.transform.position;
                                 //  (1 - timedClampSpeed ) * this.transform.position 
                                 //+ (timedClampSpeed     ) * target.transform.position;
    }

    public void InputManagement()
    {
        //if (Input.anyKeyDown)
        //{
        //    foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
        //    {
        //        if (Input.GetKeyDown(vKey))
        //        {
        //            //your code here
        //            Debug.Log("GetKey : " + vKey);
        //        }
        //    }
        //}


//        Debug.Log("Pitch = "+ Input.GetAxis("Mouse X") + " Rool == "+ Input.GetAxis("Mouse Y"));
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 joystickMove = new Vector2(
                Input.GetAxis("Mouse X"),
                -Input.GetAxis("Mouse Y")
            );

            lastXAxisValue += rotationSpeed.x * joystickMove.x;
            lastXAxisValue = lastXAxisValue % 1;
            lastYAxisValue += rotationSpeed.y * joystickMove.y;
            lastYAxisValue = Mathf.Clamp(lastYAxisValue, -1, 1);

            //ok value inputed, rotate it
            UpdateCamPosition(lastXAxisValue, lastYAxisValue);
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Joystick1Button2))
        {
            if (Cursor.lockState == CursorLockMode.Locked) 
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void UpdateCamPosition(float xAxis, float yAxis)
    {
        xAxis = xAxis % 1;//to overcome overflow 
        xAxis -= 0.5f;
        foreach (Transform trans in circles)
        {
            trans.localRotation = Quaternion.Euler(0, 360 * xAxis, 0);
        }
        Transform secondCam = cameraVirtuals[0];
        if (yAxis < 0)//mean that we are going between midCam and downCam and also need to take the inverse.
        {
            secondCam = cameraVirtuals[2];
            yAxis = -yAxis;
        }
        //        Debug.Log("yAxis = " + yAxis);
        mainCamera.transform.position = Vector3.Lerp(cameraVirtuals[1].position, secondCam.position, yAxis);
        mainCamera.transform.rotation = Quaternion.Lerp(cameraVirtuals[1].rotation, secondCam.rotation, yAxis);
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
