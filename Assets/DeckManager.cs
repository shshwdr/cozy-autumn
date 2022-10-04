using Sinbad;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageInfo {
    public Dictionary<string, int> cards;
    public int triggerRound;
}


public class DeckManager : Singleton<DeckManager>
{
    public bool wouldShuffle = true;
    List<string> fullDeck = new List<string>();
    List<string> waitDeck = new List<string>();
    List<string> currentDeck = new List<string>();
    List<StageInfo> stageInfos;
    int round = -1;
    // Start is called before the first frame update
    void Start()
    {

        stageInfos = CsvUtil.LoadObjects<StageInfo>("stage");
        addStageCards(0);
    }

    void addStageCards(int index)
    {
        addStageCards(stageInfos[index]);
    }

    void addStageCards(StageInfo info)
    {

        addDictionaryToDeck(info.cards);
        stageInfos.Remove(info);
    }

    public void createAndShuffleCards()
    {
        round++;
        Debug.Log("start deck round " + round);
        addDeckByRound();

        currentDeck.Clear();

        initCurrentDeck();

        if (wouldShuffle && round != 2)
        {

            currentDeck.Shuffle();
        }

    }
    public string drawCard(bool canDrawWaitingDeck,bool isBoss = false)
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
            if (!cardInfo.isEnemy())
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
    public void initCurrentDeck()
    {
        //
        if (currentDeck.Count == 0)
        {
            foreach (var value in fullDeck)
            {
                currentDeck.Add(value);
            }
        }
        else
        {
            Debug.Log("deck is not empty");
        }
    }

    void addDeckByRound()
    {
        var temp = new List<StageInfo>(stageInfos);
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
