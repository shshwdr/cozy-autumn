using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCell : MonoBehaviour
{
    public Image icon;
    public Text text;

    public void init(string type, int amount)
    {
        icon.sprite = Resources.Load<Sprite>("resource/" + type);
        text.text = amount.ToString();
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
