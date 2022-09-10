using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapItem : GridItemAction
{
    public int damage = 1;
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
        if (cell.GetComponent<EnemyCell>())
        {
            cell.GetComponent<EnemyCell>().getDamage(damage);
            Destroy(gameObject);
        }
        else if (cell.GetComponent<EnemyCell>())
        {
            ResourceManager.Instance.consumeResource("nut", damage);
            Destroy(gameObject);
        }

    }
}
