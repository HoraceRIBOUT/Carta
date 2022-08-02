using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CameraVisual : MonoBehaviour
{
#if UNITY_EDITOR
    public CameraManager cameraMng;
    //public PlayerMove charaMng;

    public bool simulateCamPlacement = false;
    

    // Update is called once per frame
    void Update()
    {
        //if (charaMng == null)
        //    charaMng = GameManager.instance.playerMove;


        cameraMng.ReplaceCameraFromRadius();
        for (int i = 0; i < 3; i++)
        {
            DrawCircle(cameraMng.circles[i].position, cameraMng.camRadius[i]);
        }

        //cameraMng.UpdateHouseCutout(charaMng);

        if (simulateCamPlacement)
            cameraMng.UpdateCamPosition(cameraMng.lastXAxisValue, cameraMng.lastYAxisValue);
    }


    void DrawCircle(Vector3 centerPoint, float radius, int numberPoint = 16)
    {
        float Theta = 0f;
        float xVal = radius * Mathf.Cos(Theta);
        float yVal = radius * Mathf.Sin(Theta);


        Vector3 startPoint = centerPoint + new Vector3(xVal, 0, yVal);
        Vector3 previousPoint = startPoint;
        Vector3 currentPoint = startPoint;

        for (int i = 0; i < numberPoint; i++)
        {
            previousPoint = currentPoint;
            if (i == numberPoint - 1)
            {
                currentPoint = startPoint;
            }
            else
            {
                Theta += (2.0f * Mathf.PI * (1f / numberPoint));
                xVal = radius * Mathf.Cos(Theta);
                yVal = radius * Mathf.Sin(Theta);
                currentPoint = centerPoint + new Vector3(xVal, 0, yVal);
            }
            
            Debug.DrawLine(previousPoint, currentPoint, Color.gray);
        }
    }
#endif
}
