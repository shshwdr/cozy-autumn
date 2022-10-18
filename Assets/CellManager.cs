using Sinbad;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellInfo {
    public string type;
    public int cellType;
    public string categoryType;
    public List< string> categoryDetail;
    public int maxCount;
    public Dictionary<int, string> textWhenFirstMeet;
    public int showTime = 0;
    public int hpMode = 0;


    public bool isCell() { return cellType == 1; }
    public bool isEnemy() { return categoryType == "enemy"; }
    public bool isBoss() { return categoryType == "boss"; }
    public bool isEmpty() { return categoryType == "empty"; }
    public bool isPlayer() { return categoryType == "player"; }
    public bool isAlly() { return categoryType == "ally"; }
    public bool isResource() { return categoryType == "resource"; }
    public bool isWeapon() { return categoryType == "weapon"; }
    public bool isTrap() { return categoryType == "trap"; }
    public bool isSplitable() { return categoryType == "splitable"; }

    public int categoryValue;


    public string requireResourcePerStep;
    public string attackMode;
    public string requireResource;
    public int attackPerStep; 
    public int attack; 
    public int moveMode;   
    public int destoryOthers;
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
    public void showCell(string type)
    {
        if (!cellInfoDict.ContainsKey(type))
        {
            return;
        }
        cellInfoDict[type].showTime++;
        if (cellInfoDict[type].textWhenFirstMeet !=null&& cellInfoDict[type].textWhenFirstMeet.ContainsKey(cellInfoDict[type].showTime))
        {
            //TextWhenShowCell.Instance.showText(cellInfoDict[type].textWhenFirstMeet[cellInfoDict[type].showTime]);
        }
    }
    public CellInfo getInfo(string type)
    {

        //if (type.StartsWith("leaf"))
        //{
        //    type = "leaf";
        //}
        var typeSplit = type.Split('_');
        if (typeSplit.Length > 1)
        {
            type = typeSplit[0];
        }
        if (!cellInfoDict.ContainsKey(type))
        {
            Debug.LogError("no key for " + type);
        }
        return cellInfoDict[type];
    }

    public bool isCell(string type)
    {
        var info = getInfo(type);
        return cellInfoDict[info.type].isCell();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
