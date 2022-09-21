using System;
using System.Collections.Generic;
using Doozy.Engine.UI;
using Pool;
using Sinbad;
using UnityEngine;

namespace Doozy.Examples
{
    public class AchievementInfo
    {
        public string type;
        public string description;
        public string title;

    }
    public class E12PopupManagerScript : Singleton<E12PopupManagerScript>
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
                addRule(rule.type);
            }
        }
        public void addRule(string type)
        {
            if (!visited.Contains(type))
            {
                visitedList.Add(type);
                unvisitedList.Remove(type);
                //EventPool.Trigger("unlockRule");
            }
            visited.Add(type);

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
            addRule(achievementType);
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
                Debug.LogError("no icon for " + icon);
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
}