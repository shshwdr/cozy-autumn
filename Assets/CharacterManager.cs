using Pool;
using Sinbad;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CharacterInfo
{
    public string name;
    public string displayName;
    public string unlockAchievement;
    public string unlockHint;
}
public class CharacterManager : Singleton<CharacterManager>
{
    public string currentChar = "squirrel";
    Dictionary<string, CharacterInfo> characterInfoDict = new Dictionary<string, CharacterInfo>();
    List<string> unlockedStage = new List<string>();
    List<string> lockedStage = new List<string>();

    // Start is called before the first frame update
    void Start()
    {

        var combinationInfos = CsvUtil.LoadObjects<CharacterInfo>("character");
        foreach (var info in combinationInfos)
        {
            characterInfoDict[info.name] = info;
            if (info.unlockAchievement == null || info.unlockAchievement == "")
            {
                unlockedStage.Add(info.name);
            }
            else
            {
                lockedStage.Add(info.name);
            }
        }
        updateStageUnlock();
        EventPool.OptIn("unlockAchievement", updateStageUnlock);
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
        foreach (var stage in new List<string>(lockedStage))
        {
            var stageInfo = getCharInfo(stage);
            if (AchievementManager.Instance.hasUnlocked(stageInfo.unlockAchievement))
            {
                lockedStage.Remove(stage);
                unlockedStage.Add(stage); 
                EventPool.Trigger("unlockStage");
            }
        }
    }
    public void setCurrentChar(string na)
    {
        currentChar = na;
    }


    public CharacterInfo getCharInfo(string name)
    {
        if (!characterInfoDict.ContainsKey(name))
        {
            Debug.LogError("no character " + name);
        }
        return characterInfoDict[name];
    }

    public CharacterInfo getCurrentInfo()
    {
        return getCharInfo(currentChar);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
