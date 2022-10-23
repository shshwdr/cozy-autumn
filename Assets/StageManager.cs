using Pool;
using Sinbad;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageInfo
{
    public string stageName;
    public string displayName;
    public string leafName;
    public int stopRound;
    public string unlockByAchievement;
}

public class StageManager : Singleton<StageManager>
{
    public Transform bks;
    public string currentStage = "bearForest";
    Dictionary<string, StageInfo> stageInfoDict = new Dictionary<string, StageInfo>();
    List<string> unlockedStage = new List<string>();
    List<string> lockedStage = new List<string>();

    // Start is called before the first frame update
    void Start()
    {

        var combinationInfos = CsvUtil.LoadObjects<StageInfo>("stage");
        foreach (var info in combinationInfos)
        {
            stageInfoDict[info.stageName] = info;
            if(info.unlockByAchievement == null || info.unlockByAchievement == "")
            {
                unlockedStage.Add(info.stageName);
            }
            else
            {
                lockedStage.Add(info.stageName);
            }
        }
        EventPool.OptIn("unlockAchievement",updateStageUnlock);
    }

    public void reopt()
    {

        EventPool.OptIn("unlockAchievement", updateStageUnlock);
    }
    public bool hasUnlocked(string str)
    {
        return unlockedStage.Contains(str);
    }
    void updateStageUnlock()
    {
        foreach(var stage in new List<string>( lockedStage))
        {
            var stageInfo = getStageInfo(stage);
            if (AchievementManager.Instance.hasUnlocked(stageInfo.unlockByAchievement))
            {
                lockedStage.Remove(stage);
                unlockedStage.Add(stage);
                EventPool.Trigger("unlockStage");
            }
        }
    }

    public void setCurrentStage(string na)
    {
        currentStage = na;
        
        for (int i = 0; i < bks.childCount; i++)
        {
            bks.GetChild(i).gameObject.SetActive(false);
        }
        var stageBK = bks.Find(na);
        if (!stageBK)
        {
            stageBK = bks.Find("bearForest");
        }
        stageBK.gameObject.SetActive(true);
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
