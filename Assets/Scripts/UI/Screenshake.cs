using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screenshake : MonoBehaviour
{
    [Range(0,1)]
    public float screenshakeIntensity = 0f;
    public float diminishSpeed = 1f;

    public Vector2 maxRangeXY = new Vector2(0.2f,0.2f);
    public float memX, memY;


        
    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Shake();


        if (screenshakeIntensity > 0)
            screenshakeIntensity = Mathf.Clamp01(screenshakeIntensity - Time.deltaTime * diminishSpeed);

    }
    [Range(0, 1)]
    public float smootherRand = 0.75f;
    void Shake()
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);
        x = Mathf.Lerp(memX, x, smootherRand);
        y = Mathf.Lerp(memY, y, smootherRand);
        memX = x;
        memY = y;

        this.transform.localPosition = new Vector3(
            x * screenshakeIntensity * screenshakeIntensity * maxRangeXY.x * Screen.width,
            y * screenshakeIntensity * screenshakeIntensity * maxRangeXY.y * Screen.height,
            0);
    }

    public void AddScreenshake(float addValue)
    {
        screenshakeIntensity += addValue;
    }
}
