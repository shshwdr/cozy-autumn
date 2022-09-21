using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RuleMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    public void showMenu()
    {

        int i = 0; 
        GetComponentInChildren< ScrollRect>().verticalNormalizedPosition =1f;
        var hintCells = GetComponentsInChildren<HintCell>();

        for (; i < RuleManager.Instance.visitedList.Count; i++)
        {
            var ruleCell = hintCells[i];
            StartCoroutine( ruleCell.init(RuleManager.Instance.visitedList[i]));
        }
        for (; i < RuleManager.Instance.unvisitedList.Count; i++)
        {
            var ruleCell = hintCells[i];
            StartCoroutine(ruleCell.init(RuleManager.Instance.unvisitedList[i],true));
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
