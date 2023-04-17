using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpTiming : MonoBehaviour
{
    public Transform cube60;
    public Transform cube30;
    public Transform cube10;
    public Transform cubeRand;
    
    public float globalSpeedOfSlowDown = 0.9f;

    public bool reset = false;
    // Start is called before the first frame update
    void Start()
    {
        Reset();
    }

    void Reset()
    {
        StartCoroutine(RunCube(cube60, 60));
        StartCoroutine(RunCube(cube30, 30));
        StartCoroutine(RunCube(cube10, 10));
        cubeRand.transform.localPosition = new Vector3(0, 10, -3);
    }

    IEnumerator RunCube(Transform cube, int frameRate)
    {
        int frameCounter = 0;

        float yPos = 10;
        cube.transform.localPosition = new Vector3(0, yPos, frameRate/10);
        Vector3 pos = cube.transform.localPosition;
        while (true)
        {
            yield return new WaitForSeconds(1f / frameRate);
            frameCounter++;
            //La formule ici :
            float deltaTime = (1f / frameRate);
            yPos = yPos * Mathf.Pow(globalSpeedOfSlowDown, deltaTime);

            pos.y = yPos;
            cube.transform.localPosition = pos;



            //if (frameRate == 10)
            //    Debug.Log("10 Update : " + yPos + " at " + Time.time + "         frameCounter = " + frameCounter + " deltaTime = " + deltaTime);
            //if (frameRate == 30 && frameCounter % 3 == 0)                                                       
            //    Debug.Log("30 Update : " + yPos + " at " + Time.time + "         frameCounter = " + frameCounter + " deltaTime = " + deltaTime);
            //if (frameRate == 60 && frameCounter % 6 == 0)                                                        
            //    Debug.Log("60 Update : " + yPos + " at " + Time.time + "         frameCounter = " + frameCounter + " deltaTime = " + deltaTime);
            //

        }
    }


    // Update is called once per frame
    void Update()
    {

        //La formule ici :
        float yPos = cubeRand.transform.localPosition.y;
        yPos = yPos * Mathf.Pow(globalSpeedOfSlowDown, Time.deltaTime);

        Vector3 pos = cubeRand.transform.localPosition;
        pos.y = yPos;
        cubeRand.transform.localPosition = pos;


        if (reset)
        {
            reset = false;

            StopAllCoroutines();
            Reset();
        }
    }

    //si ça marche pas :

    //

    //public static float LerpValueFromSpeed(float speed, int frameRate = 60)
    //{
    //    speed = Mathf.Clamp01(speed);
    //    if (speed == 1)
    //        return 1;
    //    return 1 - Mathf.Pow(1 - speed, Time.deltaTime * frameRate);
    //}
}
