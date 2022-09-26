using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutOutVariable : MonoBehaviour
{
    [SerializeField]
    private Transform targetObject;//the character (or any other target)

    private Camera cam;
    public LayerMask decorMask;

    public List<GameObject> objectTouch = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        cam = GameManager.instance.cameraMng.mainCamera;
    }

    // Update is called once per frame
    void Update()
    {
        Reposition();




        Vector2 cutoutPosition = cam.WorldToViewportPoint(targetObject.position);
        cutoutPosition.y /= ((float)Screen.width / (float)Screen.height);

        for (int i = 0; i < objectTouch.Count; i++)
        {
            Material[] mats = objectTouch[i].GetComponent<Renderer>().materials;

            foreach(Material mat in mats)
            {
                mat.SetVector("_CutoutPosition", cutoutPosition);
            }
        }
    }

    public void Reposition()
    {
        this.transform.position = (targetObject.position + cam.transform.position) / 2;
        this.transform.LookAt(cam.transform);
    }

    public void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Touch " + other.name + " layer " + other.gameObject.layer);
        if (other.gameObject.layer != 6)
            return;

        if (!objectTouch.Contains(other.gameObject))
        {
            Material[] mats = other.gameObject.GetComponent<Renderer>().materials;

            foreach (Material mat in mats)
            {
                mat.SetFloat("_Open", 1);
            }

            objectTouch.Add(other.gameObject);
        }
    }
    public void OnTriggerExit(Collider other)
    {
        //Debug.Log("Touch " + other.name + " layer " + other.gameObject.layer);
        if (other.gameObject.layer != 6)
            return;

        if (objectTouch.Contains(other.gameObject))
        {
            Material[] mats = other.gameObject.GetComponent<Renderer>().materials;

            foreach (Material mat in mats)
            {
                mat.SetFloat("_Open", 0);
            }

            objectTouch.Remove(other.gameObject);
        }

    }
}
