using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CreationCloud : MonoBehaviour
{
    public Texture2D texture = null;
    public Vector2 resolution = new Vector2(100,100);

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
    public void ResetNoise()
    {
        List<float> firstTexture = MakeANoiseText(zoneNumber1, perlinOffset1);


        for (int i = 0; i < resolution.x; i++)
        {
            for (int j = 0; j < resolution.y; j++)
            {
                texture.SetPixel(i, j, firstTexture[i*(int)resolution.y + j] * Color.white);
            }
        }


        // Apply all SetPixel calls
        texture.Apply();
    }


    public List<float> MakeANoiseText(Vector2 zoneNumber, Vector2 perlinOffset)
    {
        List<float> valuePerPoint = new List<float>();
        List<Vector2> pointPerZone = new List<Vector2>(); ;

        //Make the point
        System.Text.StringBuilder stringbuild = new System.Text.StringBuilder();
        Vector2 zoneSize = new Vector2(1f / zoneNumber.x, 1f / zoneNumber.y);
        float zoneSizeMax = (zoneSize.magnitude);
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

                float randX = Mathf.PerlinNoise(i * perlinOffset.x, j * perlinOffset.y);
                float randY = Mathf.PerlinNoise(i * perlinOffset.x, j * perlinOffset.y);

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
        for (int i = 0; i < resolution.x; i++)
        {
            Vector2 currentPos = pixelSize.x * i * Vector2.right;
            for (int j = 0; j < resolution.y; j++)
            {
                currentPos.y = pixelSize.y * j;
                float distance = 1;
                //Search nearest point
                foreach (Vector2 point in pointPerZone)
                {
                    distance = Mathf.Min(distance, (point - currentPos).magnitude);
                }
                distance /= zoneSizeMax;


                //Draw distance to that point
                valuePerPoint.Add(distance);
            }
        }


        return valuePerPoint;
    }


    public bool reloadTexture = false;
    public void Update()
    {
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
