using Doozy.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementCell : MonoBehaviour
{
    public Image image;
    public Text title;
    public Text desc;
    public GameObject lockedGo;
    // Start is called before the first frame update
    public void init(string type,bool isLocked)
    {
        if (isLocked)
        {
            lockedGo.SetActive(true);
        }
        else
        {

            lockedGo.SetActive(false);
            var info = E12PopupManagerScript.Instance.getInfo(type);
            image.sprite = Resources.Load<Sprite>("achievement/" + type);
            title.text = info.title;
            desc.text = info.description;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
