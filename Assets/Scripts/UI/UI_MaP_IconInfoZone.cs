using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_MaP_IconInfoZone : UI_MaP_IconDropZone
{
    public UI_MaP_Icon iconPrefab;

    [Header("Normally, set by game")]
    public List<pnj> pnjToDeploy = new List<pnj>();

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

            UI_MaP_Icon newIcon = Instantiate(iconPrefab);
            IconData data = GetDataForThisPJN(pnjToDeploy[i].id);
            newIcon.transform.SetParent(iconParent);
            newIcon.Create(data, true);
            int xPos = iconsGO.Count % 2;
            int yPos = (int)Mathf.Floor(iconsGO.Count / 2);
            Debug.Log("Pos = " + xPos + ", " + yPos);
            newIcon.himselfRect.localPosition = new Vector3(
                xPos * positionX * fullSize.x + marginX * fullSize.x,
               -yPos * positionY * fullSize.y + marginY * fullSize.y,
                0                   );
            newIcon.transform.localRotation = Quaternion.identity;
            newIcon.himselfRect.sizeDelta = new Vector2(
                fullSize.x * size,
                fullSize.x * size);

            iconsGO.Add(newIcon);
        }
    }

    public bool IconAlreadyDeployed(pnj pnjToTest)
    {
        foreach (UI_MaP_Icon ic in iconsGO)
        {
            if(ic.data.id == pnjToTest.id)
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

    public void AddNewIcon()
    {

    }
}
