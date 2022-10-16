using DG.Tweening;
using Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGeneration : Singleton<GridGeneration>
{
    public float cellSize = 2.1f;
    Vector2 playerPosition { get { return playerCell.index; } }
    [HideInInspector] public GridCell playerCell;
    public Transform mainBoard;
    public GameObject cellPrefab;
    public float animTime = 0.3f;

    Dictionary<Vector2, GridCell> indexToCell = new Dictionary<Vector2, GridCell>();

    public int swapTime = 1;
    public int currentSwapTime = 0;
    bool isMoving = false;

    Vector2[] dirs = new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };
    // Start is called before the first frame update
    void Start()
    {

        playerCell = generateCell(playerPosition, "player").GetComponent<GridCell>();
        playerCell.renderer.sprite = Resources.Load<Sprite>("cell/" + CharacterManager.Instance.currentChar);

        TetrisGeneration.Instance.generateATetrisShape();

        StartCoroutine(placeCellsAnim(new List<GameObject>() { playerCell.gameObject }));
    }
    public bool canSwap()
    {
        return currentSwapTime < swapTime &&!isMoving;
    }
    public bool canMoveCell()
    {
        return !isMoving;
    }

    public IEnumerator swap(GridCell cell1, GridCell cell2)
    {
        currentSwapTime++;
        EventPool.Trigger("updateSwap");
        isMoving = true;
        yield return StartCoroutine( exchangeCardAnim(cell1, cell2));


        List<Vector2> mightUpdatedPositions = new List<Vector2>();
        addFarerCellIntoPositions(mightUpdatedPositions, cell1.index);
        addFarerCellIntoPositions(mightUpdatedPositions, cell2.index);

        yield return StartCoroutine(movePositions(mightUpdatedPositions));

        isMoving = false;

    }
    void resetSwap()
    {
        currentSwapTime = 0;
        EventPool.Trigger("updateSwap");
    }
    public bool isOccupied(Vector2 index)
    {
        Vector2Int ind = Vector2Int.RoundToInt(index);
        return indexToCell.ContainsKey(ind) && indexToCell[ind] != null;
    }

    public void occupy(Vector2 index, GridCell cell)
    {
        Vector2Int ind = Vector2Int.RoundToInt(index);

        indexToCell[ind] = cell;
        cell.index = ind;
    }

    public void release(Vector2 index)
    {

        Vector2Int ind = Vector2Int.RoundToInt(index);
        if (!indexToCell.ContainsKey(ind))
        {
            Debug.LogError("release not existed index " + ind);
        }
        indexToCell.Remove(ind);
    }

    public bool isNextToOccupiedCells(Vector2 index)
    {
        if (getSurroundingCells(index).Count > 0)
        {
            return true;
        }
        return false;
    }

    public List<GridCell> getSurroundingCells(Vector2 index)
    {
        List<GridCell> cells = new List<GridCell>();
        foreach (var dir in dirs)
        {
            var newIndex = index + dir;
            var newCell = getCellOnPosition(newIndex);
            if (newCell)
            {
                cells.Add(newCell);
            }
        }
        return cells;
    }

    GridCell getCellOnPosition(Vector2 index)
    {

        Vector2Int ind = Vector2Int.RoundToInt(index);
        if (!indexToCell.ContainsKey(ind))
        {
            return null;
        }
        return indexToCell[ind];
    }

    public void placeCells(List<GameObject> cells)
    {
        StartCoroutine(placeCellsAnim(cells));
    }
    IEnumerator destroy(GameObject go)
    {
        go.transform.DOScale(Vector3.zero, animTime);
        go.transform.DOLocalMoveY(1, animTime);
        release(go.GetComponent<GridCell>().index);
        yield return new WaitForSeconds(animTime);
        Destroy(go);
    }
    IEnumerator moveCardAnim(GridCell cell, Vector2 targetPosition)
    {

        yield return null;
        SFXManager.Instance.play("cardmove");

        cell.transform.DOMove(targetPosition, animTime);

        yield return new WaitForSeconds(animTime);
    }

    IEnumerator moveCardAndOccupyAnim(GridCell cell, Vector2 targetPosition)
    {
        if (!cell)
        {
            Debug.LogError("no cell");
        }
        release(cell.index);

        occupy(targetPosition, cell);
        yield return StartCoroutine(moveCardAnim(cell, targetPosition));
    }
    IEnumerator exchangeCardAnim(GridCell cell, GridCell cell2)
    {
        var cellIndex = cell.index;
        var cellIndex2 = cell2.index;
        if(cellIndex == cellIndex2)
        {
            yield break;//hack fix
        }
        release(cellIndex);

        release(cellIndex2);
        StartCoroutine(moveCardAnim(cell, cell2.index));
        yield return StartCoroutine(moveCardAnim(cell2, cell.index));
        occupy(cellIndex2, cell);
        occupy(cellIndex, cell2);
    }


    IEnumerator combineAround(GridCell cell)
    {
        yield return null;
        bool hasCombined = false;
        foreach (var dir in dirs)
        {
            var newIndex = cell.index + dir;
            var newCell = getCellOnPosition(newIndex);
            if (newCell == null)
            {
                continue;
            }
            var combination = CombinationManager.Instance.getCombinationResult(newCell.type, cell.type);
            if (combination == null)
            {

                combination = CombinationManager.Instance.getCombinationResult(cell.type, newCell.type);

            }
            if (combination != null)
            {
                // update newCell


                hasCombined = true;
                var animCell = generateCell(cell.index, cell.type).GetComponent<GridCell>();
                yield return StartCoroutine(moveCardAnim(animCell, newCell.index));

                Destroy(animCell.gameObject);

                if (combination.result.ContainsKey("generate1"))
                {
                    var combinationGenerated = combination.result["generate1"];
                    if (combinationGenerated == newCell.type)
                    {
                        newCell.addAmount(cell.amount);
                    }
                    else
                    {

                        yield return StartCoroutine(destroy(newCell.gameObject));


                        generateCellAndOccupy(newIndex, combinationGenerated);
                        yield return new WaitForSeconds(animTime);
                    }
                }
            }
        }
        if (hasCombined)
        {
            yield return StartCoroutine(destroy(cell.gameObject));

        }
    }
    static int distance(Vector2 index)
    {
        index = Vector2Int.RoundToInt(index);
        return (int)(Mathf.Abs(index.x) + Mathf.Abs(index.y));
    }
    static int SortByDistanceToPlayer(GameObject g1, GameObject g2)
    {
        GridCell gc1 = g1.GetComponent<GridCell>();
        GridCell gc2 = g2.GetComponent<GridCell>();
        return distance(gc1.index).CompareTo(distance(gc2.index));
    }

    static int SortByDistanceToPlayer(GridCell gc1, GridCell gc2)
    {
        return distance(gc1.index).CompareTo(distance(gc2.index));
    }

    static int SortByDistanceToPlayer(Vector2 v1, Vector2 v2)
    {
        return distance(v1).CompareTo(distance(v2));
    }

    Vector2 getDir(Vector2 pos)
    {
        if (pos == Vector2.zero)
        {
            return Vector2.zero;
        }

        var dir = new Vector2(0, 1);
        //if (Mathf.Abs(pos.y) == 0)
        //{
        //        dir = new Vector2(Mathf.Sign(pos.x), 0);

        //}
        //else
        //{

        //    dir = new Vector2( 0, Mathf.Sign(pos.y));
        //}
        if (Mathf.Abs(pos.x) > Mathf.Abs(pos.y))
        {
            dir = new Vector2(Mathf.Sign(pos.x), 0);
        }
        else
        {

            dir = new Vector2(0, Mathf.Sign(pos.y));
        }
        return dir;
    }

    IEnumerator decreaseBothCell(GridCell cell, GridCell enemy, List<Vector2> mightUpdatedPositions)
    {

        var damage = Mathf.Min(cell.amount, enemy.amount);
        cell.decreaseAmount(damage);
        if (cell.amount == 0)
        {
            addFarerCellIntoPositions(mightUpdatedPositions, cell.index);
            yield return StartCoroutine(destroy(cell.gameObject));
        }

        enemy.decreaseAmount(damage);
        if (enemy.amount == 0)
        {
            addFarerCellIntoPositions(mightUpdatedPositions, enemy.index);
            yield return StartCoroutine(destroy(enemy.gameObject));
        }
        yield return null;
    }

    void playTrapAttackEffect(GridCell trap)
    {

        var go = Instantiate(Resources.Load<GameObject>("effect/trapEffect"), trap.index, Quaternion.identity);
    }

    void playWeaponAttackEffect(GridCell weapon, GridCell enemy)
    {
        var go = Instantiate(Resources.Load<GameObject>("effect/attack"), weapon.index, Quaternion.identity);
        go.transform.DOMove(enemy.index, GridController.Instance.animTime + 0.1f);
        Destroy(go, 1f);


        var go2 = Instantiate(Resources.Load<GameObject>("effect/attack"), weapon.index, Quaternion.identity);
        go2.transform.DOMove(enemy.index, GridController.Instance.animTime + 0.1f);
        go2.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("cell/" + weapon.type);
        go2.GetComponentInChildren<SpriteRenderer>().sortingOrder = 1;
        Destroy(go2, 1f);
    }

    IEnumerator moveEnemies()
    {

        yield return null;
        List<GridCell> enemies = new List<GridCell>();
        foreach (var cell in GameObject.FindObjectsOfType<GridCell>())
        {
            if (cell.cellInfo.isEnemy() && !cell.GetComponentInParent<TetrisShape>())
            {
                enemies.Add(cell);
            }
        }
        enemies.Sort(SortByDistanceToPlayer);
        int test2 = 30;
        while (enemies.Count > 0)
        {
            test2--;
            if (test2 < 0)
            {

                Debug.LogError("move too many times");
                break;
            }

            var enemy = enemies[0];
            enemies.RemoveAt(0);
            if (!enemy)
            {
                continue;
            }

            var pos = enemy.index;
            if (distance(enemy.index - playerPosition) == 1)
            {
                //enemy attack
                enemy.GetComponent<EnemyCell>().attack(playerCell,true);
                yield return StartCoroutine(destroy(enemy.gameObject));

                if (playerCell.amount <= 0)
                {
                    GameManager.Instance.gameover();
                }

                List<Vector2> mightUpdatedPositions = new List<Vector2>();
                addFarerCellIntoPositions(mightUpdatedPositions, pos);

                yield return StartCoroutine(movePositions(mightUpdatedPositions));
                continue;

            }

            var dir = getDir(pos - playerPosition);
            int test = 30;

            //check if next to weapon, if so, attack
            foreach(var scell in getSurroundingCells(enemy.index))
            {
                if (scell.cellInfo.isWeapon())
                {
                    List<Vector2> mightUpdatedPositions = new List<Vector2>();
                    StartCoroutine(decreaseBothCell(scell, enemy, mightUpdatedPositions));

                    playWeaponAttackEffect(scell, enemy);
                    yield return new WaitForSeconds(animTime);
                    yield return StartCoroutine(movePositions(mightUpdatedPositions));
                    if (enemy.amount <= 0)
                    {
                        break;
                    }
                }
            }

            if (!enemy || enemy.amount <= 0)
            {
                continue;
            }

            if (isOccupied(pos - dir))
            {
                var cell = getCellOnPosition(pos - dir);

                //check if swap with trap, if so, attack
                if (cell.cellInfo.isTrap())
                {
                    var trapIndex = cell.index;
                    List<Vector2> mightUpdatedPositions = new List<Vector2>();
                    yield return StartCoroutine(moveCardAnim(enemy, cell.index));
                    StartCoroutine(decreaseBothCell(cell, enemy, mightUpdatedPositions));
                    if (enemy && enemy.amount>0)
                    {
                        release(enemy.index);
                        occupy(trapIndex, enemy);
                    }
                    playTrapAttackEffect(cell);
                    yield return new WaitForSeconds(animTime);
                    yield return StartCoroutine(movePositions(mightUpdatedPositions));
                }
                else
                {
                    yield return StartCoroutine(exchangeCardAnim(enemy, cell));
                }
            }
            else
            {
                yield return StartCoroutine(moveCardAndOccupyAnim(getCellOnPosition(pos), pos - dir));

                List<Vector2> mightUpdatedPositions = new List<Vector2>();
                addFarerCellIntoPositions(mightUpdatedPositions, pos);

                yield return StartCoroutine(movePositions(mightUpdatedPositions));
            }

            enemies.Sort(SortByDistanceToPlayer);
        }
    }

    void addFarerCellIntoPositions(List<Vector2> positions, Vector2 pos)
    {
        var surroundingCells = getSurroundingCells(pos);
        foreach (var scell in surroundingCells)
        {
            if (distance(scell.index) > distance(pos))
            {
                if (!positions.Contains(scell.index))
                {
                    positions.Add(scell.index);
                }
            }
        }
    }

    IEnumerator movePositions(List<Vector2> positions)
    {
        yield return null;
        positions.Sort(SortByDistanceToPlayer);
        int test2 = 30;
        while (positions.Count > 0)
        {
            test2--;
            if (test2 < 0)
            {

                Debug.LogError("move too many times");
                break;
            }
            //get cell
            var pos = positions[0];
            positions.RemoveAt(0);
            var cell = getCellOnPosition(pos);
            if (cell == null)
            {
                //Debug.LogError("cell is null");
                continue;
            }

            //move cell to bottom
            var dir = getDir(pos);
            int test = 30;


            if (CheatManager.Instance.wouldMoveCells)
            {
                while (!isOccupied(pos - dir))
                {
                    test--;
                    if (test <= 0)
                    {
                        Debug.LogError("move too many times");
                        break;
                    }
                    yield return StartCoroutine(moveCardAndOccupyAnim(getCellOnPosition(pos), pos - dir));

                    addFarerCellIntoPositions(positions, pos);

                    pos = pos - dir;
                    dir = getDir(pos);
                }
            }


            yield return StartCoroutine(combineAround(cell.GetComponent<GridCell>()));

            if (CheatManager.Instance.wouldMoveCells)
            {
                if (!isOccupied(pos))
                {
                    //if the cell is destroyed after combien, find cells that might be affected and add to positions
                    addFarerCellIntoPositions(positions, pos);
                }
            }
            positions.Sort(SortByDistanceToPlayer);
        }
    }

    IEnumerator placeCellsAnim(List<GameObject> cells)
    {
        isMoving = true;
        //
        foreach (var cell in cells)
        {
            cell.GetComponent<GridCell>().collider.enabled = true; 
            occupy(cell.GetComponent<GridCell>().index, cell.GetComponent<GridCell>());
            foreach (var spriteRenderer in cell.GetComponentsInChildren<SpriteRenderer>())
            {
                spriteRenderer.sortingOrder -= 50;
            }
        }
        yield return null;



        List<Vector2> positions = new List<Vector2>();
        //find combinations from current to previous
        foreach (var cell in cells)
        {
            if (cell == null)
            {
                Debug.Log("cell is null");
            }
            positions.Add(cell.GetComponent<GridCell>().index);
        }



        yield return StartCoroutine(movePositions(positions));
        yield return StartCoroutine(moveEnemies());

        isMoving = false;
        resetSwap();

        TetrisGeneration.Instance.generateATetrisShape();

    }

    Vector2 gesCellPosition(Vector2 cellIndex)
    {
        return cellIndex * cellSize;
    }
    public GameObject generateCellAndOccupy(Vector2 index, string type, int amount = -1)
    {
        var res = generateCell(index, type, amount);
        occupy(index, res.GetComponent<GridCell>());
        return res;
    }
    public GameObject generateCell(Vector2 index, string type, int amount = -1)
    {
        Vector2 position = gesCellPosition(index);
        GameObject res;
        //if (type == "empty")
        //{

        //    res = Instantiate(emptyPrefab, position, Quaternion.identity, mainBoard);
        //}
        //else if (CellManager.Instance.isCell(type))
        {

            res = Instantiate(cellPrefab, position, Quaternion.identity, mainBoard);
        }
        //else
        //{

        //    res = Instantiate(itemPrefab, cellParents[index].position, Quaternion.identity, cellParents[index]);
        //}

        //  res.transform.localScale = Vector3.one;
        res.transform.DOPunchScale(Vector3.one, animTime);

        var typeSplit = type.Split('_');
        if (typeSplit.Length > 1)
        {
            type = typeSplit[0];
            amount = int.Parse(typeSplit[1]);
        }

        res.GetComponent<GridCell>().init(type, index, amount);
        return res;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
