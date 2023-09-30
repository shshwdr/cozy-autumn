using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Pool;

public class GridCell : MonoBehaviour
{
    public Vector2 index;
    public SpriteRenderer renderer;

    public GameObject bk;

    public CellInfo cellInfo;

   public  CounterDown countDownObject;
    public string type;

    public string equipment = null;
    public int equipementDamage;
    public Text equipementDamageLabel;
    public SpriteRenderer equipRenderer;
    public GameObject ice;
    public GameObject highlightOB;
    public GameObject enemyTargetOB;

    public Text displayName;

    public GameObject newRuleAlert;
    public CanvasGroup descriptionCanvas;
    public bool isFreezed = false;

    public Text amountLabel;
    public int amount;

    public GameObject HPBK;

    public Collider2D collider;


    public Transform birdItemsParent;
    public string birdItem;

    public GameObject mask;

    public bool isTarget = false;

    private void Awake()
    {
        collider = GetComponent<Collider2D>();
        if (highlightOB)
        {

            highlightOB.SetActive(false);
        }
    }

    public void showEnemyTarget()
    {
        if (enemyTargetOB)
        {
            enemyTargetOB.SetActive(true);
        }
    }
    public void hideEnemyTarget()
    {
        if (enemyTargetOB)
            enemyTargetOB.SetActive(false);

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

        if (!ice)
        {
            Debug.Log("??");
            return;
        }
        ice.SetActive(false);
    }

    void updateBackground()
    {
        if(type == "player")
        {

            bk.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("cell/blank/hero");
            HPBK.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("cell/blank/alternative-1-corner-left");
        }
        else if (cellInfo.isEmpty())
        {
            bk.SetActive(false);
            HPBK.SetActive(false);
            amountLabel.gameObject.SetActive(false);
        }
        else if (cellInfo.isEnemy())
        {

            bk.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("cell/blank/enemy");
            HPBK.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("cell/blank/enemy-corner-left");
        }
        else if (cellInfo.isAlly())
        {

            bk.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("cell/blank/ally");
            HPBK.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("cell/blank/ally-corner-left");
        }
        else if (cellInfo.isTrap() || cellInfo.isWeapon())
        {

            bk.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("cell/blank/alternative-2");
            HPBK.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("cell/blank/alternative-2-corner-left");
        }

        else
        {
            bk.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("cell/blank/default");
            HPBK.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("cell/blank/default-corner-left");
            var color = Color.black;
            ColorUtility.TryParseHtmlString("#6c7680", out color);
            amountLabel.color = color;
        }

    }

    public virtual  void init(string _type,Vector2 i,int _amount)
    {
        CellManager.Instance.showCell(_type);
        type = _type;
        cellInfo = CellManager.Instance.getInfo(type);
        renderer.sprite = Resources.Load<Sprite>("cell/" + type);
        index = i;
        bk.SetActive(cellInfo.isCell());
        displayName.text = cellInfo.displayName;

        if (cellInfo.type == "fire")
        {
            gameObject.AddComponent<FireCell>();
            bk.GetComponent<SpriteRenderer>().color = new Color(1,0.5f, 0.5f);
        }
        
        if (_amount > 0)
        {
            amount = _amount;
        }
        else
        {
            amount = cellInfo.categoryValue;

            //if (cellInfo.categoryValue > 0)
            //{
            //    amount = cellInfo.categoryValue;
            //    amountLabel.gameObject.SetActive(true);
            //}
            //else
            //{
            //    amount = 1;
            //    if (amountLabel)
            //    {

            //        amountLabel.gameObject.SetActive(false);
            //    }
            //}
        }


        if (cellInfo.isEnemy())
        {
            gameObject.AddComponent<EnemyCell>().init(type);
           // bk.GetComponent<SpriteRenderer>().color = Color.red;
        }

        updateAmount();

        equipementDamage = 0;
        updateEquipmentDamage();

        equipment = null;
        if (equipRenderer!=null)
        {

            equipRenderer.sprite = Resources.Load<Sprite>("cell/" + "empty");
        }


        if(type == "itemBird")
        {
            //
            var itemDropID = Random.Range(0, birdItemsParent.childCount);
            var itemDrop = birdItemsParent.GetChild(itemDropID).gameObject;
            itemDrop.SetActive(true);
            birdItem = itemDrop.name;
        }

        //if (cellInfo.isEnemy())
        //{
        //    HPBK.gameObject.SetActive(true);
        //}else
        //{
        //    if (HPBK)
        //    {
        //        HPBK.gameObject.SetActive(false);

        //    }

        //}
        updateBackground();
    }



