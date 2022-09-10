using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NutItem : GridItemAction
{
    public int rewardNut = 3;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void act(GridCell cell)
    {
        base.act(cell);
        if (cell.GetComponent<PlayerCell>())
        {

            var resource = new List<PairInfo<int>>() { };
            resource.Add(new PairInfo<int>("nut", rewardNut));
            CollectionManager.Instance.AddCoins(transform.position, resource);
            Destroy(gameObject);
        }
        else if (cell.GetComponent<EnemyCell>())
        {

            Destroy(gameObject);
        }
    }
}
