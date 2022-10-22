using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageProgressView : MonoBehaviour
{
    public RectTransform p1;
    public RectTransform p2;
    public RectTransform player;

    public Image playerRender;

    // Start is called before the first frame update
    void Start()
    {

        EventPool.OptIn("updateProgress", updateProgress);
        //EventPool.OptIn("startBossFight", hide);
        playerRender.sprite = Resources.Load<Sprite>("cell/" + CharacterManager.Instance.currentChar);

    }
    void updateProgress() { 
    
        var prog =DeckManager.Instance.getProgress();
        player.transform.position = p1.position* (1 - prog)  + p2.position * prog;
    }
    void hide()
    {
        gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
