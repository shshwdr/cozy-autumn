using Doozy.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuideBookMenu : MonoBehaviour
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
        for (; i < NewCellManager.Instance.visitedList.Count; i++)
        {
            var ruleCell = hintCells[i];
            ruleCell.initGuide(NewCellManager.Instance.visitedList[i], false);
            ruleCell.gameObject.SetActive(true);
        }
        for (; i < NewCellManager.Instance.unvisitedList.Count + NewCellManager.Instance.visitedList.Count; i++)
        {
            var ruleCell = hintCells[i];
            ruleCell.initGuide(NewCellManager.Instance.unvisitedList[i - NewCellManager.Instance.visitedList.Count], true);
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
