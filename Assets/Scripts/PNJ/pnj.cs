using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pnj : MonoBehaviour
{

    public bool playerOnReach;

    public Dialog dialog;
    public List<Transform> cameraPoints = new List<Transform>();


    public List<ItemReaction> reactions = new List<ItemReaction>();
    public Dialog defaultReaction_FalseGive;
    public Dialog defaultReaction;

    [System.Serializable]
    public class ItemReaction
    {
        public itemID itemToReactFrom = itemID.none;

        public bool finalTarget = false;//can give, it take
        public Dialog responseGive; //most of the time, just redirect after a line when not final target
        public Dialog responseShow;
    }

    public bool ReturnUpdate()
    {
        //Better if take care in a "control manager" and compare to "dialogManager" too
        if (!playerOnReach)
            return false;
        if(!GameManager.instance.playerMove.talking)
        {
            Talk();
            return true;
        }
        return false;
    }

    public void Talk()
    {
        GameManager.instance.dialogMng.StartDialog(dialog, this);

        if (cameraPoints != null && cameraPoints.Count != 0)
            GameManager.instance.cameraMng.SetSecondaryTarget(cameraPoints[0]);
    }

}
