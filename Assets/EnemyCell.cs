using DG.Tweening;
using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCell : MonoBehaviour
{

    public bool isDead = false;
    public GridCell cell;
    CellInfo info;
    public bool isFirst = true;
    bool hasBeenAttacked = false;
    public bool isBoss = false;
    public int hp { get { return cell.amount; } }
    int getAttack { get { return cell.amount; } }

    //bool isChasedAway

    int attackCountDown = 10000;

    CounterDown countDownObject;
    CanvasGroup explainPanel;
    bool hasUpdatedDescription = false;
    public void init(string type)
    {
        info = CellManager.Instance.getInfo(type);

        SFXManager.Instance.play(type + "show");
        explainPanel = GetComponentInChildren<CanvasGroup>();
        updateDescription();

        cell = GetComponent<GridCell>();
    }

    public void finishedMove()
    {
        hasBeenAttacked = false;
    }

    private void OnDestroy()
    {
        explainPanel.DOKill();
    }
    void updateDescription()
    {
        HintCell.generateHintText(explainPanel.transform.Find("attackWhenNext"), "Steal " + info.attack + " [" + info.requireResource + "] if next to [player]");
        if (info.stayMode == 0)
        {

            HintCell.generateHintText(explainPanel.transform.Find("Move2"), "Run away after attack.");
        }
        if (info.attackPerStep > 0)
        {
            HintCell.generateHintText(explainPanel.transform.Find("attackPerRound"), "Otherwise steal " + info.attackPerStep + " [" + info.requireResourcePerStep + "] per round");
        }
        else
        {
            explainPanel.transform.Find("attackPerRound").gameObject.SetActive(false);
        }

        if (info.moveMode < 0)
        {

            HintCell.generateHintText(explainPanel.transform.Find("Move"), "Attack every " + (-info.moveMode) + " round");
            HintCell.generateHintText(explainPanel.transform.Find("Move2"), "Won't run away after attack.");
        }

        if (info.requireResource != "nut")
        {

            HintCell.generateHintText(explainPanel.transform.Find("notNut"), "If [" + info.requireResourcePerStep + "] is not enough, steal 2x [nut]");
        }
        else
        {

            explainPanel.transform.Find("notNut").gameObject.SetActive(false);
        }

        if (info.moveMode > 0)
        {
            HintCell.generateHintText(explainPanel.transform.Find("Move"), "Move to [player]");
        }
        if (info.moveMode == 0)
        {
            explainPanel.transform.Find("Move").gameObject.SetActive(false);
        }

        StartCoroutine(test());
    }

    IEnumerator test()
    {
        yield return new WaitForSeconds(0.01f);
        if (this && gameObject)
        {

            // LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            foreach (var h in GetComponentsInChildren<HorizontalLayoutGroup>())
            {
                h.enabled = false;
            }
            //  string1.GetComponent<HorizontalLayoutGroup>().enabled = false;
            yield return new WaitForSeconds(0.01f);
            // Canvas.ForceUpdateCanvases();
            if (this && gameObject)
            {

                // LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
                foreach (var h in GetComponentsInChildren<HorizontalLayoutGroup>())
                {
                    h.enabled = true;
                }
                // string1.gameObject.SetActive(true);
                //string1.GetComponent<HorizontalLayoutGroup>().enabled = true;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        cell = GetComponent<GridCell>();
        if (cell)
        {

            countDownObject = cell.countDownObject;
            if (info.moveMode < 0)
            {
                attackCountDown = -info.moveMode;
                countDownObject.gameObject.SetActive(true);
                countDownObject.initCount(attackCountDown);
            }
        }
        EventPool.OptIn("moveAStep", stepAttack);
    }

    void stepAttack()
    {
        if (isDead)
        {
            return;
        }
        Debug.Log("step attack");
        //isFirst = false;
    }

    void die()
    {
        //if (!isDead)
        //{
        //    if (!isBoss)
        //    {

        //        GridController.Instance.addEmpty(GetComponent<GridCell>().index);
        //    }
        //    else
        //    {
        //        GetComponentInParent<Boss>().getKilled();
        //    }
        //    GetComponent<Collider2D>().enabled = false;
        //    isDead = true;
        //    transform.DOShakeScale(0.3f, GridController.Instance.animTime);
        //    Destroy(gameObject, GridController.Instance.animTime + 0.1f);
        //    foreach (var render in GetComponentsInChildren<SpriteRenderer>())
        //    {
        //        render.sortingOrder = 100000;
        //    }

        //    if (AchievementManager.Instance.hasAchievement("dead" + info.type))
        //    {
        //        AchievementManager.Instance.ShowAchievement("dead" + info.type);
        //    }
        //}
    }

    public void getDamage(int x)
    {
        if (isDead)
        {
            return;
        }

        cell.decreaseAmount(x);
        if(cell.amount <= 0)
        {
            die();
        }

        SFXManager.Instance.play("animalLose");
        //isFirst = false;
        hasBeenAttacked = true;
    }

    void playAttackEffect(GridCell attackee)
    {

        if (isDead)
        {
            return;
        }
        var go = Instantiate(Resources.Load<GameObject>("effect/fangAttackEffect"), transform.position, Quaternion.identity);
        go.transform.DOMove(attackee.transform.position, GridController.Instance.animTime + 0.1f);
        Destroy(go, 1f);
        SFXManager.Instance.play("scream");
    }
    //bool beAttack(GridCell attackee, bool forceAttack)
    //{
    //    if (attackee.hasEquipment() && !forceAttack && canBeAttacked())
    //    {
    //        attackee.unequip(transform);

    //        FindObjectOfType<AchievementManager>().ShowAchievement("slash");
    //        FindObjectOfType<AchievementManager>().ShowAchievement("slash2");
    //        return true;
    //    }
    //    return false;
    //}

    void playHealEffect(EnemyCell healer)
    {

        if (isDead)
        {
            return;
        }
        var go = Instantiate(Resources.Load<GameObject>("effect/healEffect"), healer.transform.position , Quaternion.identity);
        go.transform.DOMove(transform.position, GridController.Instance.animTime + 0.1f);
        Destroy(go, 1f);
        SFXManager.Instance.play("scream");
    }
    public IEnumerator heal(EnemyCell healer)
    {
        return heal(healer, healer.cell.cellInfo.attack);
    }

    public IEnumerator heal(EnemyCell healer, int healAmount)
    {
        SFXManager.Instance.play("heal");
        playHealEffect(healer);
        cell.addAmount(healAmount);
        yield return new WaitForSeconds(GridController.Instance.animTime);
    }

    public    bool attack(GridCell attackee, bool canAttack)
    {
        if (canAttack && !isDead)
        {
            SFXManager.Instance.play("attack");
            playAttackEffect(attackee);
            //RulePopupManager.Instance.showRule("playerNextTo" + info.type);
            //Instantiate(Resources.Load("effect/attack"), GridController.Instance.getPlayerTransform().position, Quaternion.identity);

            if (attackee.cellInfo.isPlayer())
            {

                attackee.decreaseAmount(getAttack);
                //takeResource(info.requireResource, getAttack);
            }
            else
            {
                attackee.decreaseAmount(getAttack);
            }
            return true;
        }
        return false;
    }

    public IEnumerator activeAttack(bool forceAttack = false, bool canAttack = true)
    {

        yield return null;
        //if (isDead)
        //{
        //    yield break;
        //}
        ////if player has weapon, unequip weapon and die
        //var player = GridController.Instance.getPlayerTransform().GetComponent<GridCell>();
        //var ally = GridController.Instance.getAllyGridCell();
        ////logic need to change if enemy has more than one hp
        ////if (ally && GridController.Instance.isAllyAround(GetComponent<GridCell>().index) && beAttack(ally, forceAttack))
        ////{
        ////} else if (player && GridController.Instance.isPlayerAround(GetComponent<GridCell>().index) && beAttack(player, forceAttack))
        ////{
        ////}else 
        //if (!ally)
        //{
        //    attack(player, canAttack);
        //}
        //else
        //{
        //    if (ally &&( GridController.Instance.isAllyAround(GetComponent<GridCell>().index)) && attack(ally, canAttack))
        //    {

        //    }
        //    else if (player &&(GridController.Instance.isPlayerAround(GetComponent<GridCell>().index) )&& attack(player, canAttack)) { } 
        //    else
        //    {
        //        //find out the closest one between ally and player and attack
        //        var d1 = GridController.Instance.distance(ally.index, cell.index);
        //        var d2 = GridController.Instance.distance(player.index, cell.index);
        //        if (d1 > d2)
        //        {
        //            attack(player, canAttack);
        //        }
        //        else
        //        {
        //            attack(ally, canAttack);

        //        }
        //    }

        //}

        //if (attackCountDown == 0)
        //{
        //    attackCountDown = -info.moveMode;
        //}
        //if (info.stayMode == 0)
        //{
        //    getDamage(getAttack);
        //}
        //yield return new WaitForSeconds(GridController.Instance.animTime);

        //hasAttacked = true;
    }

    private void OnMouseEnter()
    {

        //if (GridController.Instance.isMoving)
        //{
        //    return;
        //}

        //if (explainPanel)
        //{
        //    //if (!hasUpdatedDescription)
        //    //{
        //    //    hasUpdatedDescription = true;
        //    //    updateDescription();
        //    //}
        //    DOTween.To(() => explainPanel.alpha, x => explainPanel.alpha = x, 1, 0.3f);
        //}

        //explainPanel.alpha = 1;

    }

    private void OnMouseExit()
    {
        if (explainPanel)
            DOTween.To(() => explainPanel.alpha, x => explainPanel.alpha = x, 0, 0.3f);

        //explainPanel.alpha = 0;
    }


    public bool willAttack()
    {
        hasAttacked = false;
        if (isFirst || isDead || hasBeenAttacked)
        {
            return false;
        }
        //if (GridController.Instance.isPlayerAround(GetComponent<GridCell>().index) || attackCountDown == 0)
        //{
        //    return true;
        //}
        return true;

    }
    public bool canBeAttacked()
    {
        return !isDead && !isFirst;
    }


    public IEnumerator startAttack()
    {
        if (isDead)
        {
            yield break;
        }
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
        //{
        //    attackCountDown -= 1;

        //    if (info.moveMode < 0)
        //    {

        //        RulePopupManager.Instance.showRule("foxCountingdown");
        //        countDownObject.initCount(attackCountDown);
        //    }

        //    if(info.attackMode == "heal")
        //    {
        //        //heal enemies
        //        foreach (var adjacentCell in GridController.Instance.getAdjacentCells(cell.index))
        //        {
        //            if (adjacentCell.GetComponent<EnemyCell>())
        //            {
        //                yield return StartCoroutine( adjacentCell.GetComponent<EnemyCell>().heal(this));
        //            }
        //        }
        //    }
        //    else
        //    {
        //        bool hasStolen = false;
        //        if(info.attackMode == "steal")
        //        {
        //            //try steal around
        //            foreach (var adjacentCell in GridController.Instance.getAdjacentCells(cell.index))
        //            {
        //                if (adjacentCell.cellInfo.isSplitable())
        //                {
        //                    var nextSplit = GridController.Instance.getNextSplit(adjacentCell);
        //                    hasStolen = true;
        //                    yield return StartCoroutine(GridController.Instance.removeSplit(adjacentCell, cell.index, nextSplit));
        //                    yield return StartCoroutine(heal(this, 1));
        //                    break;
        //                }
        //            }
        //        }
        //        if (!hasStolen)
        //        {
        //            if (GridController.Instance.isCharacterAround(GetComponent<GridCell>().index) || attackCountDown == 0)
        //            {
        //                yield return StartCoroutine(activeAttack());
        //            }
        //            else
        //            {
        //                if (info.attackPerStep > 0)
        //                {

        //                    transform.DOShakeScale(0.2f, 0.2f);


        //                    //takeResource(info.requireResourcePerStep, info.attackPerStep);
        //                }
        //            }
        //        }

                
        //    }
        //}

    }

    void takeResource(string res, int value)
    {
        //var resValue = ResourceManager.Instance.getAmount(res);
        //if (resValue >= value || res == "nut")

        //{
        //    ResourceManager.Instance.consumeResource(res, value,transform.position);
        //}
        //else
        //{

        //    ResourceManager.Instance.consumeResource(res, resValue, transform.position);


        //    ResourceManager.Instance.consumeResource("nut", (value - resValue) * 2, transform.position);
        //}
    }
    bool hasAttacked = false;

    public bool willMove()
    {
        if (isFirst || isDead || hasAttacked|| hasBeenAttacked)
        {
            return false;
        }
        if (info.moveMode == 1 || info.moveMode == 2)
        {
            return true;
        }
        return false;

    }
    public IEnumerator startMove()
    {
        yield return null;
        //if (isDead)
        //{
        //    hasBeenAttacked = false;
        //    yield break;
        //}
        //if (isFirst)
        //{
        //    hasBeenAttacked = false;
        //    yield break;
        //}
        //if (isDead)
        //{
        //    hasBeenAttacked = false;
        //    yield break;
        //}
        //if (hasAttacked)
        //{

        //    hasBeenAttacked = false;
        //    yield break;
        //}


        //if (info.moveMode == 1 || info.moveMode == 2)
        //{

        //    RulePopupManager.Instance.showRule("weaselMove");
        //    //should it move and attack? not wait a move?
        //    if (GridController.Instance.isCharacterAround(GetComponent<GridCell>().index) || attackCountDown == 0)
        //    {
        //        yield return StartCoroutine(activeAttack());
        //        hasBeenAttacked = false;
        //        yield break;
        //    }
        //    hasAttacked = true;


        //    //this enemy would chase player, and only attack infront of it.
        //    var target = GridController.Instance.getTargetIndexToPlayer(cell.index);

        //    if (!cell || target == null)
        //    {
        //        Debug.LogError("?");
        //    }
        //    yield return StartCoroutine(GridController.Instance.exchangeCard(cell, target));


        //}



        //hasBeenAttacked = false;
    }


}
