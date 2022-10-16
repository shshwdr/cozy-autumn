using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwapCountVIew : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        EventPool.OptIn("updateSwap", updateSwap);
    }

    void updateSwap()
    {
        GetComponent<Text>().text = "Swap: " + (GridGeneration.Instance.swapTime - GridGeneration.Instance.currentSwapTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
