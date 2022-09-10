using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
   public int index;

    public  void init(int i)
    {
        index = i;
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
