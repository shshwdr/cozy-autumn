using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceMenu : MonoBehaviour
{
    ResourceCell[] cells;

    public Transform getCellTransform(string type)
    {
        return cells[0].transform;
        switch (type)
        {
            case "meat":
                return cells[0].transform;
                break;
            case "soul":
                return cells[1].transform;
                break;
            case "metal":
                return cells[2].transform;
                break;

        }
        return transform;

    }

    // Start is called before the first frame update
    void Start()
    {
        cells = GetComponentsInChildren<ResourceCell>();
        updateUI();
        EventPool.OptIn("updateResource", updateUI);
    }

    public void updateUI()
    {

        int i = 0;
        foreach (var resource in ResourceManager.Instance.resourceInfos)
        {
            cells[i].init(resource.type, ResourceManager.Instance.getAmount(resource.type));
            i++;
        }
        //for (; i < cells.Length; i++)
        //{
        //    cells[i].gameObject.SetActive(false);
        //}
    }

    // Update is called once per frame
    void Update()
    {

    }
}
