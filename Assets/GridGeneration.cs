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
    public GameObject modifyPrefab;
    public GameObject targetCellPrefab;
    
    public float animTime = 0.3f;

    public int gridSize = 2;

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
        return (currentSwapTime < swapTime && !isMoving ) || CheatManager.Instance.unlimitedSwap;
    }
    public bool canMoveCell()
    {
        return !isMoving;
    }

    IEnumerator swapBehavior(GridCell cell1, GridCell cell2)
    {
        yield return null;
        foreach(var special in cell1.cellInfo.specialMode)
        {

            switch (special)
            {
                case "halfSwap":
                    cell2.decreaseAmount( Mathf.CeilToInt((float)cell2.amount/2f));

                    var go = Instantiate(Resources.Load<GameObject>("effect/spike"), cell1.index, Quaternion.identity);
                    go.transform.DOMove(cell2.index, GridController.Instance.animTime + 0.1f);
                    Destroy(go, 1f);

                    if (cell2.amount == 0)
                    {
                        yield return StartCoroutine(destroy(cell2.gameObject));
                    }
                    break;

            }
        }

    }

    public IEnumerator swap(GridCell cell1, GridCell cell2)
    {
        currentSwapTime++;
        EventPool.Trigger("updateSwap");
        isMoving = true;
        yield return StartCoroutine(exchangeCardAnim(cell1, cell2));


        List<Vector2> mightUpdatedPositions = new List<Vector2>();
        addFarerCellIntoPositions(mightUpdatedPositions, cell1.index);
        addFarerCellIntoPositions(mightUpdatedPositions, cell2.index);

        yield return StartCoroutine(movePositions(mightUpdatedPositions));

        if (cell1.cellInfo.specialMode!=null)
        {

            yield return StartCoroutine(swapBehavior(cell1, cell2));
        }

        if (cell2.cellInfo.specialMode != null)
        {
            yield return StartCoroutine(swapBehavior(cell2, cell1));
        }


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
    public IEnumerator destroy(GameObject go)
    {
        go.transform.DOScale(Vector3.zero, animTime);
        go.transform.DOLocalMoveY(1, animTime);
        release(go.GetComponent<GridCell>().index);
        yield return new WaitForSeconds(animTime);

        if(go.GetComponent<GridCell>() && go.GetComponent<GridCell>().birdItem!=null && go.GetComponent<GridCell>().birdItem.Length>0)
        {
            //drop
            generateCell(go.GetComponent<GridCell>().index, go.GetComponent<GridCell>().birdItem);
        }

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
        if (cellIndex == cellIndex2)
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


    public IEnumerator calculateCombinedResult(List<GridCell> cells, List<GameObject> newCells, bool supportInPosiiton = true)
    {
        bool showResult = newCells != null;
        Dictionary<Vector2, bool> hasCombined = new Dictionary<Vector2, bool>();
        Dictionary<Vector2, GameObject> positionToCell = new Dictionary<Vector2, GameObject>();

        Dictionary<Vector2, List<Vector2>> visited = new Dictionary<Vector2, List<Vector2>>();

        List<Vector2> currentCellsPositions = new List<Vector2>();


        HashSet<GameObject> willDestory = new HashSet<GameObject>();
        HashSet<GameObject> willOccupy = new HashSet<GameObject>();

        foreach (var cell in cells)
        {
            currentCellsPositions.Add(cell.index);
        }

        foreach (var cell in cells)
        {
            var cellIndex = Vector2Int.RoundToInt(cell.index);
            hasCombined[cellIndex] = false;
            foreach (var dir in dirs)
            {
                var newIndex = Vector2Int.RoundToInt(cell.index + dir);
                if (!supportInPosiiton && currentCellsPositions.Contains(newIndex))
                {
                    continue;
                }
                var newCell = getCellOnPosition(newIndex);
                if (newCell == null)
                {
                    foreach (var currentCell in cells)
                    {
                        if (currentCell.index == newIndex)
                        {
                            newCell = currentCell;//remove duplication
                        }
                    }

                }
                if (!newCell)
                {
                    continue;
                }

                if (visited.ContainsKey(newIndex) && visited[newIndex].Contains(cell.index))
                {
                    continue;
                }

                if (!visited.ContainsKey(cell.index))
                {
                    visited[cell.index] = new List<Vector2>();
                }
                visited[cell.index].Add(newIndex);

                var combination = CombinationManager.Instance.getCombinationResult(newCell.type, cell.type);

                if (combination != null)
                {


                    if (!showResult)
                    {

                        var animCell = generateCell(cell.index, cell.type).GetComponent<GridCell>();
                        animCell.GetComponent<GridCell>().collider.enabled = false;
                        yield return StartCoroutine(moveCardAnim(animCell, newCell.index));
                        Destroy(animCell.gameObject);
                    }

                    // update newCell
                    hasCombined[cell.index] = true;
                   // willDestory.Add(combineCell.gameObject);

                    if (combination.result.ContainsKey("generate1"))
                    {

                        if (positionToCell.ContainsKey(newCell.index))
                        {
                            continue;
                        }
                        var combinationGenerated = combination.result["generate1"];

                        if (showResult)
                        {
                            var animCell = generateCell(newIndex, combinationGenerated).GetComponent<GridCell>();
                            newCells.Add(animCell.gameObject);
                            animCell.GetComponent<GridCell>().collider.enabled = false;
                            animCell.GetComponent<GridCell>().HPBK.SetActive(false);
                            positionToCell[newCell.index] = animCell.gameObject;
                        }
                        else
                        {


                            NewCellManager.Instance.ShowNewCell(combinationGenerated);

                            if (cell == null || cell.gameObject == null)
                            {
                                Debug.LogError("???");
                            }

                            //yield return StartCoroutine(destroy(newCell.gameObject));
                            //newCell.

                            //only generate, occupy later together
                            var gCell = generateCell(newIndex, combinationGenerated);
                            willOccupy.Add(gCell);
                            yield return new WaitForSeconds(animTime);
                            //hasCombined[combineNewCell.index] = true;

                            willDestory.Add(newCell.gameObject);
                        }
                    }
                    else if (combination.result.ContainsKey("addHP"))
                    {
                        string value = combination.result["addHP"];
                        var theOneAddValue = value == "1" ? newCell : cell;
                        var theOneIsValue = value != "1" ? newCell : cell;
                        if (showResult)
                        {
                            if (positionToCell.ContainsKey(newIndex))
                            {

                                positionToCell[theOneAddValue.index].GetComponent<GridCell>().addAmount(theOneIsValue.amount);
                            }
                            else
                            {
                                var animCell = generateCell(newIndex, "addHP", theOneIsValue.amount).GetComponent<GridCell>();
                                newCells.Add(animCell.gameObject);
                                animCell.GetComponent<GridCell>().collider.enabled = false;

                                positionToCell[newIndex] = animCell.gameObject;
                            }
                        }
                        else
                        {
                            theOneAddValue.addAmount(theOneIsValue.amount);
                            willDestory.Add(theOneIsValue.gameObject);
                            //yield return StartCoroutine(destroy(cell.gameObject));
                        }
                    }
                }
            }
            if (hasCombined[cellIndex])
            {
                willDestory.Add(cell.gameObject);

            }
        }

        foreach (var cell in cells)
        {
            if (hasCombined.ContainsKey(cell.index) && hasCombined[cell.index] && !positionToCell.ContainsKey(cell.index))
            {
                if (showResult)
                {
                    var animCell = generateCell(cell.index, "destroy").GetComponent<GridCell>();
                    newCells.Add(animCell.gameObject);
                }


            }
        }
        if (!showResult)
        {
            foreach (var pair in willDestory)
            {
                //if(pair.Value == true)
                {

                    var cell = pair;
                    if (cell == null || cell.gameObject == null)
                    {
                        Debug.LogError("???");
                    }
                    yield return StartCoroutine(destroy(cell.gameObject));
                }

            }

            foreach (var pair in willOccupy)
            {

                occupy(pair.GetComponent<GridCell>().index, pair.GetComponent<GridCell>());
            }
                }

        if (newCells != null)
        {
            foreach(var c in newCells)
            {

                foreach (var render in c.GetComponentsInChildren<SpriteRenderer>())
                {
                    render.sortingOrder += 100;
                }
            }
        }
        yield return null;
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
                animCell.GetComponent<GridCell>().collider.enabled = false;
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
    List<GridCell> allAttracts = null;


    int enemyDistance(GridCell gc)
    {
        if(allAttracts == null)
        {
            allAttracts = new List<GridCell>();
            foreach(var cell in GameObject.FindObjectsOfType<GridCell>())
            {
                if(cell.cellInfo.isTrap() && cell.cellInfo.attackMode == "attract")
                {
                    allAttracts.Add(cell);
                }
            }
        }
        int shortest = distance(gc.index - playerPosition);
        foreach (var trap in allAttracts)
        {
            shortest = Mathf.Min(shortest, distance(gc.index - trap.index));
        }
        return shortest;
    }

    Vector2 enemyDir(GridCell gc)
    {
        
        int shortest = distance(playerPosition - gc.index);
        var dir = playerPosition - gc.index;
        if(allAttracts == null)
        {

            allAttracts = new List<GridCell>();
            foreach (var cell in GameObject.FindObjectsOfType<GridCell>())
            {
                if (cell.cellInfo.isTrap() && cell.cellInfo.attackMode == "attract")
                {
                    allAttracts.Add(cell);
                }
            }
        }
        foreach (var trap in allAttracts)
        {
            var newdis = distance( trap.index - gc.index);
            if (shortest >= newdis)
            {
                shortest = newdis;
                dir = trap.index - gc.index;
            }
        }
        return dir;
    }

    public Vector2 enemyTargets(GridCell gc,out GridCell target)
    {

        int shortest = distance(playerPosition - gc.index);
        var dir = playerPosition - gc.index;
        target = playerCell;
        if (allAttracts == null)
        {

            allAttracts = new List<GridCell>();
            foreach (var cell in GameObject.FindObjectsOfType<GridCell>())
            {
                if (cell.cellInfo.isTrap() && cell.cellInfo.attackMode == "attract")
                {
                    allAttracts.Add(cell);
                }
            }
        }
        foreach (var trap in allAttracts)
        {
            var newdis = distance(trap.index - gc.index);
            if (shortest >= newdis)
            {
                shortest = newdis;
                dir = trap.index - gc.index;
                target = trap;
            }
        }
        return dir;
    }

    int SortByDistanceToTarget(GridCell gc1, GridCell gc2)
    {
        return enemyDistance(gc1).CompareTo(enemyDistance(gc2));
    }

    static int SortByDistanceToPlayer(Vector2 v1, Vector2 v2)
    {
        return distance(v1).CompareTo(distance(v2));
    }

    public Vector2 getDir(Vector2 pos)
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
            if (cell.cellInfo != null && cell.cellInfo.isEnemy() && !cell.GetComponentInParent<TetrisShape>() && cell.GetComponent<EnemyCell>())
            {
                enemies.Add(cell);
            }
        }
        enemies.Sort(SortByDistanceToTarget);
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



            var dir = getDir(enemyDir(enemy));
            int test = 30;

            //check if next to weapon, if so, attack
            foreach (var scell in getSurroundingCells(enemy.index))
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


            if (enemy.GetComponent<EnemyCell>().isFirst)
            {
                //dont move for the first step
                enemy.GetComponent<EnemyCell>().isFirst = false;
                continue;
            }

                if (isOccupied(pos + dir))
            {
                var cell = getCellOnPosition(pos + dir);

                //check if swap with trap, if so, attack
                if (cell.cellInfo.isTrap())
                {
                    var trapIndex = cell.index;
                    List<Vector2> mightUpdatedPositions = new List<Vector2>();
                    yield return StartCoroutine(moveCardAnim(enemy, cell.index));
                    StartCoroutine(decreaseBothCell(cell, enemy, mightUpdatedPositions));
                    if (enemy && enemy.amount > 0)
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
                    if (distance(enemy.index - playerPosition) == 1)
                    {
                        //enemy attack
                        enemy.GetComponent<EnemyCell>().attack(playerCell, true);
                        yield return StartCoroutine(destroy(enemy.gameObject));

                        if (playerCell.amount <= 0)
                        {
                            GameManager.Instance.gameover();
                        }

                        List<Vector2> mightUpdatedPositions = new List<Vector2>();
                        addFarerCellIntoPositions(mightUpdatedPositions, pos);

                        yield return StartCoroutine(movePositions(mightUpdatedPositions));

                    }

                    if (!enemy || enemy.amount <= 0)
                    {
                        continue;
                    }
                    foreach (var scell in getSurroundingCells(enemy.index))
                    {
                        if (scell.cellInfo.isAlly())
                        {

                            enemy.GetComponent<EnemyCell>().attack(scell, true);
                            yield return StartCoroutine(destroy(enemy.gameObject));
                            if (scell.amount <= 0)
                            {
                                yield return StartCoroutine(destroy(scell.gameObject));
                            }

                            List<Vector2> mightUpdatedPositions = new List<Vector2>();
                            addFarerCellIntoPositions(mightUpdatedPositions, pos);
                            addFarerCellIntoPositions(mightUpdatedPositions, scell.index);

                            yield return StartCoroutine(movePositions(mightUpdatedPositions));
                        }
                    }

                    if (!enemy || enemy.amount <= 0)
                    {
                        continue;
                    }
                    yield return StartCoroutine(exchangeCardAnim(enemy, cell));
                }
            }
            else
            {
                yield return StartCoroutine(moveCardAndOccupyAnim(getCellOnPosition(pos), pos + dir));

                List<Vector2> mightUpdatedPositions = new List<Vector2>();
                addFarerCellIntoPositions(mightUpdatedPositions, pos);

                yield return StartCoroutine(movePositions(mightUpdatedPositions));
            }


            if (!enemy || enemy.amount <= 0)
            {
                continue;
            }



            enemies.Sort(SortByDistanceToTarget);
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


            //yield return StartCoroutine(combineAround(cell.GetComponent<GridCell>()));

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



            if (cell.GetComponent<GridCell>().cellInfo.specialMode!=null && cell.GetComponent<GridCell>().cellInfo.specialMode.Contains("FlyAtPlace"))
            {
                //fly to a random position
                var currentDistanceToCenter = distance(cell.GetComponent<GridCell>().index);
                List<Vector2> sameDistanceList = new List<Vector2>();
                List<Vector2> notSameDistanceList = new List<Vector2>();
                //find position that can swap
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        var vec = new Vector2(i, j);
                        var newDistanc = distance(vec);
                        if (!isOccupied(vec))
                        {

                            if (newDistanc == currentDistanceToCenter && vec != cell.GetComponent<GridCell>().index)
                            {
                                sameDistanceList.Add(vec);
                            }
                            else
                            {
                                notSameDistanceList.Add(vec);
                            }
                        }
                    }
                }
                Vector2 finalIndex = cell.GetComponent<GridCell>().index;
                if (sameDistanceList.Count > 0)
                {
                    finalIndex = Utils.randomList(sameDistanceList);
                }
                else if (notSameDistanceList.Count > 0)
                {

                    finalIndex = Utils.randomList(notSameDistanceList);
                }


                var go = Instantiate(Resources.Load<GameObject>("effect/fly"), cell.GetComponent<GridCell>().index, Quaternion.identity);
                var dir = finalIndex - cell.GetComponent<GridCell>().index;
                go.transform.DOMove(dir.normalized+ cell.GetComponent<GridCell>().index, GridController.Instance.animTime + 0.1f);
                Destroy(go, 1f);

                yield return moveCardAndOccupyAnim(cell.GetComponent<GridCell>(), finalIndex);
                
            }

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

        List<GridCell> gridCells = new List<GridCell>();
        foreach (var cell in cells)
        {
            gridCells.Add(cell.GetComponent<GridCell>());
        }

        yield return StartCoroutine(GridGeneration.Instance.calculateCombinedResult(gridCells, null));
        //yield return StartCoroutine(movePositions(positions));
        yield return StartCoroutine(moveEnemies());

        isMoving = false;
        resetSwap();
        allAttracts = null;

        EventPool.Trigger("moveAStep");

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

    public GameObject generateTargetCell(Vector2 index)
    {
        Vector2 position = gesCellPosition(index);

        GameObject res;
        res = Instantiate(targetCellPrefab, position, Quaternion.identity, mainBoard);
        return res;
    }

    public GameObject generateCell(Vector2 index, string type, int amount = -1)
    {
        Vector2 position = gesCellPosition(index);
        GameObject res;
        if (type == "addHP")
        {

            res = Instantiate(modifyPrefab, position, Quaternion.identity, mainBoard);
        }
        else if (type == "destroy")
        {
            res = Instantiate(modifyPrefab, position, Quaternion.identity, mainBoard);
        }
        else
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
