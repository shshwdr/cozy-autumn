using Pool;
using Sinbad;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageCardInfo {
    public Dictionary<string, int> cards;
    public int triggerRound;
}


public class DeckManager : Singleton<DeckManager>
{
    public bool wouldShuffle = true;
    List<string> fullDeck = new List<string>();
    List<string> waitDeck = new List<string>();
    List<string> currentDeck = new List<string>();
    List<StageCardInfo> stageInfos;
    int round = -1;

    string stageName = "bearForest";
    // Start is called before the first frame update
    void Start()
    {
        if (StageManager.Instance.currentStage != null && StageManager.Instance.currentStage.Length > 0)
        {
            stageInfos = CsvUtil.LoadObjects<StageCardInfo>(StageManager.Instance.currentStage);


            addCardUntilBoss();
        }
    }
    void addCardUntilBoss()
    {
        while (round<StageManager.Instance.getCurrentInfo().stopRound)
        {
            createAndShuffleCards();

        }
        cardInTotal = currentDeck.Count;
    }
    void addStageCards(int index)
    {
        addStageCards(stageInfos[index]);
    }

    void addStageCards(StageCardInfo info)
    {

        addDictionaryToDeck(info.cards);
        stageInfos.Remove(info);
    }

    public int placedCardCount = 0;
    public float getProgress()
    {
        return  Mathf.Clamp01(((float)placedCardCount / (float)cardInTotal));
    }
    public void createAndShuffleCards()
    {
        round++;
        Debug.Log("start deck round " + round);

        if(round> StageManager.Instance.getCurrentInfo().stopRound)
        {
            AchievementManager.Instance.ShowAchievement(StageManager.Instance.getCurrentInfo().stageName + "Finish");

            foreach (var cell in GameObject.FindObjectsOfType<GridCell>())
            {
                if (cell.cellInfo.isAlly())
                {

                    AchievementManager.Instance.ShowAchievement(StageManager.Instance.getCurrentInfo().stageName + "Ally");
                }
            }
        }

        addDeckByRound();

        //currentDeck.Clear();
        List<string> tempDeck = new List<string>();
        initCurrentDeck(tempDeck);

        if (wouldShuffle)
        {

            tempDeck.Shuffle();
        }
        foreach(var d in tempDeck)
        {
            currentDeck.Add(d);
        }

    }
    int cardInTotal = 0;
    public string drawCard(bool canDrawWaitingDeck,bool isBoss = false)
    {
        int test = 100;
        while (true)
        {
            var card = drawCardInternal(canDrawWaitingDeck);
            var cardInfo = CellManager.Instance.getInfo(card);
            if (!GridController.Instance.hasEqualOrMoreCardsWithType(cardInfo.type,cardInfo.maxCount))
            {
                EventPool.Trigger("updateProgress");
                return card;
            }

            test--;
            if (test <= 0)
            {
                EventPool.Trigger("updateProgress");
                Debug.LogError("?");
                return card;
            }
        }
    }

    string drawCardInternal(bool canDrawWaitingDeck, bool isBoss = false)
    {

        if (currentDeck.Count == 0)
        {
            createAndShuffleCards();
        }

        var firstCard = currentDeck[0];
        if (canDrawWaitingDeck && waitDeck.Count > 0)
        {

            firstCard = waitDeck[0];
            waitDeck.RemoveAt(0);
            return firstCard;
        }
        currentDeck.RemoveAt(0);
        return firstCard;
    }
    public string drawBossCard()
    {
        int test = 100;
        while (true)
        {

            var card = drawCard(false);

            var cardInfo = CellManager.Instance.getInfo(card);
            if (!cardInfo.isEnemy() && !cardInfo.isBoss())
            {
                return card;
            }

            test--;
            if (test <= 0)
            {
                return card;
            }
        }

    }

    public string peekCard()
    {

        if (currentDeck.Count == 0)
        {
            createAndShuffleCards();
        }

        var firstCard = currentDeck[0];
        return firstCard;
    }



    public void addDictionaryToDeck(Dictionary<string, int> dict)
    {
        foreach(var pair in dict)
        {
            if (pair.Value > 0)
            {

                for (int i = 0; i < pair.Value; i++)
                {
                    addCardToDeck(pair.Key);

                    if(pair.Key == "ice")
                    {

                        FindObjectOfType<AchievementManager>().ShowAchievement("winter");
                    }
                }
            }
            else
            {
                removeAllCardFromDeck(pair.Key);
            }
        }
    }

    public void removeAllCardFromDeck(string n)
    {
        while (fullDeck.Contains(n))
        {
            fullDeck.Remove(n);

        }
    }
    public void waitingCards(string n)
    {
        waitDeck.Add(n);
    }
    public void addCardToDeck(string n)
    {

        fullDeck.Add(n);
    }
    public void initCurrentDeck(List<string> deck)
    {
        //
        if (deck.Count == 0)
        {
            foreach (var value in fullDeck)
            {
                deck.Add(value);
            }
        }
        else
        {
            Debug.Log("deck is not empty");
        }
    }

    void addDeckByRound()
    {
        var temp = new List<StageCardInfo>(stageInfos);
        foreach(var stageInfo in temp)
        {
            if (round >= stageInfo.triggerRound)
            {
                addStageCards(stageInfo);
                stageInfos.Remove(stageInfo);
                Debug.Log("add deck by round " + round);
            }
        }
        //stageInfos = temp;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
