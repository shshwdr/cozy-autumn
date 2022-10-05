using Sinbad;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellInfo {
    public string type;
    public int cellType;
    public string categoryType;
    public string categoryDetail;
    public int maxCount;
    
    public bool isCell() { return cellType == 1; }
    public bool isEnemy() { return categoryType == "enemy"; }
    public bool isBoss() { return categoryType == "boss"; }
    public bool isEmpty() { return categoryType == "empty"; }
    public bool isPlayer() { return categoryType == "player"; }
    public bool isResource() { return categoryType == "resource"; }
    public bool isWeapon() { return categoryType == "weapon"; }

    public int categoryValue;


    public string requireResourcePerStep;
    public string requireResource;
    public int attackPerStep; public int attack; public int moveMode;   public int destoryOthers;
    public int stayMode;//0 after attack, run away, 1 dont run away


}


public class CellManager : Singleton<CellManager>
{
    Dictionary<string, CellInfo> cellInfoDict = new Dictionary<string, CellInfo>();
    void Start()
    {

        var combinationInfos = CsvUtil.LoadObjects<CellInfo>("cell");
        foreach(var info in combinationInfos)
        {
            cellInfoDict[info.type] = info;
        }
    }

    public CellInfo getInfo(string type)
    {

        if (type.StartsWith("leaf"))
        {
            type = "leaf";
        }
        if (!cellInfoDict.ContainsKey(type))
        {
            Debug.LogError("no key for " + type);
        }
        return cellInfoDict[type];
    }

    public bool isCell(string type)
    {
        if (!cellInfoDict.ContainsKey(type))
        {
            if (type.StartsWith("leaf"))
            {
                return true;
            }
            Debug.LogError("no key for " + type);
        }
        return cellInfoDict[type].isCell();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
