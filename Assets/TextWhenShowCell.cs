using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextWhenShowCell : Singleton<TextWhenShowCell>
{
    Text text;
    private void Awake()
    {
        text = GetComponent<Text>();
    }
    public void showText(string t)
    {
        text.text = t;
    }
    public void hideText()
    {
        text.text = "";
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
