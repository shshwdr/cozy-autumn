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
    // Start is called before the first frame update
    void Start()
    {

        nameLabel.text = charName;
        playerSelection.sprite = Resources.Load<Sprite>("cell/" + charName);
        updateState();
        EventPool.OptIn("unlockStage", updateState);
    }


    void updateState()
    {
        GetComponent<Button>().interactable =  CharacterManager.Instance.hasUnlocked(charName);
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
