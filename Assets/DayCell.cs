using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayCell : MonoBehaviour,CanReset
{
    int day = 300;
    public Text dayText;
    // Start is called before the first frame update
    void Start()
    {
        updateText();
        EventPool.OptIn("moveAStep", moveStep);
    }

    public void Reset()
    {
        // day = 180;
        updateText();
        EventPool.OptIn("moveAStep", moveStep);
    }

    public void updateText()
    {

        dayText.text = ( GridController.Instance.moveCount).ToString() + "\nDay";
    }

    void moveStep()
    {
        //day--;
        updateText();
        if (day - GridController.Instance.moveCount <= 0)
        {
            GameManager.Instance.win();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
