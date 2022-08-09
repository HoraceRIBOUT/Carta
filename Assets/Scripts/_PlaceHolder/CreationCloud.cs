using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CreationCloud : MonoBehaviour
{
    public Texture2D texture = null;
    public Vector2 resolution = new Vector2(100,100);

    [Range(0, 1)]
    public float depht = 0;
    public float dephtSpeed = 1f;
    private float virtualDepth = 0;

    public AnimationCurve rand;
    void Start()
    {
        // Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
        texture = new Texture2D((int)resolution.x, (int)resolution.y, TextureFormat.ARGB32, false);

        // set the pixel values
        ResetNoise();

        // connect texture to material of GameObject this script is attached to
        if (Application.isPlaying)
            GetComponent<Renderer>().material.mainTexture = texture;
        else
            GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
    }

    public Vector2 zoneNumber1 = new Vector2(10f,10f);
    public Vector2 perlinOffset1 = new Vector2(1.564f, 129.94f);
    [Space]
    public bool secondText = false;
    public Vector2 zoneNumber2 = new Vector2(10f,10f);
    public Vector2 perlinOffset2 = new Vector2(1.564f, 129.94f);

    public bool addANoise = false;
    public Vector2 noiseSize = Vector2.one;
    public AnimationCurve noiseCurve = new AnimationCurve();
    public void ResetNoise()
    {
        virtualDepth = depht * dephtSpeed;

        List<Color> firstTexture = MakeANoiseText(zoneNumber1, perlinOffset1);
        List<Color> secondTexture = MakeANoiseText(zoneNumber2, perlinOffset2);


        for (int i = 0; i < resolution.x; i++)
        {
            for (int j = 0; j < resolution.y; j++)
            {
                Color
                    val = Color.black;// firstTexture[i * (int)resolution.y + j];
                if (secondText)
                {
                    val += secondTexture[i * (int)resolution.y + j];
                    val /= 2f;
                }

                if (addANoise)
                {
                    val *= noiseCurve.Evaluate(Mathf.PerlinNoise(resolution.x + noiseSize.x, resolution.y + noiseSize.y));
                }

                texture.SetPixel(i, j, val);
            }
        }


        // Apply all SetPixel calls
        texture.Apply();
    }


    public List<Color> MakeANoiseText(Vector2 zoneNumber, Vector2 perlinOffset)
    {
        List<float> valuePerPoint = new List<float>();
        List<Vector2> pointPerZone = new List<Vector2>(); ;

        rand = new AnimationCurve();

        //Make the point
        System.Text.StringBuilder stringbuild = new System.Text.StringBuilder();
        Vector2 zoneSize = new Vector2(1f / zoneNumber.x, 1f / zoneNumber.y);
        for (int i = 0; i < zoneNumber.x; i++)
        {
            float zoneXMin = zoneSize.x * i;
            float zoneXMax = zoneSize.x * (i + 1);
            if (i + 1 == zoneNumber.x)
                zoneXMax = 1;//normally

            for (int j = 0; j < zoneNumber.y; j++)
            {
                float zoneYMin = zoneSize.y * j;
                float zoneYMax = zoneSize.y * (j + 1);
                if (j + 1 == zoneNumber.y)
                    zoneYMax = 1;//normally

                float randX =
                    //Random.Range(0f, 1f);
                    Mathf.PerlinNoise(virtualDepth + i + .25f + perlinOffset.x, virtualDepth + .33f + j + perlinOffset.y * perlinOffset.x);
                float randY =
                    //Random.Range(0f, 1f);
                    Mathf.PerlinNoise(virtualDepth + i + .75f + perlinOffset.y * perlinOffset.x, virtualDepth + .87f + j + perlinOffset.x);

                rand.AddKey(randX, randY);

                pointPerZone.Add(new Vector2(
                    Mathf.Lerp(zoneXMin, zoneXMax, randX),
                    Mathf.Lerp(zoneYMin, zoneYMax, randY)
                    ));
                stringbuild.Append(pointPerZone[pointPerZone.Count-1] 
                    + "("  + randX 
                    + ", " + randY + ")" + "\n");
            }
        }
        Debug.Log(" So : "+ stringbuild.ToString());

        //For each pixel : 
        Vector2 pixelSize = new Vector2(1f / resolution.x, 1f / resolution.y);
        Vector2 distMinMax = new Vector2(1, 0);
        for (int i = 0; i < resolution.x; i++)
        {
            Vector2 currentPos = pixelSize.x * i * Vector2.right;
            for (int j = 0; j < resolution.y; j++)
            {
                currentPos.y = pixelSize.y * j;
                float distance = 1;

                //Only calcul for the point next to you
                List<Vector2> pointPerZone_Voisins = new List<Vector2>();

                //search for center 
                int zoneI = (int)(currentPos.x / zoneSize.x);
                int zoneJ = (int)(currentPos.y / zoneSize.y);
                pointPerZone_Voisins.Add(pointPerZone[zoneI * (int)zoneNumber.y + zoneJ]);

                for (int k = -1; k < 2; k++)
                {
                    int zoneK = zoneI + k;
                    for (int l = -1; l < 2; l++)
                    {
                        int zoneL = zoneJ + l;

                        int zoneKAdjust = zoneK < 0 ? (int)zoneNumber.x - 1 
                            : zoneK >= zoneNumber.x? 0: zoneK;
                        int zoneLAdjust = zoneL < 0 ? (int)zoneNumber.y - 1
                            : zoneL >= zoneNumber.y ? 0: zoneL;

                        Vector2 pointPos = pointPerZone[zoneKAdjust * (int)zoneNumber.y + zoneLAdjust];
                        if (zoneK <  0)
                            pointPos.x = pointPos.x - 1;
                        if (zoneK >= zoneNumber.x)
                            pointPos.x = pointPos.x + 1;
                        if (zoneL <  0)
                            pointPos.y = pointPos.y - 1;
                        if (zoneL >= zoneNumber.y)
                            pointPos.y = pointPos.y + 1;

                        pointPerZone_Voisins.Add(pointPos);
                    }
                }


                foreach (Vector2 point in pointPerZone_Voisins)
                {
                    distance = Mathf.Min(distance, (point - currentPos).magnitude);
                }
                distMinMax.x = Mathf.Min(distMinMax.x, distance);
                distMinMax.y = Mathf.Max(distMinMax.y, distance);



                //Draw distance to that point
                valuePerPoint.Add(distance);
            }
        }
        Debug.Log("distMin and MAx = "+distMinMax);
        List<Color> colorPerPoint = new List<Color>();
        for (int k = 0; k < valuePerPoint.Count; k++)
        {
            colorPerPoint.Add(
                grad.Evaluate(
                (valuePerPoint[k] - distMinMax.x) / distMinMax.y
                )
                );
        }

        return colorPerPoint;
    }

    public Gradient grad;

    public bool reloadTexture = false;
    public void Update()
    {
        if (virtualDepth != dephtSpeed * depht)
        {
            ResetNoise();
        }

        if (reloadTexture)
        {
            if(texture == null)
            {
                // Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
                texture = new Texture2D((int)resolution.x, (int)resolution.y, TextureFormat.ARGB32, false);
                // connect texture to material of GameObject this script is attached to
                GetComponent<Renderer>().material.mainTexture = texture;
            }
            ResetNoise();

            reloadTexture = false;
        }
    }

}
