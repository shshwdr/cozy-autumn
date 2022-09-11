using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public int index;
    public SpriteRenderer renderer;

    public GameObject bk;

    public CellInfo cellInfo;

    public string type;

    public  void init(string _type,int i)
    {
        type = _type;
        cellInfo = CellManager.Instance.getInfo(type);
        renderer.sprite = Resources.Load<Sprite>("cell/" + type);
        index = i;
        bk.SetActive(cellInfo.isCell());

        if (cellInfo.isEnemy())
        {

            gameObject. AddComponent<EnemyCell>();
            bk.GetComponent<SpriteRenderer>().color = Color.red;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnMouseDown()
    {
        if (GetComponent<PlayerCell>())
        {
            ResourceManager.Instance.consumeResource("nut", 1);
        }
       index = GridController.Instance.moveCellToEmpty(this);
        if(index == -1)
        {
            Destroy(gameObject);
        }
    }

}
