using DG.Tweening;
using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCell : MonoBehaviour
{

    public int hp = 1;
    public bool isDead = false;
    GridCell cell;
    CellInfo info;
    bool isFirst = true;

    int attackCountDown = 10000;

    GameObject countDownObject;
   Text countDownText;

    public void init(string type)
    {
        info = CellManager.Instance.getInfo(type);

        SFXManager.Instance.play(type+"show");
    }

    // Start is called before the first frame update
    void Start()
    {
        cell = GetComponent<GridCell>();
        countDownObject = cell.countDownObject;
        countDownText = countDownObject.GetComponentInChildren<Text>(true);
        if (info.moveMode < 0)
        {
            attackCountDown = -info.moveMode;
            countDownObject.SetActive(true);
            countDownText.text = attackCountDown.ToString();
        }
         EventPool.OptIn("moveAStep", stepAttack);
    }

    void stepAttack()
    {
        Debug.Log("step attack");
        isFirst = false;
    }

    public void getDamage(int x)
    {
        GridController.Instance.addEmpty(GetComponent<GridCell>().index);
        GetComponent<Collider2D>().enabled = false;
        isDead = true;
        transform.DOShakeScale(0.3f, GridController.Instance.animTime);
        Destroy(gameObject, GridController.Instance.animTime+0.1f);

        SFXManager.Instance.play("animalLose");
    }

    void playAttackEffect()
    {

       var go =  Instantiate(Resources.Load<GameObject>("effect/fangAttackEffect"), transform.position, Quaternion.identity);
        go.transform.DOMove(GridController.Instance.getPlayerTransform().position, GridController.Instance.animTime+0.1f);
        Destroy(go, 1f);
    }

    public IEnumerator activeAttack()
    {

        //if player has weapon, unequip weapon and die
        var player = GridController.Instance.getPlayerTransform().GetComponent<GridCell>();
        if (player.hasEquipment())
        {
            player.unequip(transform);

            FindObjectOfType<Doozy.Examples.E12PopupManagerScript>().ShowAchievement("slash");
        }
        else
        {
            SFXManager.Instance.play("attack");
            playAttackEffect();
            RulePopupManager.Instance.showRule("playerNextToSnake");
            //Instantiate(Resources.Load("effect/attack"), GridController.Instance.getPlayerTransform().position, Quaternion.identity);


            takeResource(info.requireResource, info.attack);
        }

        if (attackCountDown == 0)
        {
            attackCountDown = -info.moveMode;
        }
        if (info.moveMode >= 0)
        {
            getDamage(1);
        }
        yield return new WaitForSeconds(GridController.Instance.animTime);

        hasAttacked = true;
    }

    public IEnumerator startAttack()
    {
        hasAttacked = false;
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
            attackCountDown -= 1;

            if (GridController.Instance.isPlayerAround(GetComponent<GridCell>().index) || attackCountDown == 0)
            {
                yield return StartCoroutine(activeAttack());
            }
            else
            {
                if (info.attackPerStep > 0)
                {

                    transform.DOShakeScale(0.2f, 0.2f);


                    takeResource(info.requireResourcePerStep, info.attackPerStep);
                }
            }

            countDownText.text = attackCountDown.ToString();
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
    bool hasAttacked = false;
    public IEnumerator startMove()
    {
        yield return null;
        if (isFirst)
        {
            yield break;
        }
        if (isDead)
        {
            yield break;
        }
        if (hasAttacked)
        {

            yield break;
        }


        if (GridController.Instance.isPlayerAround(GetComponent<GridCell>().index) || attackCountDown == 0)
        {
            yield return StartCoroutine(activeAttack());
            yield break;
        }
        hasAttacked = true;
        if (info.moveMode == 1 || info.moveMode == 2)
        {
            //this enemy would chase player, and only attack infront of it.
            var target = GridController.Instance.getTargetIndexToPlayer(cell.index);
            yield return  StartCoroutine(GridController.Instance.exchangeCard(cell, target));


        }



    }


}
