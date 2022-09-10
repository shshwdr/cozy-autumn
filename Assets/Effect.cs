using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Effect : MonoBehaviour
{
    public float duration = 0.3f;
    // Start is called before the first frame update
    void Start()
    {
        transform.DOPunchScale(new Vector3(1.5f, 1.5f, 1.5f), duration);
        Destroy(gameObject, duration);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
