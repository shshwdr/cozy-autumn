using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatManager : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ResourceManager.Instance.addResource("nut", 10);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            ResourceManager.Instance.addResource("stick", 10);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            ResourceManager.Instance.addResource("stone", 10);
        }
    }
}
