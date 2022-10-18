using Sinbad;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinationInfo
{
    public string type1;
    public string type2;
    public Dictionary<string,string> result;
    public string rules;
    public string achievement;

    public int addValue1;
    public int addValue2;
    public int reverse;

}



public class CombinationManager : Singleton<CombinationManager>
{
    Dictionary<string, List<CombinationInfo>> combinationDict = new Dictionary<string, List<CombinationInfo>>();
    Dictionary<string, List<CombinationInfo>> combinationDict2 = new Dictionary<string, List<CombinationInfo>>();
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


            if (!combinationDict2.ContainsKey(info.type2))
            {
                combinationDict2[info.type2] = new List<CombinationInfo>();
            }
            combinationDict2[info.type2].Add(info);

        }
    }

    public int getCombinationAddValue(string resource, string result)
    {
        foreach (var info in combinationDict[resource])
        {
            if (info.result.ContainsKey("generate1")&& info.result["generate1"]== result)
            {
                return info.addValue1;
            }
        }


        foreach (var info in combinationDict2[resource])
        {
            if (info.result.ContainsKey("generate1") && info.result["generate1"] == result)
            {
                return info.addValue2;
            }
        }

        return -1;
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

        if (!combinationDict.ContainsKey(type2))
        {
            return null;
        }
        foreach (var info in combinationDict[type2])
        {
            if(info.reverse == 1 && info.type2 == type1)
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
