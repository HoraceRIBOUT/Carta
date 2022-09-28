using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SkyManager : MonoBehaviour
{
    public Material skyMat;

    public bool timeAdvance = false;
    public float timeSpeed = 1f/(5f*60f); // duration of a day : 5 minute (5*60s)

    [Range(0,1)]
    public float timeOfTheDay;
    private float timeOfTheDay_m;
    [Header("Sun")]
    public Transform sunPos;
    public Transform sunPivot;
    public Light light_sun;
    public Light light_moon;
    public Light light_global;

    [Header("Color")]
    public Gradient colorUp;
    public Gradient colorMid;
    public Gradient colorDown;
    public Gradient colorSun;
    public Gradient colorMoon;
    public Gradient colorGlobal;
    public Gradient colorShadow;

    [Header("Lamp")]
    public AnimationCurve lightCurve;
    public List<Light> nightLamp = new List<Light>();

    // Update is called once per frame
    void Update()
    {
        if (timeAdvance && Application.isPlaying)
        {
            timeOfTheDay += Time.deltaTime * timeSpeed;
            if(timeOfTheDay > 1)
            {
                timeOfTheDay--;
            }
            if (timeOfTheDay < 0)
            {
                timeOfTheDay++;
            }
        }
        
        if (timeOfTheDay == timeOfTheDay_m)
            return;
        timeOfTheDay_m = timeOfTheDay;

        sunPivot.transform.rotation = Quaternion.AngleAxis(360*timeOfTheDay, this.transform.forward);

        skyMat.SetVector("_SunPosition", sunPos.transform.position);
        skyMat.SetColor("_Color1", colorUp.Evaluate(timeOfTheDay));
        skyMat.SetColor("_Color2", colorMid.Evaluate(timeOfTheDay));
        skyMat.SetColor("_Color3", colorDown.Evaluate(timeOfTheDay));

        light_sun.color = colorSun.Evaluate(timeOfTheDay);
        light_moon.color = colorMoon.Evaluate(timeOfTheDay);
        light_global.color = colorGlobal.Evaluate(timeOfTheDay);
        RenderSettings.ambientSkyColor = colorShadow.Evaluate(timeOfTheDay);


        float lightIntensity = lightCurve.Evaluate(timeOfTheDay);


        foreach (Light l in nightLamp)
        {
            l.intensity = lightIntensity;
        }
    }
}
