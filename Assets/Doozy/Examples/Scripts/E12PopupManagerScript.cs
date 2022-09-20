using System;
using System.Collections.Generic;
using Doozy.Engine.UI;
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
        Dictionary<string, AchievementInfo> acDict = new Dictionary<string, AchievementInfo>();
        // Start is called before the first frame update
        void Start()
        {

            var acInfos = CsvUtil.LoadObjects<AchievementInfo>("achievement");
            foreach (var info in acInfos)
            {
                acDict[info.type] = info;
            }
        }

        public void addRule(string type)
        {
            if (!acDict.ContainsKey(type))
            {
                Debug.LogError("no key for " + type);
            }
            visited.Add(type);
            if (visited.Count == acDict.Count-1)
            {

                FindObjectOfType<Doozy.Examples.E12PopupManagerScript>().ShowAchievement("allAchievement");
            }
        }

        public void ShowAchievement(string achievementType)
        {
            if (visited.Contains(achievementType))
            {
                return;
            }
            addRule(achievementType);
            //get the achievement from the list
            AchievementInfo achievement = acDict[achievementType];

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