using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBackground : MonoBehaviour
{
    public bool isHot;
    SpriteRenderer bk;
    public Color fireColor;
    public Color normalColor;
    public void heat()
    {
        isHot = true;
        hp = 2;
        bk.color = fireColor;
    }
    public void unheat()
    {
        isHot = false;
        bk.color = normalColor;
    }

    int hp = 2;
    // Start is called before the first frame update
    void Start()
    {
        bk = GetComponentInChildren<SpriteRenderer>();
        EventPool.OptIn("moveAStep", stepAttack);
        bk.color = normalColor;
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
