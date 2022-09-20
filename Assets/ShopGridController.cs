using DG.Tweening;
using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopGridController : Singleton<ShopGridController>
{
    public int cellCountX = 3;
    public int cellCountY = 3;
    public float cellSize = 2;

    List<Transform> cellParents = new List<Transform>();
    public Transform shopBoard;
    public GameObject shopCellPrefab;
    public GameObject bkPrefab;

    int emptyCell = 0;
    GridCell playerCell;
    GameObject counterCell;

   public  bool isMoving = false;
    int playerCellIndex { get { return playerCell.index; } }
    int originalPlayerCell = 5;


    public IEnumerator getIntoShop()
    {
        SFXManager.Instance.play("shopappear");
        yield return StartCoroutine(showCells());

    }

    public IEnumerator leaveShop()
    {
        SFXManager.Instance.play("shopdisappear");
        yield return StartCoroutine(hideCells());
        StartCoroutine(GridController.Instance.leaveShop());

    }

    float cellAnimInterval = 0.05f;
    IEnumerator showCells()
    {
        playerCell.transform.position = cellParents[playerCellIndex].position;

        foreach (var cell in cellParents)
        {
            cell.transform.localScale = Vector3.zero;
        }
        shopBoard.gameObject.SetActive(true);
        foreach (var cell in cellParents)
        {
            yield return new WaitForSeconds(cellAnimInterval);

            cell.transform.DOScale(Vector3.one, GridController.Instance.animTime);
        }

        yield return new WaitForSeconds(GridController.Instance.animTime);
    }

    IEnumerator hideCells()
    {
        foreach (var cell in cellParents)
        {
            cell.transform.localScale = Vector3.one;
        }

        foreach (var cell in cellParents)
        {
            yield return new WaitForSeconds(cellAnimInterval);

            cell.transform.DOScale(Vector3.zero, GridController.Instance.animTime);
        }

        yield return new WaitForSeconds(GridController.Instance.animTime);

        shopBoard.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {

        // initMainBoard();
        Utils.destroyAllChildren(shopBoard);
        initShopBoard();

    }

    void initShopBoard()
    {

        float xStartPosiiton = -cellSize * (cellCountX / 2);
        float yStartPosiiton = -cellSize * (cellCountY / 2);
        // set up cell position
        float xPosition = xStartPosiiton;
        int index = 0;
        int shopIndex = 0;
        for (int i = 0; i < cellCountX; i++)
        {
            float yPosition = yStartPosiiton;
            for (int j = 0; j < cellCountY; j++)
            {
                var go = Instantiate(bkPrefab, shopBoard);
                cellParents.Add(go.transform);
                go.transform.position = new Vector3(xPosition, yPosition, 0);
                yPosition += cellSize;

                if (originalPlayerCell == index)
                {
                    playerCell = generateShopCell(index, "player",false).GetComponent<GridCell>();

                }
                else if (emptyCell == index)
                {
                    counterCell = generateShopCell(index, "counter", false);
                }
                else if (shopIndex < ShopManager.Instance.unPurchasedShopInfos.Count)
                {

                    generateShopCell(index, ShopManager.Instance.unPurchasedShopInfos[shopIndex],true);
                    shopIndex++;
                }
                else
                {
                    generateShopCell(index, "leaf", false);
                }
                index++;
            }
            xPosition += cellSize;
        }
    }


    GameObject generateShopCell(int index, string type, bool isShop)
    {
        GameObject res;
        res = Instantiate(shopCellPrefab, cellParents[index].position, Quaternion.identity, cellParents[index]);

        //  res.transform.localScale = Vector3.one;
        res.transform.DOPunchScale(Vector3.one, GridController.Instance.animTime);

        res.GetComponent<ShopCell>().init(type, index, isShop);
        return res;
    }
    // Update is called once per frame
    void Update()
    {

    }
    public void moveCell(ShopCell cell)
    {

        if (isMoving || GridController.Instance.isMoving)
        {

            return;
        }
        isMoving = true;
        StartCoroutine(moveCellAnim(cell));
    }

    void destroy(GameObject go)
    {
        go.transform.DOScale(Vector3.zero, GridController.Instance.animTime);
        go.transform.DOLocalMoveY(1, GridController.Instance.animTime);
        Destroy(go, GridController.Instance.animTime);
    }
    IEnumerator moveCellAnim(ShopCell cell)
    {

        yield return null;
        
        

        //move current cell to position
        var originEmptyIndex = emptyCell;
        var movingCellIndex = cell.index;
        var emptyPosition = cellParents[originEmptyIndex].position;
        //cell.GetComponent<SortingGroup>().sortingOrder = 100;
        cell.transform.DOMove(emptyPosition, GridController.Instance.animTime);

       // generateShopCell(movingCellIndex, "counter", false);

        yield return new WaitForSeconds(GridController.Instance.animTime);

        if(cell == playerCell)
        {
            //leave shop
            emptyCell = movingCellIndex;
            cell.index = originEmptyIndex;
            Destroy(counterCell);
            counterCell = generateShopCell(emptyCell, "counter", false);
            StartCoroutine( leaveShop());
        }else if (cell.isShop)
        {
            //buy it
            ShopManager.Instance.purchase(cell.type);
            generateShopCell(movingCellIndex, "leaf", false);
            destroy(cell.gameObject);

        }


        cell.transform.parent = cellParents[originEmptyIndex];
        cell.index = originEmptyIndex;

        var targetCell = cellParents[originEmptyIndex].GetComponentInChildren<GridItem>();
        var cell1String = cell.type;
        var cell2String = targetCell ? targetCell.type : "empty";
        

        yield return new WaitForSeconds(GridController.Instance.animTime);
        //EventPool.Trigger("moveAStep");

        finishMove();



    }
    void finishMove()
    {
        isMoving = false;
        Debug.Log("empty index " + emptyCell);
    }
}
