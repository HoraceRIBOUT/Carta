using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        ReturnUpdate();

        EUpdate();
    }

    void ReturnUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Return))
            GameManager.instance.dialogMng.ReturnUpdate();
    }

    void EUpdate()
    {
        //For now, always, no limit.
        if (Input.GetKeyDown(KeyCode.E))
            GameManager.instance.inventory.EUpdate();
    }
}
