using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddHPCell : GridCell
{
    public GameObject destory;

    public override void init(string _type, Vector2 i, int _amount)
    {

        collider.enabled = false;
        amount = 0;
        switch (_type)
        {
            case "destroy":
                destory.SetActive(true);
                amountLabel.gameObject.SetActive(false);
                break;
            case "addHP":
                destory.SetActive(false);
                addAmount(_amount);
                break;


        }


    }

    public override void addAmount(int a)
    {
        amount += a;
        amountLabel.text = (amount > 0 ? "+" : "-") + Mathf.Abs(amount);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public override void Update()
    {
        
    }
}
