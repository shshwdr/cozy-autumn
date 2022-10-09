using Sinbad;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageInfo
{
    public string stageName;
    public string displayName;
    public string leafName;

}

public class StageManager : Singleton<StageManager>
{

    public string currentStage = "bearForest";
    Dictionary<string, StageInfo> stageInfoDict = new Dictionary<string, StageInfo>();

    public void setCurrentStage(string na)
    {
        currentStage = na;
    }
    // Start is called before the first frame update
    void Start()
    {

        var combinationInfos = CsvUtil.LoadObjects<StageInfo>("stage");
        foreach (var info in combinationInfos)
        {
            stageInfoDict[info.stageName] = info;
        }
    }

    public StageInfo getStageInfo(string name)
    {
        if (!stageInfoDict.ContainsKey(name))
        {
            Debug.LogError("no stage " + name);
        }
        return stageInfoDict[name];
    }

    public StageInfo getCurrentInfo()
    {
        return getStageInfo(currentStage);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
