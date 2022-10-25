using System;
using System.Collections.Generic;
using Doozy.Engine.UI;
using Pool;
using Sinbad;
using UnityEngine;

//namespace Doozy.Examples
//{
public class NewCellManager : Singleton<NewCellManager>
{
    [Header("Popup Settings")]
    public string PopupName = "AchievementPopup";


    private UIPopup m_popup;

    HashSet<string> visited = new HashSet<string>();
    public List<string> visitedList = new List<string>();
    public List<string> unvisitedList = new List<string>();
    // Start is called before the first frame update
    void Start()
    {

        foreach (var info in CellManager.Instance.combinationInfos)
        {
            if(info.desc!=null && info.desc !="" && info.desc.Length > 0)
            {
                unvisitedList.Add(info.type);
            }
        }
    }

    public bool hasUnlocked(string str)
    {
        return visited.Contains(str);
    }
    public void unlockAll()
    {
        foreach (var rule in unvisitedList)
        {
            unlock(rule);
        }
    }
    //public bool hasAchievement(string type)
    //{
    //    return ruleDict.ContainsKey(type);
    //}
    public void unlock(string type)
    {
        if (!visited.Contains(type) && unvisitedList.Contains(type))
        {

            visitedList.Add(type);
            unvisitedList.Remove(type);

            visited.Add(type);
          //  EventPool.Trigger("unlockAchievement");
        }
    }

    public bool isQueueEmpty()
    {
        return UIPopupManager.QueueSize() == 0;
    }
    public bool isUnlocked(string type)
    {
        return visited.Contains(type);
    }

    public void showCellInfo(string achievementType)
    {
        if (!visited.Contains(achievementType))
        {
            return;
        }
        var cell = CellManager.Instance.getInfo(achievementType);

        //get a clone of the UIPopup, with the given PopupName, from the UIPopup Database 
        m_popup = UIPopupManager.GetPopup(PopupName);

        //make sure that a popup clone was actually created
        if (m_popup == null)
            return;

        //set the achievement icon
        var icon = Resources.Load<Sprite>("cell/" + cell.type);
        if (!icon)
        {
            icon = Resources.Load<Sprite>("cell/" + "tutorialDrag");
            //Debug.LogError("no icon for " + cell.type);
        }
        m_popup.Data.SetImagesSprites(icon);
        //set the achievement title and message
        m_popup.Data.SetLabelsTexts(cell.displayName, cell.desc);
        if (cell.displayName == "" || cell.desc == "")
        {

            Debug.LogError("no icon for " + cell.type);
        }
        //show the popup
        UIPopupManager.ShowPopup(m_popup, m_popup.AddToPopupQueue, false);
    }
    public void ShowNewCell(string achievementType)
    {
        if (visited.Contains(achievementType))
        {
            return;
        }
        if(!unvisitedList.Contains(achievementType))
        {
            return;
        }
        unlock(achievementType);

        showCellInfo(achievementType);
    }

    public void ClearPopupQueue()
    {
        UIPopupManager.ClearQueue();
    }
}

//}