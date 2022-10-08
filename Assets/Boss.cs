using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    public int count = 0;
    public Text amountLabel;
    public Text attackLabel;
    public int amount;
    public int attack;
    // Start is called before the first frame update
    public virtual void init(string type)
    {
        EventPool.Trigger("startBossFight");
        var info = CellManager.Instance.getInfo(type);
        attack = info.attack;
        attackLabel.text = attack.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public virtual IEnumerator onNextStep() {
        yield return null;
    }

    public virtual void getKilled() { }


    public void updateAmount()
    {
        if (amountLabel == null)
        {
            return;
        }
        if (amount > 1)
        {
            amountLabel.text = amount.ToString();
        }
        else
        {
            amountLabel.text = "";
        }
    }

    public void setAmount(int x)
    {
        amount = x;
        updateAmount();
    }
}
