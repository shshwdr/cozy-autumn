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
    List<string> fullDeck = new List<string>();
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
        currentDeck.Clear();

        initCurrentDeck();

        currentDeck.Shuffle();

        round++;
        Debug.Log("start deck round " + round);
        addDeckByRound();
    }
    public string drawCard()
    {
        if (currentDeck.Count == 0)
        {
            createAndShuffleCards();
        }
        var firstCard = currentDeck[0];
        currentDeck.RemoveAt(0);
        return firstCard;
    }

    public void addDictionaryToDeck(Dictionary<string, int> dict)
    {
        foreach(var pair in dict)
        {
            for (int i = 0; i < pair.Value; i++)
            {
                addCardToDeck(pair.Key);
            }
        }
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
                Debug.Log("add deck by round " + round);
            }
        }
        stageInfos = temp;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
