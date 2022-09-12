using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingObject : MonoBehaviour
{

    Vector3 targetPosition;
    public SpriteRenderer renderer;

    public void init(Sprite sprite, Vector3 target, float time = 0.3f)
    {
        renderer.sprite = sprite;
        transform.DORotate(new Vector3(0, 0, 180), time);
        transform.DOMove(target, time);
        Destroy(gameObject, time);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
