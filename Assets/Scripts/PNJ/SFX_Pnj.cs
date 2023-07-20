using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX_Pnj : MonoBehaviour
{
    public float idleVolume = 1f;
    public float talkVolume = .5f;
    public float victoryVolume = 0f;

    [Space]
    public float changeSpeed = 1f;

    private float currentTargetVolume = 1f;
    private Coroutine victoryCorout = null;

    public AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        if (source == null)
            source = this.GetComponent<AudioSource>();
        currentTargetVolume = idleVolume;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        source.volume = Mathf.Lerp(source.volume, (victoryCorout == null ? currentTargetVolume : victoryVolume), Time.deltaTime * changeSpeed);
    }

    public void StartDialog()
    {
        Debug.Log("Start dialog = "+talkVolume);
        currentTargetVolume = talkVolume;
    }
    public void FinishDialog()
    {
        Debug.Log("Finish dialog = " + (victoryCorout == null?" back to idle : " + idleVolume:" victory is playin'."));
        currentTargetVolume = idleVolume;
    }

    public void Victory(float timeOfVictory)
    {
        victoryCorout = StartCoroutine(VictoryTiming(timeOfVictory));
    }
    public IEnumerator VictoryTiming(float victoryTime)
    {
        Debug.Log("Start vcitory. for "+ victoryTime);
        yield return new WaitForSeconds(victoryTime);
        Debug.Log("Finish vcitory. ");
        victoryCorout = null;
    }

}
