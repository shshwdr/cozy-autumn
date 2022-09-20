using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Effect : MonoBehaviour
{
    public float duration = 0.3f;
    float scaleValue = 1.5f;
    // Start is called before the first frame update
    void Start()
    {
        transform.DOPunchScale(new Vector3(scaleValue, scaleValue, scaleValue), duration,1);
        transform.DOLocalMoveY(0.2f, duration);
        //transform.DOScale(scaleValue, duration);
        Destroy(gameObject, duration);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