    public void updateAmount()
    {
        if(amountLabel == null)
        {
            return;
        }
        //if (amount > 1)
        {
            amountLabel.text = amount.ToString();
        }
        //else
        //{
        //    amountLabel.text = "";
        //}
    }

    public void setAmount(int x)
    {
        amount = x;
        updateAmount();
    }
    public virtual void addAmount(int x = 1)
    {
        amount += x;
        updateAmount();
    }
    public virtual void decreaseAmount(int x = 1)
    {
        amount -= x;
        updateAmount();

    }

    public void equip(string e, int am)
    {
        equipment = e;
        equipRenderer.sprite = Resources.Load<Sprite>("cell/" + equipment);

        equipRenderer.transform.DOPunchScale(equipRenderer.transform.localScale*2, GridController.Instance.animTime);

        SFXManager.Instance.play("equipweapon");
        equipementDamage = am;
        updateEquipmentDamage();
    }

    public bool hasEquipment()
    {
        return equipment != null && equipment!="";
    }


    private void OnDestroy()
    {
        transform.DOKill();
        descriptionCanvas.DOKill();


        if (target)
        {
            target.hideEnemyTarget();
            Destroy(targetOb);
            targetOb = null;
        }
    }

    void updateEquipmentDamage()
    {
        if (!equipementDamageLabel)
        {
            return;
        }
        if (equipementDamage <= 0)
        {
            equipementDamageLabel.text = "";
        }
        else
        {

            equipementDamageLabel.text = equipementDamage.ToString();
        }
    }
    public void attackWithEquipement(int value)
    {
        equipementDamage -= value;
        if (equipementDamage <= 0)
        {
            unequip(transform);
        }
        updateEquipmentDamage();
    }
    public void unequip(Transform targetTransform)
    {
        if (equipment == null)
        {
            Debug.LogError("nothing to unequip");
        }

        //var go = Instantiate(Resources.Load<GameObject>("flyingObject"), equipRenderer.transform.position, Quaternion.identity);
        //go.transform.localScale = equipRenderer.transform.localScale;
        //go.GetComponent<FlyingObject>().init(equipRenderer.sprite, targetTransform.position);

        equipment = null;
        equipRenderer.sprite = Resources.Load<Sprite>("cell/" + "empty");

    }
    // Start is called before the first frame update
    void Start()
    {
        EventPool.OptIn("moveAStep", step);
    }

    GridCell willSwapCell;

    bool pointHovered(Vector2 point)
    {
        return collider.OverlapPoint(point);
       // return collider.bounds.Contains(point);
    }

    public void select()
    {
        highlightOB.SetActive(true);
    }
    public void unselect()
    {
        highlightOB.SetActive(false);
    }

