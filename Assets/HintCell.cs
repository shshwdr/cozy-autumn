using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class HintCell : MonoBehaviour
{
    public GameObject hintImagePrefab;
    public GameObject hintTextPrefab;
    public Transform string1;
    public Transform string2;
    private void Start()
    {
        //init("playerToNut");
    }
    //public void init(string type)
    //{

    //    StartCoroutine(test(type));
    //    //Canvas.ForceUpdateCanvases();
    //    //  equipRenderer.sprite = Resources.Load<Sprite>("cell/" + equipment);
    //}

    public IEnumerator init(string type)
    {
        yield return new WaitForSecondsRealtime(0.1f);
        var ruleInfo = RuleManager.Instance.getInfo(type);
        var splitResult1 = Regex.Split(ruleInfo.words1, @"(?<=[\[\]])");

        for (int i = 0; i < splitResult1.Length; i++)
        {
            var text = splitResult1[i];
            if(text.Length == 0)
            {
                continue;
            }
            if (text[text.Length - 1] == '[')
            {

                text = text.Remove(text.Length - 1);
                if (text.Length > 0)
                {
                    addText(string1, text);
                }
                i++;
                var image = splitResult1[i].Remove(splitResult1[i].Length - 1);
                addImage(string1, image);
            }
            else
            {

                if (text[text.Length - 1] == ']')
                {
                    text = text.Remove(text.Length - 1);
                }
                addText(string1, text);
            }
        }

        addText(string2, ruleInfo.words2);
        //string1.gameObject.SetActive(false); 
        Canvas.ForceUpdateCanvases();
        if (this && gameObject)
        {

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            string1.GetComponent<HorizontalLayoutGroup>().enabled = false;
            yield return new WaitForSeconds(0.1f);
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            // string1.gameObject.SetActive(true);
            string1.GetComponent<HorizontalLayoutGroup>().enabled = true;
        }
    }
    void addImage(Transform parent, string type)
    {

        var text2 = Instantiate(hintImagePrefab, parent);
        text2.GetComponent<Image>().sprite = Resources.Load<Sprite>("cell/" + type);
    }
    void addText(Transform parent,string text)
    {

        var text2 = Instantiate(hintTextPrefab, parent);
        text2.GetComponent<Text>().text = text;
    }
}
