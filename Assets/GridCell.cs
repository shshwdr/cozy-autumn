using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GridCell : MonoBehaviour
{
    public int index;
    public SpriteRenderer renderer;

    public GameObject bk;

    public CellInfo cellInfo;

    public string type;

    string equipment = null;
    public SpriteRenderer equipRenderer;

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

    public void equip(string e)
    {
        equipment = e;
        equipRenderer.sprite = Resources.Load<Sprite>("cell/" + equipment);

        equipRenderer.transform.DOPunchScale(equipRenderer.transform.localScale*2, 0.3f);
    }

    public bool hasEquipment()
    {
        return equipment != null;
    }


    public void unequip(Transform targetTransform)
    {
        if (equipment == null)
        {
            Debug.LogError("nothing to unequip");
        }

        var go = Instantiate(Resources.Load<GameObject>("flyingObject"), equipRenderer.transform.position, Quaternion.identity);
        go.transform.localScale = equipRenderer.transform.localScale;
        go.GetComponent<FlyingObject>().init(equipRenderer.sprite, targetTransform.position);

        equipment = null;
        equipRenderer.sprite = Resources.Load<Sprite>("cell/" + "empty");

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
        //if (GetComponent<PlayerCell>())
        //{
        //    ResourceManager.Instance.consumeResource("nut", 1);
        //}
        GridController.Instance.moveCell(this);
      //index = GridController.Instance.moveCellToEmpty(this);
      //  if(index == -1)
      //  {
      //      Destroy(gameObject);
      //  }
    }

}
