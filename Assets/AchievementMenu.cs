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
        for (; i < E12PopupManagerScript.Instance.visitedList.Count; i++)
        {
            var ruleCell = hintCells[i];
            ruleCell.init(E12PopupManagerScript.Instance.visitedList[i],false);
            ruleCell.gameObject.SetActive(true);
        }
        for (; i < E12PopupManagerScript.Instance.unvisitedList.Count+ E12PopupManagerScript.Instance.visitedList.Count; i++)
        {
            var ruleCell = hintCells[i];
           ruleCell.init(E12PopupManagerScript.Instance.unvisitedList[i - E12PopupManagerScript.Instance.visitedList.Count], true);
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
