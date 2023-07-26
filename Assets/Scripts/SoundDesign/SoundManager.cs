using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Music and ambient")]
    [SerializeField] private AudioSource ambientSea;
    [SerializeField] private AudioSource ambientMount;
    [Range(0,1)]
    [SerializeField] private float volumeAmbient = 1;

    [SerializeField] private AudioSource musicEggs;
    [SerializeField] private AudioSource musicBass;
    [SerializeField] private AudioSource musicCongo;
    [SerializeField] private AudioSource musicKalimba;
    [Range(0, 1)]
    [SerializeField] private float volumeMusique = 1;


    [Range(0, 1)]
    [SerializeField] private float mountOrSeaLerp = 0;
    [SerializeField] private Vector2 seaX = new Vector2(0,10), seaZ = new Vector2(0, 20);

    [SerializeField] private float silencierFromDialog = 0.6f;
    [SerializeField] private float inDialog = 0f;


    public void Update()
    {
        Vector3 playerPos = GameManager.instance.playerMove.transform.position;

        float seaXdist = Mathf.InverseLerp(seaX.x, seaX.y, playerPos.x);
        float seaZdist = Mathf.InverseLerp(seaZ.x, seaZ.y, playerPos.z);

        mountOrSeaLerp = Mathf.Max(seaXdist, seaZdist);

        inDialog = Mathf.Lerp(inDialog, (GameManager.instance.dialogMng.inDialog ? 1 : 0), Time.deltaTime);

        ambientSea  .volume = (    (mountOrSeaLerp * mountOrSeaLerp)) - silencierFromDialog * inDialog;
        ambientMount.volume = (1 - (mountOrSeaLerp * mountOrSeaLerp)) - silencierFromDialog * inDialog;
                                                                                            
                                                                                            
        //if possible, lower that when seaX rise to high                                    
        musicEggs   .volume =                                       1 - silencierFromDialog * inDialog;
        musicBass   .volume = (1 - (mountOrSeaLerp * mountOrSeaLerp)) - silencierFromDialog * inDialog;
        musicCongo  .volume = (1 - (mountOrSeaLerp * mountOrSeaLerp)) - silencierFromDialog * inDialog;
        musicKalimba.volume = (    (mountOrSeaLerp * mountOrSeaLerp)) - silencierFromDialog * inDialog;
    }


    [Header("UI fx")]
    [SerializeField] private AudioSource startTalk;
    [SerializeField] private AudioSource nextTalk;
    [SerializeField] private AudioSource buttonYes;
    [SerializeField] private AudioSource buttonNo;
    [SerializeField] private AudioSource getItem;
    [SerializeField] private AudioSource suspens_wait;
    [SerializeField] private AudioSource suspens_win;
    [SerializeField] private List<AudioClip> suspens_win_clip;
    [SerializeField] private AudioSource suspens_loose;

    public void StartTalk()
    {
        startTalk.PlayOneShot(startTalk.clip);
    }
    public void NextTalk()
    {
        if(nextTalk != null)
            nextTalk.PlayOneShot(nextTalk.clip);
    }
    public void ButtonYes()
    {
        if (buttonYes != null)
            buttonYes.PlayOneShot(buttonYes.clip);
    }
    public void ButtonNo()
    {
        if (buttonNo != null)
            buttonNo.PlayOneShot(buttonNo.clip);
    }
    public void GetItem()
    {
        getItem.PlayOneShot(getItem.clip);
    }
    public void Suspens_Wait()
    {
        suspens_wait.PlayOneShot(suspens_wait.clip);
    }
    public float Suspens_Win()
    {
        int random = Random.Range(0, suspens_win_clip.Count);
        if (random == suspens_win_clip.Count)
            random = suspens_win_clip.Count - 1;
        suspens_win.PlayOneShot(suspens_win_clip[random]);
        return suspens_win_clip[random].length;
    }
    public void Suspens_Loose()
    {
        suspens_loose.PlayOneShot(suspens_loose.clip);
    }


    [Header("UI fx")]
    [SerializeField] private AudioSource hurtWall;
    [SerializeField] private AudioSource hurtGround_soft;
    [SerializeField] private AudioSource hurtGround_hard;
    [SerializeField] private float hurtGroundVol = 0.4f;
    public void HitWall()
    {
        hurtWall.PlayOneShot(hurtWall.clip);
    }
    public void HurtGround(float power)
    {
        if (power < 0.05f)
            return; //to avoid little inaudible hit

        //hurtGround_soft.volume = (1 - (power)    ) * hurtGroundVol;
        //hurtGround_hard.volume = ((power * power)) * hurtGroundVol;

        //hurtGround_soft.PlayOneShot(hurtGround_soft.clip);
        //hurtGround_hard.PlayOneShot(hurtGround_hard.clip);


        //For now :
        hurtGround_soft.volume = 1 - (1-power)*(1-power); //get quickly more sound
        hurtGround_soft.PlayOneShot(hurtGround_soft.clip);
    }
}
