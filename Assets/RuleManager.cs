using Pool;
using Sinbad;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuleInfo {
    public string type;
    public string words1; 
    public string words2;

}


public class RuleManager : Singleton<RuleManager>
{
    HashSet<string> visited = new HashSet<string>();
    public List<string> visitedList = new List<string>();
    public List<string> unvisitedList = new List<string>();
    Dictionary<string, RuleInfo> ruleDict = new Dictionary<string, RuleInfo>();
    public List<RuleInfo> ruleList = new List<RuleInfo>();
    // Start is called before the first frame update
    void Start()
    {

        ruleList = CsvUtil.LoadObjects<RuleInfo>("rules");
        foreach(var info in ruleList)
        {
            unvisitedList.Add(info.type);
            ruleDict[info.type] = info;
        }
    }
    public void unlockAll()
    {
        foreach(var rule in ruleList)
        {
            addRule(rule.type);
        }
    }
    public void addRule(string type)
    {
        if (!visited.Contains(type))
        {
            visitedList.Add(type);
            unvisitedList.Remove(type);
            EventPool.Trigger("unlockRule");
        }
        visited.Add(type);
        
        if (visited.Count == ruleDict.Count)
        {

            FindObjectOfType<AchievementManager>().ShowAchievement("allRules");
        }
    }

    public bool isUnlocked(string type)
    {
        return visited.Contains(type);
    }
    public RuleInfo getInfo(string type)
    {

        if (!ruleDict.ContainsKey(type))
        {
            Debug.LogError("no key for " + type);
        }
        return ruleDict[type];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
