using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayCell : MonoBehaviour
{
    int day = 180;
    public Text dayText;
    // Start is called before the first frame update
    void Start()
    {
        dayText.text = day.ToString() + " Day";
        EventPool.OptIn("moveAStep", moveStep);
    }

    public void Reset()
    {
        day = 180;

        dayText.text = day.ToString() + " Day";
        EventPool.OptIn("moveAStep", moveStep);
    }

    void moveStep()
    {
        day--;
        dayText.text = day.ToString() +" Day";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
