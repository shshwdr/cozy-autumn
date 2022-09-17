using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateResource : MonoBehaviour, CanReset
{

    public int generateTime = 10;
    int currentTime = 0;

    public List<string> generateItems;
    public List<int> generateItemValues;
    bool canGenerate = true;
    public bool isRandom = false;

    public bool requirePlayerNotMove;
    // Start is called before the first frame update
    void Start()
    {
        EventPool.OptIn("moveAStep", moveStep);
    }

    public void Reset()
    {
        currentTime = 0;
        EventPool.OptIn("moveAStep", moveStep);
    }

    void moveStep()
    {
        if (canGenerate)
        {
            if(!requirePlayerNotMove || !GridController.Instance.hasPlayerMoved)
            {

                currentTime++;
                if (currentTime >= generateTime)
                {
                    currentTime = 0;
                    generate();
                }
            }else if(requirePlayerNotMove && GridController.Instance.hasPlayerMoved)
            {
                currentTime = 0;
            }
        }
    }

    public void finishCollect()
    {
        canGenerate = true;
    }

    void generate()
    {
        canGenerate = false;
        var test = new List<PairInfo<int>>() { };
        test.Add(new PairInfo<int>(Utils.randomList( generateItems) , Utils.randomList(generateItemValues)));
        var go  = ClickToCollect.createClickToCollectItem(test, transform.position);
        go.transform.parent = transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
