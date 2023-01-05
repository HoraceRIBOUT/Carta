using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_MaP_IconInfoZone : UI_MaP_IconDropZone
{
    public UI_MaP_Icon iconPrefab;

    [Header("Normally, set by game")]
    public List<pnj.pnjID> pnjToDeploy = new List<pnj.pnjID>();

    [Header("Data")]
    public List<IconData> dataFromPnj;

    [Header("For icon position")]
    [Range( 0, 1)] [Sirenix.OdinInspector.OnValueChanged("ChangeSize")] public float positionX = 0.3f;
    [Range( 0, 1)] [Sirenix.OdinInspector.OnValueChanged("ChangeSize")] public float positionY = 0.3f;
    [Range( 0, 1)] [Sirenix.OdinInspector.OnValueChanged("ChangeSize")] public float marginX   = 0.1f;
    [Range(-1, 0)] [Sirenix.OdinInspector.OnValueChanged("ChangeSize")] public float marginY   =-0.1f;
    [Range(-1, 1)] [Sirenix.OdinInspector.OnValueChanged("ChangeSize")] public float size      = 0.1f;
    Vector2 fullSize;
    public RectTransform dolly;

    public void Start()
    {
    }

    public void ChangeSize()
    {
        if (!Application.isPlaying)
            return;

        for (int i = 0; i < iconsGO.Count; i++)
        {
            Destroy(iconsGO[i].gameObject);
        }
        iconsGO.Clear();
        UpdateIconList();
    }

    [Sirenix.OdinInspector.Button]
    public void UpdateIconList()
    {
        fullSize = dolly.rect.size;

        for (int i = 0; i < pnjToDeploy.Count; i++)
        {
            if (IconAlreadyDeployed(pnjToDeploy[i]))
            {
                continue;
            }

            IconData data = GetDataForThisPJN(pnjToDeploy[i]);
            if (data == null)
                continue;
            UI_MaP_Icon newIcon = Instantiate(iconPrefab);
            newIcon.transform.SetParent(iconParent);
            newIcon.Create(data, true);

            iconsGO.Add(newIcon);
        }
        UpdateIconPosition();
    }

    public void UpdateIconPosition()
    {
        fullSize = dolly.rect.size;

        for (int i = 0; i < iconsGO.Count; i++)
        {
            UI_MaP_Icon iconToReplace = iconsGO[i];
            int xPos = i % 2;
            int yPos = (int)Mathf.Floor(i / 2);
            Debug.Log("Pos = " + xPos + ", " + yPos);
            iconToReplace.himselfRect.localPosition = new Vector3(
                xPos * positionX * fullSize.x + marginX * fullSize.x,
               -yPos * positionY * fullSize.y + marginY * fullSize.y,
                0);
            iconToReplace.transform.localRotation = Quaternion.identity;
            iconToReplace.transform.localScale = Vector3.one;
            iconToReplace.himselfRect.sizeDelta = new Vector2(
                fullSize.x * size,
                fullSize.x * size);
        }
    }

    public bool IconAlreadyDeployed(pnj.pnjID pnjToTest)
    {
        foreach (UI_MaP_Icon ic in iconsGO)
        {
            if(ic.data.id == pnjToTest)
            {
                return true;
            }
        }
        return false;
    }

    public IconData GetDataForThisPJN(pnj.pnjID id)
    {
        foreach (IconData data in dataFromPnj)
        {
            if (data.id == id)
            {
                return data;
            }
        }
        Debug.LogError(id + " is not in the data list. Please, add the IconData scriptable in the list on the ListIcon gameObject.");
        return null;
    }

    public void AddIconIfNeeded(pnj.pnjID iconId)
    {
        if (pnjToDeploy.Contains(iconId))
            return;
        foreach (IconData data in dataFromPnj)
        {
            if(data.id == iconId)
            {
                pnjToDeploy.Add(iconId);
            }
        }
    }

}
