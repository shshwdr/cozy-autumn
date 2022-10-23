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
            var info = AchievementManager.Instance.getInfo(type);
            var icon = Resources.Load<Sprite>("achievement/" + type);
            if (!icon)
            {
                //Debug.LogError("no icon for " + achievement.type);
                icon = Resources.Load<Sprite>("achievement/" + AchievementManager.Instance.getInfo(type).category);
            }
            image.sprite = icon;
            title.text = info.title;
            desc.text = info.description;
        }
    }

    public void initGuide(string type, bool isLocked)
    {
        if (isLocked)
        {
            lockedGo.SetActive(true);
        }
        else
        {

            lockedGo.SetActive(false);
            var info = CellManager.Instance.getInfo(type);
            image.sprite = Resources.Load<Sprite>("cell/" + type);
            title.text = info.displayName;
            desc.text = info.desc;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
