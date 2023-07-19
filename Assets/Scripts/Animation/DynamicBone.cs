using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicBone : MonoBehaviour
{
    [Header("Chain")]
    public List<DynamicChain> chains = new List<DynamicChain>();
    [System.Serializable]
    public class DynamicChain
    {
        public bool enable = true;
        public Transform startingBone;
        public float rotationMax = 60f;
        [Range(0,1)]
        public float rotationSuplex = 0f;
        [HideInInspector] public Quaternion startQuaternion;

        public Vector3 eulerMin = new Vector3(-360, -360, -360);
        public Vector3 eulerMax = new Vector3(361, 361, 361);
    }

    [Header("Movement info")]
    public Vector3 lastPosition = new Vector3();

    public Vector3 targetPoint = new Vector3();
    public Vector3 currentPoint = new Vector3();
    public Vector3 acceleration = new Vector3();
    public Vector3 speed = new Vector3();

    [Header("Tweak value")]
    public float speedEffect = 0.1f;
    public float accSlowing = 5f;
    public float recoverSlowing = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        lastPosition = this.transform.position;
        targetPoint = this.transform.position + Vector3.up;
        currentPoint = targetPoint;
        foreach (DynamicChain chain in chains)
        {
            chain.startQuaternion = chain.startingBone.localRotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        targetPoint = this.transform.position + Vector3.up;
        acceleration = (targetPoint - currentPoint) * speedEffect;

        acceleration -= acceleration * Time.deltaTime * accSlowing;
        speed += acceleration * Time.deltaTime;
        
        currentPoint += speed * Time.deltaTime;

        Vector3 rotPoint = currentPoint - targetPoint;
        currentPoint -= rotPoint * Mathf.Min(1, Time.deltaTime * recoverSlowing);
        if (rotPoint.magnitude > 1)
        {
            currentPoint = targetPoint + rotPoint.normalized;
            rotPoint.Normalize();
            speed = Vector3.zero;
        }

        foreach (DynamicChain chain in chains)
        {
            Quaternion rotation = Quaternion.Euler(
                chain.eulerMin.x == 0 && chain.eulerMax.x == 0 ? this.transform.rotation.eulerAngles.x : Mathf.Clamp(rotPoint.z * chain.rotationMax, chain.eulerMin.x, chain.eulerMax.x),
                this.transform.rotation.eulerAngles.y,
                chain.eulerMin.z == 0 && chain.eulerMax.z == 0 ? this.transform.rotation.eulerAngles.z : Mathf.Clamp(rotPoint.x * chain.rotationMax, chain.eulerMin.z, chain.eulerMax.z));
            //Debug.Log("y = "+ this.transform.rotation.eulerAngles.y);
            chain.startingBone.localRotation = rotation * chain.startQuaternion;
            RecursiveRotation(rotation, chain.startingBone, chain.rotationSuplex);
        }

        lastPosition = this.transform.position;
    }


    public void RecursiveRotation(Quaternion rotation, Transform parent, float supplex)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).localRotation = Quaternion.Lerp(Quaternion.identity, rotation, supplex);
            RecursiveRotation(rotation, parent.GetChild(i), supplex * supplex);
        }
    }
}
