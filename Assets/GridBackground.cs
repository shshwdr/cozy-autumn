using DG.Tweening;
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

    public SpriteRenderer dangeousOverlay;

    public void addDangerous(string dangerName)
    {


        dangeousOverlay.color = new Color(255, 0, 0, 0);
        DOTween.To(() => dangeousOverlay.color, x => dangeousOverlay.color = x, new Color(255, 0, 0,0.5f), 1).SetLoops(-1,LoopType.Yoyo);
    }
    public void removeDangerous(string dangerName)
    {
        dangeousOverlay.DOKill();
        DOTween.Kill(dangeousOverlay);
        DOTween.Kill(transform);
        dangeousOverlay.color = new Color(255, 0, 0, 0);
    }
    public void heat()
    {
        isHot = true;
        hp = 2;
        //bk.color = fireColor;
        bk.gameObject.SetActive(true);
    }
    public void unheat()
    {
        isHot = false;
        //bk.color = normalColor;
        bk.gameObject.SetActive(false);
    }

    int hp = 2;
    // Start is called before the first frame update
    void Start()
    {
        bk = GetComponentInChildren<SpriteRenderer>(true);
        EventPool.OptIn("moveAStep", stepAttack);
       // bk.color = normalColor;
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
