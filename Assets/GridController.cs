using DG.Tweening;
using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GridController : Singleton<GridController>
{
    public int cellCountX = 3;
    public int cellCountY= 3;
    public float cellSize = 2;

    int moveCount = 0;

    List<Transform> cellParents = new List<Transform>();

    int emptyCell = 0;
    GridCell playerCell;
    int playerCellIndex { get { return playerCell.index; } }
    int originalPlayerCell = 5;

    public int snakeChance = 20;
    public int nutChance = 40;
    public int trapChance = 15;

    public GameObject cellPrefab;
    public GameObject itemPrefab;
    public GameObject bkPrefab;

    public Transform mainBoard;

    public GameObject fireVFX;


    float animTime = 0.3f;
   public  bool isMoving = false;
    // Start is called before the first frame update
    void Start()
    {
        // initMainBoard();
        initMainBoard();

    }

    void initMainBoard()
    {
        isMoving = true;
        float xStartPosiiton = -cellSize * (cellCountX / 2);
        float yStartPosiiton = -cellSize * (cellCountY / 2);
        // set up cell position
        float xPosition = xStartPosiiton;
        int index = 0;
        for (int i = 0; i < cellCountX; i++)
        {
            float yPosition = yStartPosiiton;
            for (int j = 0; j < cellCountY; j++)
            {
                var go = Instantiate(bkPrefab, mainBoard);
                cellParents.Add(go.transform);
                go.transform.position = new Vector3(xPosition, yPosition, 0);
                yPosition += cellSize;

                if (emptyCell == index)
                {

                }
                else if (originalPlayerCell == index)
                {
                    playerCell = generateCell(index, "player").GetComponent<GridCell>();

                }
                else
                {
                    generateCell(index, "leaf");
                }
                index++;
            }
            xPosition += cellSize;
        }

        StartCoroutine(showCells());
    }

    
    bool isTwoIndexCrossAdjacent(int i, int j)
    {
        var ix = i / cellCountX;
        var iy = i % cellCountY;
        var jx = j / cellCountX;
        var jy = j % cellCountY;
        if ((ix == jx && (iy == jy + 1 || iy == jy - 1)) ||
             (iy == jy && (ix == jx + 1 || ix == jx - 1)))
        {
            return true;
        }
        return false;
    }

    void addIndices(List<int>  indices, int x, int y)
    {
        if(x>=0&&x< cellCountX && y >= 0 && y < cellCountY)
        {
            indices.Add(x * cellCountX + y);
        }
    }

    public List<GridCell> adjacentType(int i,string type)
    {
        List<GridCell> res = new List<GridCell>();
        var ix = i / cellCountX;
        var iy = i % cellCountY;
        List<int> indices = new List<int>();
        addIndices(indices, ix - 1, iy);
        addIndices(indices, ix + 1, iy);
        addIndices(indices, ix, iy - 1);
        addIndices(indices, ix, iy + 1);
        foreach (var cell in FindObjectsOfType<GridCell>())
        {
            if (indices.Contains(cell.index) && isType(cell, type))
            {
                res.Add(cell);
            }
        }
        return res;
    }

    public IEnumerator getIntoShop()
    {

        yield return StartCoroutine(hideCells());

       // mainBoard.gameObject.SetActive(false);
        StartCoroutine( ShopGridController.Instance.getIntoShop());
    }


    public IEnumerator leaveShop()
    {
        // mainBoard.gameObject.SetActive(true);
        yield return StartCoroutine(showCells());
    }

        bool isType(GridCell cell, string type)
    {
        if(type == "ice")
        {
            return cell.isFreezed;
        }
        return cell.cellInfo.type == "type";
    }

    bool isAdjacentToType(int i, string type)
    {

        var ix = i / cellCountX;
        var iy = i % cellCountY;
        List<int> indices = new List<int>();
        addIndices(indices, ix - 1, iy);
        addIndices(indices, ix + 1, iy);
        addIndices(indices, ix, iy - 1);
        addIndices(indices, ix, iy + 1);
        foreach (var cell in FindObjectsOfType<GridCell>())
        {
            if (indices.Contains(cell.index) && isType(cell,type))
            {
                return true;
            }
        }
        return false;
    }
    bool isAdjacentToIce(int i)
    {
        return isAdjacentToType(i, "ice");
    }

    bool isAdjacentToFire(int i)
    {
        return isAdjacentToType(i, "fire");
    }

    public bool isPlayerAround(int index)
    {
        return isTwoIndexCrossAdjacent(index, playerCellIndex);
    }

    public Transform getPlayerTransform()
    {
        return playerCell.transform;
    }
    GameObject generateCell(int index, string type)
    {
        GameObject res;
        if (CellManager.Instance.isCell(type))
        {

            res = Instantiate(cellPrefab, cellParents[index].position, Quaternion.identity, cellParents[index]);
        }
        else
        {

            res = Instantiate(itemPrefab, cellParents[index].position, Quaternion.identity, cellParents[index]);
        }

      //  res.transform.localScale = Vector3.one;
        res.transform.DOPunchScale(Vector3.one, animTime);

        res.GetComponent<GridCell>().init(type, index);
        return res;
    }

    //void generateItem(int index, string type)
    //{

    //    var child = Instantiate(Resources.Load<GameObject>("cell/" + type), cellParents[index].position, Quaternion.identity, cellParents[index]);
    //    //child.GetComponent<GridCell>().init(index);
    //}

    public void addEmpty(int index)
    {
        generateCell(index, "leaf");
    }

    int freezeCount()
    {
        int res = 0;
        foreach (var c in GameObject.FindObjectsOfType<GridCell>())
        {
            if (c.isFreezed)
            {
                res++;
            }
        }
        return res;
    }

    int itemCount(string type)
    {
        int res = 0;
        foreach (var c in GameObject.FindObjectsOfType<GridCell>())
        {
            if (c.cellInfo.type == type)
            {
                res++;
            }
        }
        return res;
    }

    public void moveCell(GridCell cell)
    {

        if (isMoving)
        {

            return;
        }
        isMoving = true;
        StartCoroutine(moveCellAnim(cell));
    }

    void finishMove()
    {
        isMoving = false;
        Debug.Log("empty index " + emptyCell);
    }
    public bool hasPlayerMoved = false;
    void destroy(GameObject go)
    {
        go.transform.DOScale(Vector3.zero, animTime);
        go.transform.DOLocalMoveY(1, animTime);
        Destroy(go, animTime);
    }
    IEnumerator moveCellAnim(GridCell cell) 
    {
        //FindObjectOfType<Doozy.Examples.E12PopupManagerScript>().ShowAchievement(0);
        yield return null;
        moveCount++;

        //if is moving player, consume
        if(cell.cellInfo.isPlayer())
        {
            ResourceManager.Instance.consumeResource("nut", 1);
            RulePopupManager.Instance.showRule("playerMove");
        }


        //draw a card
        var card = DeckManager.Instance.drawCard();
        Debug.Log("draw card " + card);
        var cardInfo = CellManager.Instance.getInfo(card);
        var freezedCellCount = freezeCount();
        if (cardInfo.type == "ice")
        {
            //if has ice, don't add
            if (freezedCellCount == 0)
            {
                RulePopupManager.Instance.showRule("ice");
                List<GridCell> canFreezeCells = new List<GridCell>();
                foreach(var c in GameObject.FindObjectsOfType<GridCell>())
                {
                    if(c.cellInfo.type!="fire" && !isAdjacentToFire(c.index))
                    {
                        canFreezeCells.Add(c);
                    }
                }

                if (canFreezeCells.Count > 0)
                {
                    var freeCell = Utils.randomList(canFreezeCells);
                    freeCell.freeze();
                }
            }
        }else if (GameObject.FindObjectsOfType<GridCell>().Length > 0)
        {
            //ice spread

            if (ShopManager.Instance.hasPurchased("fire") && Random.Range(0, 2) > 0)
            {
            }
            else
            {
                foreach (var c in GameObject.FindObjectsOfType<GridCell>())
                {
                    if (!c.GetComponent<GridItem>() && !c.GetComponent<ShopCell>() && c.cellInfo.type != "fire" && !isAdjacentToFire(c.index) && c.cellInfo.type != "ice" && isAdjacentToIce(c.index))
                    {
                        c.freeze();

                        RulePopupManager.Instance.showRule("iceSpread");
                        //if freezed everything, game over
                        if (freezedCellCount >= 8)
                        {

                            GameManager.Instance.gameover();
                        }
                        break;
                    }
                }

            }
        }

        // if it is cell card, don't move it, but destroy and replace it to the cell card
        // empty position not change.
        if (cardInfo.isCell())
        {
            //generate cell, if already a cell, don't generate and add it back to the deck.
            if (cell.cellInfo.isEmpty())
            {
                Debug.Log("generate " + card);
                //generate a snake
                var go = generateCell(cell.index, card);
                //go.transform.DOShakeScale(0.3f);
                destroy(cell.gameObject);
                //foreach (var item in cellParents[cell.index].GetComponentsInChildren<GridItem>())
                //{
                //    Destroy(item.gameObject);
                //}

                EventPool.Trigger("moveAStep");

                yield return new WaitForSeconds(0.3f);
                finishMove();
                yield break;
            }
            else
            {
                DeckManager.Instance.addCardToDeck(card);
            }
        }


        //move current cell to position
        var originEmptyIndex = emptyCell;
        var movingCellIndex = cell.index;
        var emptyPosition = cellParents[originEmptyIndex].position;
        //cell.GetComponent<SortingGroup>().sortingOrder = 100;
        cell.transform.DOMove(emptyPosition, animTime);
        generate(movingCellIndex, card);
        yield return new  WaitForSeconds(animTime);


        cell.transform.parent = cellParents[originEmptyIndex];
        cell.index = originEmptyIndex;

        var targetCell = cellParents[originEmptyIndex].GetComponentInChildren<GridItem>();
        var cell1String = cell.type;
        var cell2String = targetCell ? targetCell.type : "empty";
        if (cell.GetComponent<GridCell>().cellInfo.isPlayer())
        {
            hasPlayerMoved = true;
            if (targetCell)
            {
                if (targetCell.cellInfo.isResource())
                {
                    var resource = new List<PairInfo<int>>() { };
                    resource.Add(new PairInfo<int>(targetCell.cellInfo.categoryDetail, targetCell.cellInfo.categoryValue));
                    CollectionManager.Instance.AddCoins(targetCell.transform.position, resource);
                    destroy(targetCell.gameObject);

                    switch (targetCell.type)
                    {
                        case "nut":

                            RulePopupManager.Instance.showRule("playerToNut");
                            break;
                        case "cookedNut":
                            RulePopupManager.Instance.showRule("eatHotNut");
                            break;
                            

                    }

                }
                else if (targetCell.cellInfo.isWeapon())
                {
                    cell.GetComponent<GridCell>().equip(cell2String);
                    destroy(targetCell.gameObject);
                }else if (targetCell.cellInfo.type == "shop")
                {

                    destroy(targetCell.gameObject);
                    StartCoroutine( getIntoShop());
                }
            }
            emptyCell = movingCellIndex;
        }
        else
        {

            hasPlayerMoved = false;
            //calculate combination result
            var combination = CombinationManager.Instance.getCombinationResult(cell1String, cell2String);
            if (combination != null)
            {
                if(combination.rules!=null && combination.rules.Length > 0)
                {

                    RulePopupManager.Instance.showRule(combination.rules);
                }
                foreach (var pair in combination.result)
                {
                    switch (pair.Key)
                    {
                        //case "resource":

                        //    var resource = new List<PairInfo<int>>() { };
                        //    CellInfo info = CellManager.Instance.getInfo(cell2String);

                        //    resource.Add(new PairInfo<int>(info.categoryDetail, int.Parse(pair.Value)));
                        //    CollectionManager.Instance.AddCoins(transform.position, resource);
                        //    break;
                        case "destroy1":
                            addEmpty(originEmptyIndex);
                            destroy(cell.gameObject);
                            break;
                        case "destroy2":
                            destroy(targetCell.gameObject);
                            break;
                        case "addHot":
                            cellParents[movingCellIndex].GetComponent<GridBackground>().heat();
                            Instantiate(fireVFX, cellParents[movingCellIndex].position, Quaternion.identity);
                            break;

                        case "generate1":
                            //generate new item in target position, generate empty in origin position
                            addEmpty(movingCellIndex);
                            destroy(cell.gameObject);
                            generateCell(emptyCell, pair.Value);
                            break;
                        case "generate2":
                            //generate new item in original position
                            //addEmpty(originEmptyIndex);
                            //destroy(cell.gameObject);

                            if (cellParents[movingCellIndex].GetComponentInChildren<GridItem>())
                            {
                                destroy( cellParents[movingCellIndex].GetComponentInChildren<GridItem>().gameObject);
                                //DeckManager.Instance.addCardToDeck(card);
                            }

                            generateCell(movingCellIndex, pair.Value);
                            break;
                        case "increaseObjectHP":
                            cell.GetComponent<FireCell>().addHp(int.Parse(pair.Value));
                            break;
                        case "trap":

                            RulePopupManager.Instance.showRule("snakeToTrap");
                            FindObjectOfType<Doozy.Examples.E12PopupManagerScript>().ShowAchievement(1);
                            break;
                        default:
                            Debug.LogError("not support combination restul " + pair.Key);
                            break;
                    }
                }
            }

            // if generate new item on new position, don't update emptyCell
            if (combination != null && combination.result.ContainsKey("generate1"))
            {

            }
            else
            {
                emptyCell = movingCellIndex;

            }


        }


        //check if cell is on hot cell
        if (cell && cellParents[cell.index].GetComponent<GridBackground>().isHot)
        {
            if (cell.GetComponent<GridCell>().cellInfo.isPlayer())
            {

                RulePopupManager.Instance.showRule("playerOnHot");
                ResourceManager.Instance.consumeResource("nut", 3);

                Instantiate(fireVFX, cellParents[cell.index].position, Quaternion.identity);
            }
            if (cell.GetComponent<GridCell>().cellInfo.isEnemy())
            {
                RulePopupManager.Instance.showRule("enemyOnHot");
                cell.GetComponent<EnemyCell>().getDamage(5);
                Instantiate(fireVFX, cellParents[cell.index].position, Quaternion.identity);
            }

            if (cell.GetComponent<GridCell>().cellInfo.type == "branch")
            {
                //generate a fire
                generateCell(cell.index, "fire");
                destroy(cell.gameObject);
                Instantiate(fireVFX, cellParents[cell.index].position, Quaternion.identity);

            }


        }


        yield return new WaitForSeconds(animTime);

        if (targetCell &&  targetCell.cellInfo.type != "shop")
        {

            EventPool.Trigger("moveAStep");
        }

        finishMove();




        if (cell.cellInfo.type == "fire")
        {
            cell.GetComponent<FireCell>().getDamage(1);
            RulePopupManager.Instance.showRule("fireMove");
        }

    }


    public int getTargetIndexToPlayer(int index)
    {
        if(index == playerCellIndex)
        {
            Debug.LogError("on the same position as player " + index);
        }

        int x = index / cellCountX;
        int y = index % cellCountY;
        int px = playerCellIndex / cellCountX;
        int py = playerCellIndex / cellCountY;
        if(x== px)
        {
            y = (int)(Mathf.Sign(py - y)) + y;
        }
        else
        {

            x = (int)(Mathf.Sign(px - x)) + x;
        }

        return x * cellCountX + y;
    }

    public IEnumerator exchangeCard(GridCell cell1, int cell2Index)
    {
        isMoving = true;
        if (cell1.index == cell2Index)
        {
            Debug.Log("that's not correct "+ cell2Index);
        }

        var cell1Index = cell1.index;
        var cell2 = cellParents[cell2Index].GetComponentInChildren<GridCell>();
        var cell1Position = cellParents[cell1Index].position;
        var cell2Position = cellParents[cell2Index].position;
        //cell.GetComponent<SortingGroup>().sortingOrder = 100;
        cell1.transform.DOMove(cell2Position, animTime);
        cell2.transform.DOMove(cell1Position, animTime);
        cell1.index = cell2Index;
        cell2.index = cell1Index;
        Debug.Log("cell1 " + cell2Index + " cell2 " + cell1Index);
        yield return new WaitForSeconds(animTime);

        isMoving = false;
    }

    float cellAnimInterval = 0.05f;
    IEnumerator showCells()
    {
        foreach (var cell in cellParents)
        {
            cell.transform.localScale = Vector3.zero;
                }

        foreach (var cell in cellParents)
        {
            yield return new WaitForSeconds(cellAnimInterval);

            cell.transform.DOScale(Vector3.one, animTime);
        }

        yield return new WaitForSeconds(animTime);
        isMoving = false;
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

            cell.transform.DOScale(Vector3.zero, animTime);
        }

        yield return new WaitForSeconds(animTime);
    }



    void generate(int index, string card)
    {
        if (cellParents[index].GetComponentInChildren<GridItem>())
        {
            DeckManager.Instance.addCardToDeck(card);
            return;
        }
        else
        {
        }
        switch (card)
        {
            case "nut":
            case "bat":
                generateCell(index, card);
                break;
            case "trap":

                if (itemCount(card) < 3)
                {
                    generateCell(index, "trap");
                }
                break;
            case "shop":

                if (itemCount(card) < 1)
                {
                    generateCell(index, "shop");
                }
                break;


        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
