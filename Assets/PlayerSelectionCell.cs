using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectionCell : MonoBehaviour
{
    public string charName;
    public Text nameLabel;
    public Image currentPlayer;
    // Start is called before the first frame update
    void Start()
    {

        nameLabel.text = charName;
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
