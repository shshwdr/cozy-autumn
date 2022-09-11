using Sinbad;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinationInfo
{
    public string type1;
    public string type2;
    public Dictionary<string,string> result;
}



public class CombinationManager : Singleton<CombinationManager>
{
    Dictionary<string, List<CombinationInfo>> combinationDict = new Dictionary<string, List<CombinationInfo>>();
    // Start is called before the first frame update
    void Start()
    {

        var combinationInfos = CsvUtil.LoadObjects<CombinationInfo>("combination");

        foreach(var info in combinationInfos)
        {
            if (!combinationDict.ContainsKey(info.type1))
            {
                combinationDict[info.type1] = new List<CombinationInfo>();
            }
            combinationDict[info.type1].Add(info);

        }
    }

    public CombinationInfo getCombinationResult(string type1,string type2)
    {
        if (!combinationDict.ContainsKey(type1))
        {
            return null; 
        }
        foreach(var info in combinationDict[type1])
        {
            if(info.type2 == type2)
            {
                return info;
            }
        }
        return null;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
