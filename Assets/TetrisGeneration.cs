using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisGeneration : Singleton<TetrisGeneration>
{
    public GameObject tetrisShapePrefab;
    public List<List<Vector2>> TetrisShapes = new List<List<Vector2>>()
    {
        new List<Vector2>(){new Vector2(0,0),new Vector2(0,1),new Vector2(1,0)  },

    };



    public void generateATetrisShape()
    {

        var go = Instantiate(tetrisShapePrefab);
        go.GetComponent<TetrisShape>().init(TetrisShapes[0]);
    }

    

    // Start is called before the first frame update
    void Start()
    {
       // generateATetrisShape();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
