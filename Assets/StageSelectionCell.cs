using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectionCell : MonoBehaviour
{
    public string stageName;
    public Text nameLabel;
    public Sprite lockedSprite;
    Sprite originalSprite;
    // Start is called before the first frame update
    void Start()
    {

        nameLabel.text = stageName;

        originalSprite = GetComponent<Image>().sprite;
        updateState();
        updateButton();
        EventPool.OptIn("unlockStage", updateState);
    }

    public void updateButton()
    {
        if (StageManager.Instance.currentStage == "")
        {
            StageManager.Instance.currentStage = "bearForest";
        }
        if(stageName == StageManager.Instance.currentStage)
        {
            transform.parent.localScale = Vector3.one * 1.3f;
        }
        else
        {

            transform.parent.localScale = Vector3.one;
        }
    }

    void updateState()
    {
        bool hasUnlocked = StageManager.Instance.hasUnlocked(stageName);
        GetComponent<Button>().interactable = hasUnlocked;
        GetComponent<Image>().sprite = hasUnlocked ? originalSprite : lockedSprite;
       // GetComponent<Button>().interactable = StageManager.Instance.hasUnlocked(stageName);
    }

    public void onClick()
    {


        StageManager.Instance.setCurrentStage(stageName);


        foreach (var b in transform.parent.parent.GetComponentsInChildren<StageSelectionCell>())
        {
            //if (b != this)
            {
                b.updateButton();
            }
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}
