using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectionCell : MonoBehaviour
{
    public string charName;
    public Text nameLabel;
    // Start is called before the first frame update
    void Start()
    {

        nameLabel.text = charName;
    }
    public void onClick()
    {
        CharacterManager.Instance.setCurrentChar(charName);
        GameManager.Instance.restartGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
