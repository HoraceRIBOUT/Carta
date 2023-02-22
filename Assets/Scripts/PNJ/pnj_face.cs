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
        eyesRenderer.sprite = eyesSprite[i];
        dbg_eyes = i;
    }
    public void ChangeMouth(int i)
    {
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



    int EyeLenght()
    {
        return eyesSprite.Count - 1;
    }
    int MouthLenght()
    {
        return mouthSprite.Count - 1;
    }
}
