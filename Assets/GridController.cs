using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : Singleton<GridController>
{
    public int cellCount = 3;
    public float cellSize = 2;

    int moveCount = 0;

    List<Transform> cellParents = new List<Transform>();

    int emptyCell = 0;
    int playerCell = 5;

    public int snakeChance = 20;
    public int nutChance = 40;
    public int trapChance = 15;

    // Start is called before the first frame update
    void Start()
    {
        float xStartPosiiton = -cellSize * (cellCount / 2);
        float yStartPosiiton = -cellSize * (cellCount / 2);
        // set up cell position
        float xPosition = xStartPosiiton;
        int index = 0;
        for (int i = 0; i < cellCount; i++)
        {
            float yPosition = yStartPosiiton;
            for (int j = 0; j < cellCount; j++)
            {
                var go = new GameObject();
                cellParents.Add(go.transform);
                go.transform.position = new Vector3(xPosition, yPosition, 0);
                yPosition += cellSize;



                if (emptyCell == index)
                {

                }
                else if (playerCell == index)
                {
                    var child = Instantiate(Resources.Load<GameObject>("cell/player"), cellParents[index].position, Quaternion.identity, cellParents[index]);
                    child.GetComponent<GridCell>().init(index);
                }
                else
                {
                    var child = Instantiate(Resources.Load<GameObject>("cell/empty"), cellParents[index].position, Quaternion.identity, cellParents[index]);
                    child.GetComponent<GridCell>().init(index);

                }
                index++;


            }
            xPosition += cellSize;
        }
        for (int i = 0; i < cellCount * cellCount; i++)
        {
        }
    }

    bool isTwoIndexCrossAdjacent(int i, int j)
    {
        var ix = i / 3;
        var iy = i % 3;
        var jx = j / 3;
        var jy = j % 3;
        if ((ix == jx && (iy == jy + 1 || iy == jy - 1)) ||
             (iy == jy && (ix == jx + 1 || ix == jx - 1)))
        {
            return true;
        }
        return false;
    }

    public bool isPlayerAround(int index)
    {
        return isTwoIndexCrossAdjacent(index, playerCell);
    }

    public Transform getPlayerTransform()
    {
        return cellParents[playerCell];
    }

    void generateCell(int index, string type)
    {

        var child = Instantiate(Resources.Load<GameObject>("cell/" + type), cellParents[index].position, Quaternion.identity, cellParents[index]);
        child.GetComponent<GridCell>().init(index);
    }

    void generateItem(int index, string type)
    {

        var child = Instantiate(Resources.Load<GameObject>("cell/" + type), cellParents[index].position, Quaternion.identity, cellParents[index]);
        //child.GetComponent<GridCell>().init(index);
    }

    public void addEmpty(int index)
    {
        generateCell(index, "empty");
    }


    public int moveCellToEmpty(GridCell cell)
    {

        moveCount++;

        //draw a card
        var card = DeckManager.Instance.drawCard();

        if (card == "snake")
        {
            //generate enemy
            if (!(cell.GetComponent<EnemyCell>() || cell.GetComponent<PlayerCell>()))
            {
                //generate a snake
                generateCell(cell.index, "snake");

                foreach (var item in cellParents[cell.index].GetComponentsInChildren<GridItem>())
                {
                    Destroy(item.gameObject);
                }

                EventPool.Trigger("moveAStep");
                return -1;
            }
            else
            {
                DeckManager.Instance.addCardToDeck(card);
            }
        }



        var originEmptyIndex = emptyCell;
        cell.transform.parent = cellParents[originEmptyIndex];
        cell.transform.position = cellParents[originEmptyIndex].position;



        emptyCell = cell.index;
        generate(emptyCell, card);


        if (cell.GetComponent<PlayerCell>())
        {
            playerCell = originEmptyIndex;
        }
        StartCoroutine(test(originEmptyIndex, cell));
        return originEmptyIndex;
    }

    IEnumerator test(int originEmptyIndex, GridCell cell)
    {
        //yield return new WaitForSeconds(0.1f);
        //trigger event if there is item on cell

        yield return new WaitForSeconds(0.1f);
        if (cellParents[originEmptyIndex].GetComponentInChildren<GridItem>())
        {
            cellParents[originEmptyIndex].GetComponentInChildren<GridItemAction>().act(cell);
        }


        //yield return new WaitForSeconds(0.1f);
        EventPool.Trigger("moveAStep");
        yield return new WaitForSeconds(0.1f);
    }

    void generate(int index, string card)
    {
        if (cellParents[index].GetComponentInChildren<GridItem>())
        {
            return;
        }
        else
        {
            DeckManager.Instance.addCardToDeck(card);
        }
        switch (card)
        {
            case "nut":
                generateItem(index, "nut");
                break;
            case "trap":

                if (GameObject.FindObjectsOfType<TrapItem>().Length < 3)
                {
                    generateItem(index, "trap");
                }
                break;

        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
