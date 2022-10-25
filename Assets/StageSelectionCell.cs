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
        EventPool.OptIn("unlockStage", updateState);
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
        GameManager.Instance.restartGame();
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}
