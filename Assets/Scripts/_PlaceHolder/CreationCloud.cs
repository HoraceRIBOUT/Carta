using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CreationCloud : MonoBehaviour
{
    public Texture2D texture = null;
    public Vector2 resolution = new Vector2(100,100);

    public GameObject sphereToDebug;
    public List<GameObject> instantiateGO = new List<GameObject>();

    [Range(0, 1)]
    public float depht = 0;
    public float dephtSpeed = 1f;
    private float virtualDepth = 0;

    public AnimationCurve rand;


    [Header("Visual")]
    public Vector3 centerPos = new Vector3(-1.6f,1.5f,-4.2f);
    public Vector3 centerSize = Vector3.one;



    void Start()
    {
        // Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
        texture = new Texture2D((int)resolution.x, (int)resolution.y, TextureFormat.ARGB32, false);

        // set the pixel values
        ResetNoise();
        UpdateTexture();

        // connect texture to material of GameObject this script is attached to
        if (Application.isPlaying)
            GetComponent<Renderer>().material.mainTexture = texture;
        else
            GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
    }

    

    public int zoneNumber1 = 10;
    List<Vector3> pointPerZone1 = new List<Vector3>();
    public Vector2 perlinOffset1 = new Vector2(1.564f, 129.94f);
    [Space]
    public bool secondText = false;
    public int zoneNumber2 =10;
    List<Vector3> pointPerZone2 = new List<Vector3>();
    public Vector2 perlinOffset2 = new Vector2(1.564f, 129.94f);

    public bool addANoise = false;
    public Vector2 noiseSize = Vector2.one;
    public AnimationCurve noiseCurve = new AnimationCurve();
    public void ResetNoise()
    {
        pointPerZone1 = MakeANoiseText(zoneNumber1, perlinOffset1);
        pointPerZone2 = MakeANoiseText(zoneNumber2, perlinOffset2);

        for (int i = 0; i < instantiateGO.Count; i++)
        {
            if(instantiateGO[i] != null)
                DestroyImmediate(instantiateGO[i].gameObject);
        }
        instantiateGO.Clear();

        for (int i = 0; i < pointPerZone1.Count; i++)
        {
            instantiateGO.Add(Instantiate(sphereToDebug, centerPos - (centerSize/2f) + (pointPerZone1[i]), Quaternion.identity, this.transform));
        }

    }
    public void UpdateTexture()
    {
        virtualDepth = depht * dephtSpeed;

        List<Color> firstTexture = TakeASlice(zoneNumber1, pointPerZone1, 0);
        List<Color> secondTexture = TakeASlice(zoneNumber2, pointPerZone2, 0);


        for (int i = 0; i < resolution.x; i++)
        {
            for (int j = 0; j < resolution.y; j++)
            {
                Color
                    val = firstTexture[i * (int)resolution.y + j];
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


    public List<Vector3> MakeANoiseText(int zoneNumber, Vector2 perlinOffset)
    {
        List<Vector3> pointPerZone = new List<Vector3>();

        rand = new AnimationCurve();

        //Make the point
        System.Text.StringBuilder stringbuild = new System.Text.StringBuilder();
        Vector2 zoneSize = new Vector2(1f / zoneNumber, 1f / zoneNumber);
        for (int i = 0; i < zoneNumber; i++)
        {
            float zoneXMin = zoneSize.x * i;
            float zoneXMax = zoneSize.x * (i + 1);
            if (i + 1 == zoneNumber)
                zoneXMax = 1;//normally

            for (int j = 0; j < zoneNumber; j++)
            {
                float zoneYMin = zoneSize.y * j;
                float zoneYMax = zoneSize.y * (j + 1);
                if (j + 1 == zoneNumber)
                    zoneYMax = 1;//normally

                float randX =
                    Random.Range(0f, 1f);
                    //Mathf.PerlinNoise(virtualDepth + i + .25f + perlinOffset.x, virtualDepth + .33f + j + perlinOffset.y * perlinOffset.x);
                float randY =
                    Random.Range(0f, 1f);
                    //Mathf.PerlinNoise(virtualDepth + i + .75f + perlinOffset.y * perlinOffset.x, virtualDepth + .87f + j + perlinOffset.x);

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

        return pointPerZone;
    }

    public List<Color> TakeASlice(int zoneNumber, List<Vector3> pointPerZone, float depth)
    {
        List<float> valuePerPoint = new List<float>();


        //For each pixel : 
        Vector2 pixelSize = new Vector2(1f / resolution.x, 1f / resolution.y);
        Vector2 zoneSize = new Vector2(1f / zoneNumber, 1f / zoneNumber);
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
                pointPerZone_Voisins.Add(pointPerZone[zoneI * zoneNumber + zoneJ]);

                for (int k = -1; k < 2; k++)
                {
                    int zoneK = zoneI + k;
                    for (int l = -1; l < 2; l++)
                    {
                        int zoneL = zoneJ + l;

                        int zoneKAdjust = zoneK < 0 ? zoneNumber - 1
                            : zoneK >= zoneNumber ? 0 : zoneK;
                        int zoneLAdjust = zoneL < 0 ? zoneNumber - 1
                            : zoneL >= zoneNumber ? 0 : zoneL;

                        Vector2 pointPos = pointPerZone[zoneKAdjust * zoneNumber + zoneLAdjust];
                        if (zoneK < 0)
                            pointPos.x = pointPos.x - 1;
                        if (zoneK >= zoneNumber)
                            pointPos.x = pointPos.x + 1;
                        if (zoneL < 0)
                            pointPos.y = pointPos.y - 1;
                        if (zoneL >= zoneNumber)
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
        Debug.Log("distMin and MAx = " + distMinMax);
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
            UpdateTexture();
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
            UpdateTexture();

            reloadTexture = false;
        }


        DrawBox();
    }

    public void DrawBox()
    {
        Vector3 point000 = new Vector3(-1f, -1f, -1f);
        Vector3 point100 = new Vector3( 1f, -1f, -1f);
        Vector3 point010 = new Vector3(-1f,  1f, -1f);
        Vector3 point110 = new Vector3( 1f,  1f, -1f);
                           
        Vector3 point001 = new Vector3(-1f, -1f,  1f);
        Vector3 point101 = new Vector3( 1f, -1f,  1f);
        Vector3 point011 = new Vector3(-1f,  1f,  1f);
        Vector3 point111 = new Vector3( 1f,  1f,  1f);

        point000 = Relocate(point000);
        point100 = Relocate(point100);
        point010 = Relocate(point010);
        point110 = Relocate(point110);
                                    
        point001 = Relocate(point001);
        point101 = Relocate(point101);
        point011 = Relocate(point011);
        point111 = Relocate(point111);


        Debug.DrawLine(point000, point001, Color.black);
        Debug.DrawLine(point000, point010, Color.black);
        Debug.DrawLine(point011, point010, Color.black);
        Debug.DrawLine(point011, point001, Color.black);

        Debug.DrawLine(point100, point101, Color.black);
        Debug.DrawLine(point100, point110, Color.black);
        Debug.DrawLine(point111, point110, Color.black);
        Debug.DrawLine(point111, point101, Color.black);


        Debug.DrawLine(point000, point100, Color.black);
        Debug.DrawLine(point001, point101, Color.black);
        Debug.DrawLine(point010, point110, Color.black);
        Debug.DrawLine(point011, point111, Color.black);

        Vector3 minPos = centerPos;
        minPos.z -= centerSize.z / 2;
        Vector3 maxPos = centerPos;
        maxPos.z += centerSize.z / 2;
        this.transform.position = Vector3.Lerp(minPos, maxPos, depht);
    }

    public Vector3 Relocate(Vector3 point)
    {
        return centerPos + new Vector3(point.x * centerSize.x * 0.5f, point.y * centerSize.y * 0.5f, point.z * centerSize.z * 0.5f);
    }

}
