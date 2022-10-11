using Sinbad;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CharacterInfo
{
    public string name;
    public string displayName;
    public int isLocked;
}
public class CharacterManager : Singleton<CharacterManager>
{
    public string currentChar = "squirrel";
    Dictionary<string, CharacterInfo> characterInfoDict = new Dictionary<string, CharacterInfo>();
    // Start is called before the first frame update
    void Start()
    {

        var combinationInfos = CsvUtil.LoadObjects<CharacterInfo>("character");
        foreach (var info in combinationInfos)
        {
            characterInfoDict[info.name] = info;
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
