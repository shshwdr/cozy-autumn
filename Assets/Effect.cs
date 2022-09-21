using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Effect : MonoBehaviour
{
    public float duration = 0.3f;
    float scaleValue = 1.5f;
   public float moveUp = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        transform.DOPunchScale(new Vector3(scaleValue, scaleValue, scaleValue), duration,1);
        if (moveUp>0)
        {

            transform.DOLocalMoveY(moveUp, duration);
        }
        //transform.DOScale(scaleValue, duration);
        Destroy(gameObject, duration);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
