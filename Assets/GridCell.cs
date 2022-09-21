using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GridCell : MonoBehaviour
{
    public int index;
    public SpriteRenderer renderer;

    public GameObject bk;

    public CellInfo cellInfo;

   public  GameObject countDownObject;
    public string type;

    public string equipment = null;
    public SpriteRenderer equipRenderer;
    public GameObject ice;

    public Text hp;

    public bool isFreezed = false;

    public void updateHp(int x)
    {
        hp.text = x.ToString();
    }

    public void freeze()
    {
        isFreezed = true;
        if (!ice)
        {
            Debug.Log("??");
            return;
        }
        ice.SetActive(true);
    }
    public void thaw()
    {
        isFreezed = false;
        ice.SetActive(false);
    }

    public  void init(string _type,int i)
    {
        if (hp)
        {

            hp.text = "";
        }
        type = _type;
        cellInfo = CellManager.Instance.getInfo(type);
        renderer.sprite = Resources.Load<Sprite>("cell/" + type);
        index = i;
        bk.SetActive(cellInfo.isCell());

        if (cellInfo.isEnemy())
        {

            gameObject. AddComponent<EnemyCell>().init(type);
            bk.GetComponent<SpriteRenderer>().color = Color.red;
        }
        if(cellInfo.type == "fire")
        {
            gameObject.AddComponent<FireCell>();
            bk.GetComponent<SpriteRenderer>().color = new Color(1,0.5f, 0.5f);
        }
    }

    public void equip(string e)
    {
        equipment = e;
        equipRenderer.sprite = Resources.Load<Sprite>("cell/" + equipment);

        equipRenderer.transform.DOPunchScale(equipRenderer.transform.localScale*2, GridController.Instance.animTime);

        SFXManager.Instance.play("equipweapon");
    }

    public bool hasEquipment()
    {
        return equipment != null && equipment!="";
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
    public virtual void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {

            //if (GetComponent<PlayerCell>())
            //{
            //    ResourceManager.Instance.consumeResource("nut", 1);
            //}
            if (isFreezed)
            {
                failedToMove();
                return;
            }

            GridController.Instance.moveCell(this);
            //index = GridController.Instance.moveCellToEmpty(this);
            //  if(index == -1)
            //  {
            //      Destroy(gameObject);
            //  }
        }
    }

    public void failedToMove()
    {

        transform.DOShakePosition(0.3f, 0.3f, 20);

        SFXManager.Instance.play("negative");
    }

}
