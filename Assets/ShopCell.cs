using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopCell : GridCell
{
    ResourceCell[] costCell;

    public ShopInfo cellInfo;
    public bool isShop;
    public void init(string _type, int i, bool s)
    {

        costCell = GetComponentsInChildren<ResourceCell>();
        isShop = s;
        type = _type;
        if (isShop)
        {
            cellInfo = ShopManager.Instance.getInfo(type);
            renderer.sprite = Resources.Load<Sprite>("decoration/" + type);

            explainText.text = cellInfo.description;
        }
        else
        {

           // var temp = CellManager.Instance.getInfo(type);
            renderer.sprite = Resources.Load<Sprite>("cell/" + type);
        }
        index = i;

        if(type == "counter")
        {
            bk.SetActive(false);
        }

        int ci = 0;
        if (isShop)
        {

            foreach (var cost in cellInfo.cost)
            {
                costCell[ci].init(cost.Key, cost.Value, true);
                costCell[ci].gameObject.SetActive(true);
                ci++;
            }
        }
        for (; ci < costCell.Length; ci++)
        {

            costCell[ci].gameObject.SetActive(false);
        }
    }

    bool canAfford()
    {
        if (!isShop)
        {
            return true;
        }
        return ShopManager.Instance.canAfford(type);
    }

    public CanvasGroup explainPanel;
   public Text explainText;

    private void OnMouseEnter()
    {
        if (isShop)
            DOTween.To(() => explainPanel.alpha, x => explainPanel.alpha = x, 1, 0.3f);

        //explainPanel.SetActive(true);

    }

    private void OnMouseExit()
    {
        DOTween.To(() => explainPanel.alpha, x => explainPanel.alpha = x, 0, 0.3f);

        //explainPanel.SetActive(false);
    }

    public override void OnMouseDown()
    {
        //if (GetComponent<PlayerCell>())
        //{
        //    ResourceManager.Instance.consumeResource("nut", 1);
        //}
        if (type == "leaf" ||   !canAfford())
        {
            failedToMove();
            return;
        }

        ShopGridController.Instance.moveCell(this);
        //index = GridController.Instance.moveCellToEmpty(this);
        //  if(index == -1)
        //  {
        //      Destroy(gameObject);
        //  }
    }

     
}
