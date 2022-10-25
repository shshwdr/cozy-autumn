using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectionCell : MonoBehaviour
{
    public string charName;
    public Text nameLabel;
    public Image currentPlayer;
    public Image playerSelection;
    public Sprite lockedSprite;
    // Start is called before the first frame update
    void Start()
    {

        nameLabel.text = charName;
        //playerSelection.sprite = Resources.Load<Sprite>("cell/" + charName);
        updateState();
        EventPool.OptIn("unlockStage", updateState);
    }


    void updateState()
    {
        bool hasUnlocked = CharacterManager.Instance.hasUnlocked(charName);
        GetComponent<Button>().interactable = hasUnlocked;
        playerSelection.sprite = hasUnlocked?Resources.Load<Sprite>("cell/" + charName):lockedSprite;
    }
    public void onClick()
    {
        CharacterManager.Instance.setCurrentChar(charName);
        currentPlayer.sprite = Resources.Load<Sprite>("cell/" + CharacterManager.Instance.currentChar);
        //GameManager.Instance.restartGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
