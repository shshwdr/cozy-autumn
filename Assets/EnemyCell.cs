using DG.Tweening;
using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCell : MonoBehaviour
{

    public int attackPerStep = 1;
    public int attack = 5;
    public int hp = 1;
    bool isDead = false;

    // Start is called before the first frame update
    void Start()
    {
        EventPool.OptIn("moveAStep", stepAttack);
    }

    public void getDamage(int x)
    {
        GridController.Instance.addEmpty(GetComponent<GridCell>().index);
        GetComponent<Collider2D>().enabled = false;
        isDead = true;
        transform.DOShakeScale(0.3f,0.3f);
        Destroy(gameObject,0.3f);
    }
    void stepAttack()
    {
        if (isDead)
        {
            return;
        }
        //if player nearby, attack and destroy
        if (GridController.Instance.isPlayerAround(GetComponent<GridCell>().index))
        {
            //if player has weapon, unequip weapon and die
            var player = GridController.Instance.getPlayerTransform().GetComponent<GridCell>();
            if (player.hasEquipment())
            {
                player.unequip(transform);
            }
            else
            {

                Instantiate(Resources.Load("effect/attack"), GridController.Instance.getPlayerTransform().position, Quaternion.identity);
                ResourceManager.Instance.consumeResource("nut", attack);
            }

            getDamage(1);
        }
        else
        {
            transform.DOShakeScale(0.2f,0.2f);

            ResourceManager.Instance.consumeResource("nut", attackPerStep);
        }

    }


}
