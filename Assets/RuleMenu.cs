using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RuleMenu : MonoBehaviour
{
    HintCell[] hintCells;
    // Start is called before the first frame update
    void Start()
    {
    }

    public void showMenu()
    {

        int i = 0; 
        //if (hintCells == null)
        {

            hintCells = GetComponentsInChildren<HintCell>(true);
        }
        GetComponentInChildren< ScrollRect>().verticalNormalizedPosition =1f;

        for (; i < RuleManager.Instance.visitedList.Count; i++)
        {
            var ruleCell = hintCells[i];
            StartCoroutine( ruleCell.init(RuleManager.Instance.visitedList[i]));
            ruleCell.gameObject.SetActive(true);
        }
        for (; i < RuleManager.Instance.unvisitedList.Count + RuleManager.Instance.visitedList.Count; i++)
        {
            var ruleCell = hintCells[i];
            StartCoroutine(ruleCell.init(RuleManager.Instance.unvisitedList[i - RuleManager.Instance.visitedList.Count],true));
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
