using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowNextStep : MonoBehaviour,CanReset
{
    public Image nextImage;
    // Start is called before the first frame update
    void Start()
    {
        updateImage();

        EventPool.OptIn("moveAStep", updateImage);
    }



    void updateImage()
    {
        nextImage .sprite = Resources.Load<Sprite>("cell/" + DeckManager.Instance.peekCard());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Reset()
    {
        updateImage();
    }
}
