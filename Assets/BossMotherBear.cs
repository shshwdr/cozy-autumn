using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMotherBear : Boss
{
    enum Stage { none, prepare, attacked }
    Stage stage;
    int prepareRound = 4;
    int attackRound = 3;
    List<int> dangeousIndices;
    // Start is called before the first frame update
    void Start()
    {
        GridController.Instance.boss = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    public override void onNextStep()
    {
        switch (stage)
        {
            case Stage.none:
                stage = Stage.prepare;
                //select dangeous area
                dangeousIndices = new List<int>() { 0, 1, 3, 4 };
                GridController.Instance.showDangerousCell("motherBear", dangeousIndices);
                count = prepareRound;
                break;
            case Stage.prepare:
                count--;
                if(count == 0)
                {
                    //start attack

                    GridController.Instance.hideDangerousCell("motherBear", dangeousIndices);
                    count = attackRound;
                }
                break;
            case Stage.attacked:
                count--;
                if (count == 0)
                {
                    //move back

                }
                break;
        }
    }
}
