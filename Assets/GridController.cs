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

    // Start is called before the first frame update
    void Start()
    {
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
                var go = new GameObject();
                cellParents.Add(go.transform);
                go.transform.position = new Vector3(xPosition, yPosition, 0);
                yPosition += cellSize;



                if (emptyCell == index)
                {

                }
                else if (originalPlayerCell == index)
                {
                    //var child = Instantiate(Resources.Load<GameObject>("cell/player"), cellParents[index].position, Quaternion.identity, cellParents[index]);
                    //child.GetComponent<GridCell>().init(index);
                    generateCell(index, "player");
                }
                else
                {
                    //var child = Instantiate(Resources.Load<GameObject>("cell/empty"), cellParents[index].position, Quaternion.identity, cellParents[index]);
                    //child.GetComponent<GridCell>().init(index);


                    generateCell(index, "leaf");
                }
                index++;


            }
            xPosition += cellSize;
        }
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

    public bool isPlayerAround(int index)
    {
        return isTwoIndexCrossAdjacent(index, playerCellIndex);
    }

    public Transform getPlayerTransform()
    {
        return cellParents[playerCellIndex];
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
    bool isMoving = false;



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

    float animTime = 0.3f;

    void destroy(GameObject go)
    {
        go.transform.DOScale(Vector3.zero, animTime);
        go.transform.DOLocalMoveY(1, animTime);
        Destroy(go, animTime);
    }
    IEnumerator moveCellAnim(GridCell cell) 
    {

        yield return null;
        moveCount++;

        //if is moving player, consume
        if(cell.cellInfo.isPlayer())
        {
            ResourceManager.Instance.consumeResource("nut", 1);
        }

        //draw a card
        var card = DeckManager.Instance.drawCard();
        var cardInfo = CellManager.Instance.getInfo(card);

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
        cell.transform.DOMove(emptyPosition, 0.3f);
        yield return new  WaitForSeconds(0.3f);


        cell.transform.parent = cellParents[originEmptyIndex];
        cell.index = originEmptyIndex;

        //calculate combination result
        var targetCell = cellParents[originEmptyIndex].GetComponentInChildren<GridItem>();
        var cell1String = cell.type;
        var cell2String = targetCell ? targetCell.type : "empty";
        var combination = CombinationManager.Instance.getCombinationResult(cell1String, cell2String);
        if (combination != null)
        {
            foreach (var pair in combination.result)
            {
                switch (pair.Key)
                {
                    case "resource":

                        var resource = new List<PairInfo<int>>() { };
                        resource.Add(new PairInfo<int>(cell2String, int.Parse(pair.Value)));
                        CollectionManager.Instance.AddCoins(transform.position, resource);
                        break;
                    case "destroy1":
                        addEmpty(movingCellIndex);
                        //addEmpty(cell.index);
                        destroy(cell.gameObject);
                        break;
                    case "destroy2":
                        destroy(targetCell.gameObject);
                        break;


                    case "generate":
                        //generate new item in target position, generate empty in origin position
                        generateCell(emptyCell, pair.Value);
                        break;
                    default:
                        Debug.LogError("not support combination restul " + pair.Key);
                        break;
                }
            }
        }

        yield return new WaitForSeconds(animTime);
        EventPool.Trigger("moveAStep");

        // if generate new item, don't update emptyCell
        if (combination != null && combination.result.ContainsKey("generate"))
        {
            //return cell.index;
        }
        else
        {

            //cell.transform.parent = cellParents[originEmptyIndex];
            //cell.transform.position = cellParents[originEmptyIndex].position;



            emptyCell = movingCellIndex;
            //generate(emptyCell, card);


            //if (cell.GetComponent<GridCell>().cellInfo.isPlayer())
            //{
            //    playerCell = originEmptyIndex;
            //}
            //StartCoroutine(test(originEmptyIndex, cell));
            //return originEmptyIndex;
        }
        generate(emptyCell, card);
        finishMove();
    }


    //public int moveCellToEmpty(GridCell cell)
    //{

    //    moveCount++;

    //    //draw a card
    //    var card = DeckManager.Instance.drawCard();

    //    var cardInfo = CellManager.Instance.getInfo(card);
    //    if (cardInfo.isCell())
    //    {
    //        //generate enemy
    //        if (cell.cellInfo.isEmpty())
    //        {
    //            Debug.Log("generate "+ card);
    //            //generate a snake
    //            generateCell(cell.index, card);

    //            //foreach (var item in cellParents[cell.index].GetComponentsInChildren<GridItem>())
    //            //{
    //            //    Destroy(item.gameObject);
    //            //}

    //            EventPool.Trigger("moveAStep");
    //            return -1;
    //        }
    //        else
    //        {
    //            DeckManager.Instance.addCardToDeck(card);
    //        }
    //    }




    //    var originEmptyIndex = emptyCell;


    //    var cell2 = cellParents[originEmptyIndex].GetComponentInChildren<GridItem>();
    //    var cell1String = cell.type;
    //    var cell2String = cell2 ? cell2.type : "empty";
    //    var combination = CombinationManager.Instance.getCombinationResult(cell1String, cell2String);
    //    if (combination != null)
    //    {
    //        foreach (var pair in combination.result)
    //        {
    //            switch (pair.Key)
    //            {
    //                case "resource":

    //                    var resource = new List<PairInfo<int>>() { };
    //                    resource.Add(new PairInfo<int>(cell2String, int.Parse(pair.Value)));
    //                    CollectionManager.Instance.AddCoins(transform.position, resource);
    //                    break;
    //                case "destroy1":
    //                    addEmpty(cell.index);
    //                    Destroy(cell.gameObject);
    //                    break;
    //                case "destroy2":
    //                    Destroy(cell2.gameObject);
    //                    break;


    //                case "generate":
    //                    generateCell(emptyCell, pair.Value);
    //                    break;
    //                default:
    //                    Debug.LogError("not support combination restul " + pair.Key);
    //                    break;
    //            }
    //        }
    //    }

    //    EventPool.Trigger("moveAStep");

    //    if (combination!=null &&combination.result.ContainsKey("generate"))
    //    {
    //        return cell.index;
    //    }
    //    else
    //    {

    //        cell.transform.parent = cellParents[originEmptyIndex];
    //        cell.transform.position = cellParents[originEmptyIndex].position;



    //        emptyCell = cell.index;
    //        generate(emptyCell, card);


    //        if (cell.GetComponent<GridCell>().cellInfo.isPlayer())
    //        {
    //            //playerCellIndex = originEmptyIndex;
    //        }
    //        //StartCoroutine(test(originEmptyIndex, cell));
    //        return originEmptyIndex;
    //    }


    //}



    IEnumerator test(int originEmptyIndex, GridCell cell)
    {
        //yield return new WaitForSeconds(0.1f);
        //trigger event if there is item on cell

        yield return new WaitForSeconds(0.1f);

        var cell2 = cellParents[originEmptyIndex].GetComponentInChildren<GridItem>();
        var cell1String = cell.type;
        var cell2String = cell2 ? cell2.type : "empty";
        var combination = CombinationManager.Instance.getCombinationResult(cell1String, cell2String);
        if (combination!=null)
        {
            foreach(var pair in combination.result)
            {
                switch (pair.Key)
                {
                    case "resource":

                        var resource = new List<PairInfo<int>>() { };
                        resource.Add(new PairInfo<int>(cell2String, int.Parse(pair.Value)));
                        CollectionManager.Instance.AddCoins(transform.position, resource);
                        break;
                    case "destroy1":
                        addEmpty(cell.index);
                        Destroy(cell.gameObject);
                        break;
                    case "destroy2":
                        Destroy(cell2.gameObject);
                        break;


                    case "generate":
                        generateCell(emptyCell, pair.Value);
                        break;
                    default:
                        Debug.LogError("not support combination restul " + pair.Key);
                        break;
                }
            }
        }
        //if (cellParents[originEmptyIndex].GetComponentInChildren<GridItem>())
        //{
        //    cellParents[originEmptyIndex].GetComponentInChildren<GridItemAction>().act(cell);
        //}




        //yield return new WaitForSeconds(0.1f);
        EventPool.Trigger("moveAStep");
        yield return new WaitForSeconds(0.1f);
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
                generateCell(index, "nut");
                break;
            case "trap":

                if (GameObject.FindObjectsOfType<TrapItem>().Length < 3)
                {
                    generateCell(index, "trap");
                }
                break;

        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
