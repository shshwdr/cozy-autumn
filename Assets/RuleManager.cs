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
    Dictionary<string, RuleInfo> ruleDict = new Dictionary<string, RuleInfo>();
    // Start is called before the first frame update
    void Start()
    {

        var ruleInfos = CsvUtil.LoadObjects<RuleInfo>("rules");
        foreach(var info in ruleInfos)
        {
            ruleDict[info.type] = info;
        }
    }

    public void addRule(string type)
    {

        visited.Add(type);
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
