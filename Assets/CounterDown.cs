using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CounterDown : MonoBehaviour
{
    public Text text;
    int count = 0;
    public void initCount(int x)
    {
        count = x;
        updateText();
    }

    public void countDown()
    {
        count -= 1;
        updateText();

        
    }

    
    void updateText()
    {
        if(text == null)
        {
            Debug.LogError("no text");
            return;
        }
        text.text = count.ToString();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
