using DG.Tweening;
using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCell : MonoBehaviour
{

    public int hp = 1;
    bool isDead = false;
    GridCell cell;
    CellInfo info;
    bool isFirst = true;
    public void init(string type)
    {
        info = CellManager.Instance.getInfo(type);
    }

    // Start is called before the first frame update
    void Start()
    {
        cell = GetComponent<GridCell>();
       // EventPool.OptIn("moveAStep", stepAttack);
    }

    public void getDamage(int x)
    {
        GridController.Instance.addEmpty(GetComponent<GridCell>().index);
        GetComponent<Collider2D>().enabled = false;
        isDead = true;
        transform.DOShakeScale(0.3f, 0.3f);
        Destroy(gameObject, 0.3f);
    }


    public IEnumerator startAttack()
    {
        yield return null;
        if (isFirst || isDead)
        {
            yield break;
        }
        //if (info.moveMode == 1 || info.moveMode == 2) {

        //    if (GridController.Instance.isPlayerAround(cell.index))
        //    {

        //    }
        //}
        //else
        {


            if (GridController.Instance.isPlayerAround(GetComponent<GridCell>().index))
            {
                //if player has weapon, unequip weapon and die
                var player = GridController.Instance.getPlayerTransform().GetComponent<GridCell>();
                if (player.hasEquipment())
                {
                    player.unequip(transform);

                    FindObjectOfType<Doozy.Examples.E12PopupManagerScript>().ShowAchievement(2);
                }
                else
                {

                    RulePopupManager.Instance.showRule("playerNextToSnake");
                    Instantiate(Resources.Load("effect/attack"), GridController.Instance.getPlayerTransform().position, Quaternion.identity);


                    takeResource(info.requireResource, info.attack);
                    //ResourceManager.Instance.consumeResource("nut", attack);
                }

                getDamage(1);
            }
            else
            {
                transform.DOShakeScale(0.2f, 0.2f);


                takeResource(info.requireResourcePerStep, info.attackPerStep);
                //ResourceManager.Instance.consumeResource("nut", attackPerStep);
            }
        }

    }

    void takeResource(string res, int value)
    {
        var resValue = ResourceManager.Instance.getAmount(res);
        if (resValue >= value || res == "nut")

        {
            ResourceManager.Instance.consumeResource(res, value);
        }
        else
        {

            ResourceManager.Instance.consumeResource(res, resValue);


            ResourceManager.Instance.consumeResource("nut", (value - resValue)*2);
        }
    }

    public IEnumerator startMove()
    {
        yield return null;
        if (isFirst)
        {
            isFirst = false;
            yield break;
        }
        if (isDead)
        {
            yield break;
        }

        if (info.moveMode == 1 || info.moveMode == 2)
        {
            //this enemy would chase player, and only attack infront of it.
            var target = GridController.Instance.getTargetIndexToPlayer(cell.index);
            StartCoroutine(GridController.Instance.exchangeCard(cell, target));


        }
        else
        {
            //if(info.)

            //if player nearby, attack and destroy
            //if (GridController.Instance.isPlayerAround(GetComponent<GridCell>().index))
            //{
            //    //if player has weapon, unequip weapon and die
            //    var player = GridController.Instance.getPlayerTransform().GetComponent<GridCell>();
            //    if (player.hasEquipment())
            //    {
            //        player.unequip(transform);

            //        FindObjectOfType<Doozy.Examples.E12PopupManagerScript>().ShowAchievement(2);
            //    }
            //    else
            //    {

            //        RulePopupManager.Instance.showRule("playerNextToSnake");
            //        Instantiate(Resources.Load("effect/attack"), GridController.Instance.getPlayerTransform().position, Quaternion.identity);
            //        ResourceManager.Instance.consumeResource("nut", attack);
            //    }

            //    getDamage(1);
            //}
            //else
            //{
            //    transform.DOShakeScale(0.2f, 0.2f);

            //    ResourceManager.Instance.consumeResource("nut", attackPerStep);
            //}
        }



    }


}
