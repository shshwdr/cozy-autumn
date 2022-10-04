using System;
using System.Collections.Generic;
using Doozy.Engine.UI;
using Pool;
using Sinbad;
using UnityEngine;

//namespace Doozy.Examples
//{
    public class AchievementInfo
    {
        public string type;
        public string description;
        public string title;
        public int amount;
        public int currentAmount;
        public string clearReason;

    }
    public class AchievementManager : Singleton<AchievementManager>
    {
        [Header("Popup Settings")]
        public string PopupName = "AchievementPopup";


        private UIPopup m_popup;

        HashSet<string> visited = new HashSet<string>();
        public List<string> visitedList = new List<string>();
        public List<string> unvisitedList = new List<string>();
        Dictionary<string, AchievementInfo> ruleDict = new Dictionary<string, AchievementInfo>();
        public List<AchievementInfo> ruleList = new List<AchievementInfo>();
        // Start is called before the first frame update
        void Start()
        {

            ruleList = CsvUtil.LoadObjects<AchievementInfo>("achievement");
            foreach (var info in ruleList)
            {
                unvisitedList.Add(info.type);
                ruleDict[info.type] = info;
            }
        }
        public void unlockAll()
        {
            foreach (var rule in ruleList)
            {
                addAchievement(rule.type);
            }
        }
    public bool hasAchievement(string type)
    {
        return ruleDict.ContainsKey(type);
    }
        public bool addAchievement(string type)
        {
            if (!visited.Contains(type))
            {

                if (ruleDict[type].amount > 0)
                {
                    ruleDict[type].currentAmount++;
                    if (ruleDict[type].currentAmount < ruleDict[type].amount)
                    {
                        return false;
                    }
                }


                visitedList.Add(type);
                unvisitedList.Remove(type);

                visited.Add(type);
                //EventPool.Trigger("unlockRule");
            }
            return true;
        }

        public void clear(string reason)
        {
            foreach (var info in ruleList)
            {
                if (!visited.Contains(info.type) && info.clearReason == reason)
                {
                    info.currentAmount = 0;
                }
            }
        }

        public void clearAll()
        {

            foreach (var info in ruleList)
            {
                if (!visited.Contains(info.type) && info.amount>0)
                {
                    info.currentAmount = 0;
                }
            }
        }

        public bool isUnlocked(string type)
        {
            return visited.Contains(type);
        }
        public AchievementInfo getInfo(string type)
        {

            if (!ruleDict.ContainsKey(type))
            {
                Debug.LogError("no key for " + type);
            }
            return ruleDict[type];
        }

        public void ShowAchievement(string achievementType)
        {
            if (visited.Contains(achievementType))
            {
                return;
            }
            if (!addAchievement(achievementType))
            {
                return;
            }
            //get the achievement from the list
            AchievementInfo achievement = ruleDict[achievementType];

            //make sure we got an achievement and that the entry was not null
            if (achievement == null) return;

            //get a clone of the UIPopup, with the given PopupName, from the UIPopup Database 
            m_popup = UIPopupManager.GetPopup(PopupName);

            //make sure that a popup clone was actually created
            if (m_popup == null)
                return;

            //set the achievement icon
            var icon = Resources.Load<Sprite>("achievement/" + achievement.type);
            if (!icon)
            {
                Debug.LogError("no icon for " + achievement.type);
            }
            m_popup.Data.SetImagesSprites(icon);
            //set the achievement title and message
            m_popup.Data.SetLabelsTexts(achievement.title, achievement.description);

            //show the popup
            UIPopupManager.ShowPopup(m_popup, m_popup.AddToPopupQueue, false);
        }

        public void ClearPopupQueue()
        {
            UIPopupManager.ClearQueue();
        }
    }

    [Serializable]
    public class AchievementData
    {
        public string Achievement;
        public string Description;
        public Sprite Icon;
    }
//}