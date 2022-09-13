using DG.Tweening;
using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireCell : MonoBehaviour
{
    
    public int hp = 5;
    bool isDead = false;
    GridCell cell;
    // Start is called before the first frame update
    void Start()
    {
        cell = GetComponent<GridCell>();
        EventPool.OptIn("moveAStep", stepAttack);
        stepAttack();
    }

    public void getDamage(int x)
    {
        hp -= 1;
        if (hp <= 0)
        {
            GridController.Instance.addEmpty(GetComponent<GridCell>().index);
            GetComponent<Collider2D>().enabled = false;
            isDead = true;
            transform.DOShakeScale(0.3f, 0.3f);
            Destroy(gameObject, 0.3f);
        }
    }
    void stepAttack()
    {
        if (isDead)
        {
            return;
        }
        //if ice nearby, clear it and cost
        var iceAround = GridController.Instance.adjacentType(cell.index, "ice");
        foreach(var c in iceAround)
        {
            c.thaw();
            getDamage(1);
        }
    }

    
}
