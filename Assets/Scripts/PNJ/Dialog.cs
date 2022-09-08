using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialog", menuName = "Carta/Dialog", order = 1)]
public class Dialog : ScriptableObject
{
    public List<Step.Step> allSteps;

    public pnj pnj_link = null; //if not null, it's a pnj's dialog
    public Color defaultColor;
}

namespace Step
{
    [System.Serializable]
    public abstract class Step_father {}

    [System.Serializable]
    public class Step_Dialog : Step_father
    {
        [TextArea]
        public string text = "";

        public Color color_override;
        public int cameraIndex;
    }

    [System.Serializable]
    public class Step_Camera : Step_father
    {
        public int cameraIndex;
    }

    [System.Serializable]
    public class Step_AddItem: Step_father
    {
        public string itemId; //for now, like that. It's ugly, I know
    }

    [System.Serializable]
    public class Step_RemItem : Step_father
    {
        public string itemId; //for now, like that. It's ugly, I know
    }

    [System.Serializable]
    public class Step_SFX : Step_father
    {
        public AudioClip sfxToPlay;
    }


    [System.Serializable]
    public class Step_Music : Step_father
    {
        public AudioClip musicToPlay;
    }


}

