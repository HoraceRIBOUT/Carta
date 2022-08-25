using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CreationCloud3 : MonoBehaviour
{
    public List<Vector3> pointInSpace;
    public MeshRenderer thePlane;


    [Header("Variable")]
    public int pixelResolution = 200;
    public int numberOfSubsection = 5;

    [Header("Viewer")]
    public float cubeSize = 1;
    [Range(0,1)]
    public float depht = 0;    private float depht_mem = 0;
    public List<GameObject> dbg_pointInSpace;
    public GameObject spherePoint;

    [Space]
    public bool generatePoint = false;
    public bool updateSurface = false;

    // Start is called before the first frame update
    void Start()
    {
        depht_mem = depht;
    }

    // Update is called once per frame
    void Update()
    {
        if (generatePoint)
        {
            GeneratePoint();

            generatePoint = false;
        }

        if (updateSurface || depht_mem != depht)
        {
            UpdateSurface();
            updateSurface = false;
            depht_mem = depht;
        }

        DrawDebug();
    }

    public void GeneratePoint()
    {
        pointInSpace = new List<Vector3>();
        Vector3 randNumb;
        Debug.Log("Generate : ");
        for (int i = 0; i < numberOfSubsection; i++)
        {
            for (int j = 0; j < numberOfSubsection; j++)
            {
                for (int k = 0; k < numberOfSubsection; k++)
                {
                    randNumb = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                    pointInSpace.Add(randNumb);
                }
            }
        }
        Debug.Log("Generated : " + pointInSpace.Count);

    }

    public void UpdateSurface() 
    {
        Debug.Log("Update surface !");
        Texture2D text = 
            Application.isPlaying ? 
                (Texture2D)thePlane.material.mainTexture :
                (Texture2D)thePlane.sharedMaterial.mainTexture;

        if (text == null)
        {
            text = new Texture2D(pixelResolution, pixelResolution, TextureFormat.ARGB32, false);
        }

        List<List<float>> valuePerPoint = GetASlice();

        for (int i = 0; i < pixelResolution; i++)
        {
            for (int j = 0; j < pixelResolution; j++)
            {
                //Calcul pixel color :
                //Calculate the distance to the closest point
                float value = valuePerPoint[i][j];
                //text.SetPixel(i, j, Color.Lerp(Color.white - Color.black, Color.white, value));
                text.SetPixel(i, j, Color.Lerp(Color.black, Color.white, value));
            }
        }

        if(Application.isPlaying)
            thePlane.material.mainTexture = text;
        else 
            thePlane.sharedMaterial.mainTexture = text;
        text.Apply();
        Debug.Log("Finish surface slicing !");
    }

    public List<List<float>> GetASlice()
    {
        List<List<float>> res = new List<List<float>>();

        float maxDistance = 0f;
        for (int i = 0; i < pixelResolution; i++)
        {
            List<float> res_partial = new List<float>();
            for (int j = 0; j < pixelResolution; j++)
            {
                //Calculate the distance to the closest point
                float distanceMin = 100f;
                Vector3 pointPos = new Vector3(
                    (float)i / (float)pixelResolution,
                    (float)j / (float)pixelResolution,
                    depht
                    );
                foreach(Vector3 point in pointInSpace)
                {
                    float dist = (point - pointPos).magnitude;
                    if(distanceMin > dist)
                    {
                        distanceMin = dist;
                    }
                }
                res_partial.Add(distanceMin);

                if (maxDistance < distanceMin)
                    maxDistance = distanceMin;
            }
            res.Add(res_partial);
        }

        for (int i = 0; i < pixelResolution; i++)
            for (int j = 0; j < pixelResolution; j++)
                res[i][j] = res[i][j] / maxDistance;
        
        return res;
    }


    public void DrawDebug()
    {
        Vector3 point000 = new Vector3(0, 0, 0);
        Vector3 point100 = new Vector3(1, 0, 0);
        Vector3 point010 = new Vector3(0, 1, 0);
        Vector3 point110 = new Vector3(1, 1, 0);

        Vector3 point001 = new Vector3(0, 0, 1);
        Vector3 point101 = new Vector3(1, 0, 1);
        Vector3 point011 = new Vector3(0, 1, 1);
        Vector3 point111 = new Vector3(1, 1, 1);



        Debug.DrawLine(pointInWorld(point000), pointInWorld(point010), Color.gray);
        Debug.DrawLine(pointInWorld(point010), pointInWorld(point110), Color.gray);
        Debug.DrawLine(pointInWorld(point110), pointInWorld(point100), Color.gray);
        Debug.DrawLine(pointInWorld(point100), pointInWorld(point000), Color.gray);
                                                                   
        Debug.DrawLine(pointInWorld(point000), pointInWorld(point001), Color.gray);
        Debug.DrawLine(pointInWorld(point010), pointInWorld(point011), Color.gray);
        Debug.DrawLine(pointInWorld(point110), pointInWorld(point111), Color.gray);
        Debug.DrawLine(pointInWorld(point100), pointInWorld(point101), Color.gray);

        Debug.DrawLine(pointInWorld(point001), pointInWorld(point011), Color.gray);
        Debug.DrawLine(pointInWorld(point011), pointInWorld(point111), Color.gray);
        Debug.DrawLine(pointInWorld(point111), pointInWorld(point101), Color.gray);
        Debug.DrawLine(pointInWorld(point101), pointInWorld(point001), Color.gray);

        if(dbg_pointInSpace == null)
            dbg_pointInSpace = new List<GameObject>();

        if (dbg_pointInSpace.Count != pointInSpace.Count)
        {
            //Delete previous bach
            foreach (GameObject gO in dbg_pointInSpace)
            {
                if (gO != null)
                {
                    if (Application.isPlaying)
                        Destroy(gO);
                    else
                        DestroyImmediate(gO);
                }
            }
            dbg_pointInSpace.Clear();

            //Create the point
            for (int i = 0; i < pointInSpace.Count; i++)
            {
                GameObject gO = Instantiate(spherePoint, this.transform);
                gO.name = "Point (" + i + "," + i + "," + i + ")";

                dbg_pointInSpace.Add(gO);
            }

        }
        else
        {
            for (int i = 0; i < pointInSpace.Count; i++)
            {
                Vector3 pos = pointInSpace[i] - Vector3.one / 2f;
                dbg_pointInSpace[i].transform.localPosition = pos;
            }
        }



        

        if(thePlane != null) 
        {
            thePlane.transform.localPosition = (depht - 0.5f) * cubeSize * Vector3.forward;
        }
    }

    public Vector3 pointInWorld(Vector3 pointLocal)
    {
        return this.transform.position
            + (pointLocal.x - 0.5f) * cubeSize * Vector3.right
            + (pointLocal.y - 0.5f) * cubeSize * Vector3.up
            + (pointLocal.z - 0.5f) * cubeSize * Vector3.forward
        ;
    }
}
