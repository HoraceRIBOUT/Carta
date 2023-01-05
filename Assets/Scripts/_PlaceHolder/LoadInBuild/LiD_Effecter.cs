using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiD_Effecter : MonoBehaviour
{
    public LiD_Loader loader;

    public float strenght = 1.5f;
    public float jumpForce = 1.5f;
    public float speed = 1.5f;

    public TMPro.TMP_Text text;

    public void LoadButton()
    {
        loader.Load();

        text.SetText(
            "strenght = " + strenght  +
            "\njumpForce" + jumpForce +
            "\nspeed"     + speed
            );
    }
}
