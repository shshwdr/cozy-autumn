using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBackground : MonoBehaviour
{
    public bool isHot;
    SpriteRenderer bk;
    public Color fireColor;
    public void heat()
    {
        isHot = true;
        hp = 2;
        bk.color = fireColor;
    }
    public void unheat()
    {
        isHot = false;
        bk.color = new Color(0, 0, 0, 0f);
    }

    int hp = 2;
    // Start is called before the first frame update
    void Start()
    {
        bk = GetComponentInChildren<SpriteRenderer>();
        EventPool.OptIn("moveAStep", stepAttack);
    }

    void stepAttack()
    {
        if (isHot)
        {
            hp -= 1;
            if (hp <= 0)
            {
                unheat();
            }
        }
    }
}
