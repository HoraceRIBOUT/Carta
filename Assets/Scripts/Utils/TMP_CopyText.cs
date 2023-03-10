using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TMP_CopyText : MonoBehaviour
{
    public TMPro.TMP_Text copier;
    public TMPro.TMP_Text copied;

    // Update is called once per frame
    void Update()
    {
        if(copier.text != copied.text)
            copier.SetText(copied.text);
    }
}
