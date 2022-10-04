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
        cell.updateHp(hp);

        SFXManager.Instance.play("fire");
    }

    public void addHp(int x)
    {
        hp += x;

        cell.updateHp(hp);
    }

    public void getDamage(int x)
    {
        hp -= 1;
        cell.updateHp(hp);
        if (hp <= 0)
        {
            GridController.Instance.addEmpty(GetComponent<GridCell>().index);
            GetComponent<Collider2D>().enabled = false;
            isDead = true;
            transform.DOShakeScale(0.3f, GridController.Instance.animTime);
            Destroy(gameObject, GridController.Instance.animTime);
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
            if (!c.GetComponent<GridItem>() && !c.GetComponent<GridBackground>() && !c.GetComponent<GridEmpty>())
            {

                c.thaw();

                RulePopupManager.Instance.showRule("iceThaw");
                SFXManager.Instance.play("icemelt");
                FindObjectOfType<Doozy.Examples.E12PopupManagerScript>().ShowAchievement("melt");
                FindObjectOfType<Doozy.Examples.E12PopupManagerScript>().ShowAchievement("melt2");
            }
            //getDamage(1);
        }
    }

    
}
