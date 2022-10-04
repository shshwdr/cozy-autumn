using System;
using System.Collections;
using System.Collections.Generic;
using Doozy.Engine.UI;
using UnityEngine;

//namespace Doozy.Examples
//{
public class RulePopupManager : Singleton<RulePopupManager>
{
    [Header("Popup Settings")]
    public string PopupName = "AchievementPopup";

    //[Header("Achievements")]
    //public List<AchievementData> Achievements = new List<AchievementData>();
    Canvas mainCanvas;
    public GameObject popupPrefab;
    bool isShowing;
    HashSet<string> visited = new HashSet<string>();
    Queue<string> ruleQueues = new Queue<string>();
    public void showRule(string type)
    {
        getCanvas();
        if (visited.Contains(type))
        {
            return;
        }
        if (!RuleManager.Instance.unvisitedList.Contains(type))
        {
            Debug.LogError("wrong rule " + type);
            return;
        }
        visited.Add(type);
        ruleQueues.Enqueue(type);
        RuleManager.Instance.addRule(type);
    }

    void showRuleInternally(string type)
    {
        var go = Instantiate(popupPrefab, mainCanvas.transform);
        StartCoroutine(go.GetComponentInChildren<HintCell>().init(type));
        StartCoroutine(closePopup(go));

        SFXManager.Instance.play("unlocknewrule");
    }
    IEnumerator closePopup(GameObject go)
    {
        yield return new WaitForSeconds(5);
        if (go)
        {
            go.GetComponent<UIView>().Hide();
            isShowing = false;

        }
    }

    private void Update()
    {
        if(ruleQueues.Count>0 && !isShowing)
        {
            isShowing = true;
            var pop = ruleQueues.Dequeue();
            showRuleInternally(pop);
        }
    }

    void getCanvas()
    {

        if (!mainCanvas)
        {
            mainCanvas = GameObject.Find("MainCanvas").GetComponent<Canvas>();
            if (!mainCanvas)
            {
                Debug.LogError("no main canvas found!");
                mainCanvas = GetComponent<Canvas>();
            }
        }
    }
}

//[Serializable]
//public class AchievementData
//{
//    public string Achievement;
//    public string Description;
//    public Sprite Icon;
//}
//}