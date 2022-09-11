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
        transform.DOShakeScale(0.4f,0.4f);
        Destroy(gameObject,0.4f);
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
            Instantiate(Resources.Load("effect/attack"), GridController.Instance.getPlayerTransform().position, Quaternion.identity);
            ResourceManager.Instance.consumeResource("nut", attack);
            getDamage(1);
        }
        else
        {
            transform.DOShakeScale(0.2f,0.2f);

            ResourceManager.Instance.consumeResource("nut", attackPerStep);
        }

    }


}
