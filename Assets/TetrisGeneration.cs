using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisGeneration : Singleton<TetrisGeneration>
{
    public GameObject tetrisShapePrefab;
    public GameObject previewTetris;
    int count = 0;
    public List<List<Vector2>> TetrisShapes = new List<List<Vector2>>()
    {
        new List<Vector2>(){new Vector2(0,0),new Vector2(0,1),new Vector2(1,0)  },
        new List<Vector2>(){new Vector2(0,0),new Vector2(0,1)  },
        new List<Vector2>(){new Vector2(0,0),new Vector2(0,1),new Vector2(1,0),new Vector2(-1,0)  },

    };



    public void generateATetrisShape()
    {

        Vector3 previewPosition = Camera.main.ScreenToWorldPoint(GameObject.Find("previewTetris").transform.position);
        previewPosition.z = 0;
        var go = Instantiate(tetrisShapePrefab, previewPosition, Quaternion.identity);
        var tetrisShape = TetrisShapes[Random.Range(0, TetrisShapes.Count)];
        //if(count == 0)
        //{
        //    tetrisShape = new List<Vector2>() { new Vector2(0, 0) };
        //}else if(count == 1)
        //{
        //    tetrisShape = new List<Vector2>(){ new Vector2(0, 0),new Vector2(0, 1)  };

        //}
        go.GetComponent<TetrisShape>().init(tetrisShape);
        if (previewTetris)
        {

            Vector3 currentPosition = Camera.main.ScreenToWorldPoint(GameObject.Find("currentTetris").transform.position);
            currentPosition.z = 0;
            previewTetris.transform.position = currentPosition;
            previewTetris.GetComponent<TetrisShape>().getReady();

        }
        else
        {
        }
        previewTetris = go;
        count++;
        
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
