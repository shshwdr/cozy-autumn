using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipMenu : MonoBehaviour
{
    public float tipsShowTime = 5f;
    float currentTime = 0f;
    public Transform parent;
    int index;

    List<string> tips = new List<string>()
        {
            "Moving [player] cost 1 [nut]",
            "Try moving [branch] onto a [nut]!",
            "[snake] steals 1 [nut] per day",
            "You can move [snake] onto the trap",
            "When there can be a new rule, \nthere will be an alert shown on a card",
            "Furnitures can stay when you restart game",
            "Check rules book for the rules you found",
            "The leaves outside window change\naccording to the season",
            "Let me know what you think of the game~",

        };
    // Start is called before the first frame update
    void Start()
    {
        updateDescription();
    }

    void updateDescription()
    {
        Utils.destroyAllChildren(parent);
        HintCell.generateHintText(parent,"Tips: "+ tips[index]);

        StartCoroutine(test());
    }

    IEnumerator test()
    {
        //yield return new WaitForSecondsRealtime(0.1f);
        if (this && gameObject)
        {

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            foreach (var h in GetComponentsInChildren<HorizontalLayoutGroup>())
            {
                h.enabled = false;
            }
            //  string1.GetComponent<HorizontalLayoutGroup>().enabled = false;
            yield return new WaitForSecondsRealtime(0.01f);
            Canvas.ForceUpdateCanvases();
            if (this && gameObject)
            {

                LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
                foreach (var h in GetComponentsInChildren<HorizontalLayoutGroup>())
                {
                    h.enabled = true;
                }
                // string1.gameObject.SetActive(true);
                //string1.GetComponent<HorizontalLayoutGroup>().enabled = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.unscaledDeltaTime;
        if(currentTime>= tipsShowTime)
        {
            onClick();
        }
    }

    public void onClick()
    {
        index++;
        if (index >= tips.Count)
        {
            index = 0;
        }
        currentTime = 0;
        updateDescription();
    }
}
