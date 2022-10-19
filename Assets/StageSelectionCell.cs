using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectionCell : MonoBehaviour
{
    public string stageName;
    public Text nameLabel;
    // Start is called before the first frame update
    void Start()
    {

        nameLabel.text = stageName;


        updateState();
        EventPool.OptIn("unlockStage", updateState);
    }


    void updateState()
    {
        GetComponent<Button>().interactable = StageManager.Instance.hasUnlocked(stageName);
    }

    public void onClick()
    {
        StageManager.Instance.setCurrentStage(stageName);
        GameManager.Instance.restartGame();
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}
