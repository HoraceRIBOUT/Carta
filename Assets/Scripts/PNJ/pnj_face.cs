using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class pnj_face : MonoBehaviour
{
    public SpriteRenderer eyesRenderer;
    public List<Sprite> eyesSprite;
    public SpriteRenderer mouthRenderer;
    public List<Sprite> mouthSprite;

    public SpriteRenderer accesories; //like the food/drink in Simon Sez hand (Jack Hadi)
    public List<Sprite> accesSprite;

    [PropertyRange(0, "EyeLenght")]  [OnValueChanged("ChangeFace")]
    public int dbg_eyes = 0;
    [PropertyRange(0, "MouthLenght")][OnValueChanged("ChangeFace")]
    public int dbg_mouth = 0;

    [Header("Wink")]
    public List<int> winkIndexes = new List<int>();
    public Vector2 winkCadence = new Vector2(3,10);
    public float winkFrameRate = 0.033f;
    public float winkCloseDuration = 0f;
    bool winking = false;

    private void OnEnable()
    {
        if(Application.isPlaying)
            StartCoroutine(Wink_Coroutine());
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    //For test only
    [Button()]
    private void ChangeFace()
    {
        ChangeFace(dbg_eyes, dbg_mouth);
    }
    public void ChangeFace(int eyeIndex, int mouthIndex)
    {
        ChangeEyes(eyeIndex);
        ChangeMouth(mouthIndex);
    }

    public void ChangeEyes(int i)
    {
        if (eyesRenderer == null)
            return;

        if(!winking)
            eyesRenderer.sprite = eyesSprite[i];
        dbg_eyes = i;
    }
    public void ChangeMouth(int i)
    {
        if (mouthRenderer == null)
            return;

        mouthRenderer.sprite = mouthSprite[i];
        dbg_mouth = i;
    }

    public int dbg_acess = 0;
    private void ChangeAccessories()
    {
        ChangeAccessories(dbg_acess);
    }
    public void ChangeAccessories(int i)
    {
        accesories.sprite = accesSprite[i];
        dbg_acess = i;
    }



    private IEnumerator Wink_Coroutine()
    {
        if(winkIndexes.Count == 0)
            yield return 0; //early exit

        //So : 
        while (this.isActiveAndEnabled)
        {
            winking = false;
            yield return new WaitForSeconds(Random.Range(winkCadence.x,winkCadence.y));
            winking = true;
            //Then, wink !
            int startIndex = 0;
            if (winkIndexes.Contains(dbg_eyes))
                startIndex = winkIndexes.IndexOf(dbg_eyes);
            int currentWinkIndex = startIndex;
            //Open
            while (currentWinkIndex < winkIndexes.Count)
            {
                eyesRenderer.sprite = eyesSprite[winkIndexes[currentWinkIndex]];
                yield return new WaitForSeconds(winkFrameRate);
                currentWinkIndex++;
            }
            currentWinkIndex = winkIndexes.Count - 1;
            yield return new WaitForSeconds(winkCloseDuration);
            //Close
            while (currentWinkIndex >= startIndex)
            {
                eyesRenderer.sprite = eyesSprite[winkIndexes[currentWinkIndex]];
                yield return new WaitForSeconds(winkFrameRate);
                currentWinkIndex--;
            }
            eyesRenderer.sprite = eyesSprite[dbg_eyes];//not nescesarry the same as the start of the coroutine
        }
    }





    int EyeLenght()
    {
        if (eyesSprite.Count == 0) return 0;
        return eyesSprite.Count - 1;
    }
    int MouthLenght()
    {
        if (mouthSprite.Count == 0) return 0;
        return mouthSprite.Count - 1;
    }
}