    List<GameObject> generatedCombineResult = new List<GameObject>();
    void clearGeneratedCombineResult()
    {

        foreach (var c in generatedCombineResult)
        {
            Destroy(c);
        }
        generatedCombineResult.Clear();
    }
    // Update is called once per frame
    virtual public void Update()
    {


        if (isMouseDown)
        {

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if(willSwapCell && willSwapCell.pointHovered( mousePosition))
            {
                return;
            }
            if (willSwapCell)
            {
                clearGeneratedCombineResult();
                willSwapCell.unselect();
                willSwapCell = null;
            }
            //check if neighbour contains point
            foreach(var neigh in GridGeneration.Instance.getSurroundingCells(index))
            {
                if (neigh.pointHovered(mousePosition)){



                    //StartCoroutine(GridGeneration.Instance.calculateCombinedResult(new List<GridCell>(){ neigh,this} , generatedCombineResult));

                    neigh.select();
                    willSwapCell = neigh;
                    break;
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (NewCellManager.Instance.isQueueEmpty())
                {

                    Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    if (pointHovered(mousePosition))
                    {
                        NewCellManager.Instance.showCellInfo(type);
                    }
                }
            }
        }


    }

    public virtual void step()
    {
        var rule = GridController.Instance.combineResult(this);
        if (rule!=null && !RuleManager.Instance.visitedList.Contains(rule))
        {
            newRuleAlert.SetActive(true);
            descriptionCanvas.transform.Find("attackWhenNext").gameObject.SetActive(true);
            HintCell.generateHintText(descriptionCanvas.transform.Find("attackWhenNext"), "Move it now to unlock new rule!");
            descriptionCanvas.transform.Find("attackPerRound").gameObject.SetActive(false);
            descriptionCanvas.transform.Find("notNut").gameObject.SetActive(false);
            descriptionCanvas.transform.Find("Move").gameObject.SetActive(false);
            descriptionCanvas.transform.Find("Move2").gameObject.SetActive(false);
        }
        else
        {

            newRuleAlert.SetActive(false);
        }

        if(cellInfo!=null && cellInfo.hpMode != 0)
        {
            addAmount(cellInfo.hpMode);
            if (amount <= 0)
            {
                StartCoroutine(GridGeneration.Instance.destroy(gameObject));
            }
        }
    }

    bool isMouseDown = false;
    float mouseDownTime;
    float longPressTime = 0.3f;
    public virtual void OnMouseUp()
    {
        if (!isMouseDown)
        {
            return;
        }
        if (willSwapCell)
        {
            willSwapCell.unselect();

            StartCoroutine(GridGeneration.Instance.swap(willSwapCell, this));

            willSwapCell = null;
            clearGeneratedCombineResult();
        }
        //Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //if (GetComponent<Collider2D>().OverlapPoint(mousePosition))
        //{
        //    if (Time.time - mouseDownTime > longPressTime)
        //    {

        //        //GridController.Instance.exploreCell(this);
        //        GridController.Instance.moveCell(this,true);

        //        GridController.Instance.playerCell.decreaseAmount(1);
        //        //ResourceManager.Instance.consumeResource("nut", 1, transform.position);
        //    }
        //    else
        //    {

        //        GridController.Instance.moveCell(this,false);
        //    }
        //}







        isMouseDown = false;

        //ExploreHoldText.text = "";
        //progressBar.fillAmount = 0;
    }

    public Image progressBar;
    public Text ExploreHoldText;

    
    private void OnMouseOver()
    {



        //Debug.Log("just over");
        if (isMouseDown)
        {
            
            //progressBar.fillAmount = (Time.time - mouseDownTime) / longPressTime;
            //if (progressBar.fillAmount >= 1)
            //{
            //    progressBar.color = Color.black;
            //}
            //else
            //{

            //    progressBar.color = Color.green;
            //}
            //Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //if (!GetComponent<Collider2D>().OverlapPoint(mousePosition))
            //{

            //    ExploreHoldText.text = "Cancel";
            //   // progressBar.fillAmount = 0;
            //    return;
            //}

            //update progress bar

           
        }
    }

    public virtual void OnMouseDown()
    {
        if (!GridGeneration.Instance.canSwap())
        {
            return;
        }
        if (!EventSystem.current.IsPointerOverGameObject())
        {

            Collider2D colliderHit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (colliderHit != GetComponent<Collider2D>())
            {
                return;
            }

            //if (GetComponent<PlayerCell>())
            //{
            //    ResourceManager.Instance.consumeResource("nut", 1);
            //}
            if (isFreezed)
            {

                FindObjectOfType<AchievementManager>().ShowAchievement("freezeMove");
                failedToMove();
                return;
            }
            isMouseDown = true;
            mouseDownTime = Time.time;
            //ExploreHoldText.text = "Move";
            //index = GridController.Instance.moveCellToEmpty(this);
            //  if(index == -1)
            //  {
            //      Destroy(gameObject);
            //  }
        }
    }
    public void failedToMove()
    {
        transform.DOKill();
        transform.DOShakePosition(0.3f, 0.3f, 20);

        SFXManager.Instance.play("negative");
    }

    GridCell target;
    GameObject targetOb;
    private void OnMouseEnter()
    {
        if (newRuleAlert.active)
        {

            DOTween.To(() => descriptionCanvas.alpha, x => descriptionCanvas.alpha = x, 1, 0.3f);
        }

        if(cellInfo == null)
        {
            Debug.LogError("?");
            return;
        }

        if (cellInfo.isEnemy())
        {
            //show enemy glow for targets
            Vector2 nextPosition = GridGeneration.Instance.enemyTargets(this, out target);
            target.showEnemyTarget();



            targetOb = GridGeneration.Instance.generateTargetCell(index + GridGeneration.Instance.getDir( nextPosition));
        }

        //explainPanel.SetActive(true);

    }


    private void OnMouseExit()
    {
        DOTween.To(() => descriptionCanvas.alpha, x => descriptionCanvas.alpha = x, 0, 0.3f);
        if (target)
        {
            target.hideEnemyTarget();
            Destroy(targetOb);
            targetOb = null;
        }
        //explainPanel.SetActive(false);
    }
}
