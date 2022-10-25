using Doozy.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementMenu : MonoBehaviour
{
    AchievementCell[] hintCells;
    // Start is called before the first frame update
    void Start()
    {
    }

    public void showMenu()
    {

        int i = 0;
        GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1f;
        //if(hintCells== null)
        {

            hintCells = GetComponentsInChildren<AchievementCell>(true);
        }
        for (; i < AchievementManager.Instance.visitedList.Count; i++)
        {
            var index = AchievementManager.Instance.visitedList.Count - i - 1;
            var ruleCell = hintCells[i];
            ruleCell.init(AchievementManager.Instance.visitedList[index],false);
            ruleCell.gameObject.SetActive(true);
        }
        for (; i < AchievementManager.Instance.unvisitedList.Count+ AchievementManager.Instance.visitedList.Count; i++)
        {
            var ruleCell = hintCells[i];
           ruleCell.init(AchievementManager.Instance.unvisitedList[i - AchievementManager.Instance.visitedList.Count], true);
            ruleCell.gameObject.SetActive(true);
        }

        for (; i < hintCells.Length; i++)
        {
            var ruleCell = hintCells[i];
            ruleCell.gameObject.SetActive(false);
        }
    }




    // Update is called once per frame
    void Update()
    {

    }
}
