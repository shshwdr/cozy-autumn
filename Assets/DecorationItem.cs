using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationItem : MonoBehaviour
{
   public  bool shouldHide = true;
    // Start is called before the first frame update
    void Start()
    {
        if (shouldHide)
        {

            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
