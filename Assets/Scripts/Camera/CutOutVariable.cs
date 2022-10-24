using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutOutVariable : MonoBehaviour
{
    [SerializeField]
    private Transform targetObject;//the character (or any other target)

    [SerializeField]
    private Camera cam;

    public List<Material> materialToChange = new List<Material>();

    // Start is called before the first frame update
    void Start()
    {
        cam = GameManager.instance.cameraMng.mainCamera;
    }

    // Update is called once per frame
    void Update()
    {
        SetDistanceAndCutoutPos();
    }

    [Sirenix.OdinInspector.Button()]
    public void SetDistanceAndCutoutPos()
    {
        float distance = (targetObject.transform.position - cam.transform.position).magnitude;

        Vector2 cutoutPosition = cam.WorldToViewportPoint(targetObject.position);
        cutoutPosition.y /= ((float)Screen.width / (float)Screen.height);

        foreach (Material mat in materialToChange)
        {
            mat.SetVector("_CutoutPosition", cutoutPosition);
            mat.SetFloat("_DistanceToPlayer", distance);
        }
    }

#if UNITY_EDITOR
    public void OnDestroy()
    {
        foreach (Material mat in materialToChange)
        {
            mat.SetVector("_CutoutPosition", Vector2.one/2);
            mat.SetFloat("_DistanceToPlayer", .5f);//to have fully shown block
        }

        UnityEditor.AssetDatabase.Refresh();
    }
#endif

}
