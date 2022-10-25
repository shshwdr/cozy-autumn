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

    public int gridSizex = 2;
    public int gridSizey = 2;

    Dictionary<Vector2, GridCell> indexToCell = new Dictionary<Vector2, GridCell>();
    Dictionary<Vector2, GameObject> indexToTestCell = new Dictionary<Vector2, GameObject>();

    public int swapTime = 1;
    public int currentSwapTime = 0;
    bool isMoving = false;

    public GameObject mainCanvas; public GameObject MainMenuCanvas;

    Vector2[] dirs = new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };
    Vector2[] dirs8 = new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) ,
        new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(-1, -1) ,
    };

    int round = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (StageManager.Instance.currentStage != null && StageManager.Instance.currentStage.Length > 0)
        {
            mainCanvas.SetActive(true);
            MainMenuCanvas.SetActive(false);
            init();

            //show tutorial drag
            NewCellManager.Instance.ShowNewCell("tutorialDrag");
        }
        else
        {

            mainCanvas.SetActive(false);
            MainMenuCanvas.SetActive(true);
        }
        StageManager.Instance.reopt();
        CharacterManager.Instance.reopt();
    }

    public void init()
    {
        playerCell = generateCell(playerPosition, "player").GetComponent<GridCell>();
        playerCell.renderer.sprite = Resources.Load<Sprite>("cell/" + CharacterManager.Instance.currentChar);

        TetrisGeneration.Instance.generateATetrisShape();

        StartCoroutine(placeCellsAnim(new List<GameObject>() { playerCell.gameObject }));
    }
    public bool canSwap()
    {
        return (currentSwapTime < swapTime && !isMoving) || CheatManager.Instance.unlimitedSwap;
    }
    public bool canMoveCell()
    {
        return !isMoving;
    }

    void minuseHpByHalf(GridCell cell)
    {

        cell.decreaseAmount(Mathf.CeilToInt((float)cell.amount / 2f));
    }

    IEnumerator swapBehavior(GridCell cell1, GridCell cell2)
    {
        yield return null;
        foreach (var special in cell1.cellInfo.specialMode)
        {

            switch (special)
            {
                case "halfSwap":
                    minuseHpByHalf(cell2);

                    var go = Instantiate(Resources.Load<GameObject>("effect/spike"), getCellPosition(cell1.index), Quaternion.identity);
                    go.transform.DOMove(getCellPosition(cell2.index), GridController.Instance.animTime + 0.1f);
                    Destroy(go, 1f);

                    if (cell2.amount <= 0)
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
        LogManager.Instance.log("swap set is moving to true");
        isMoving = true;
        yield return StartCoroutine(exchangeCardAnim(cell1, cell2));


        List<Vector2> mightUpdatedPositions = new List<Vector2>();
        // addFarerCellIntoPositions(mightUpdatedPositions, cell1.index);
        // addFarerCellIntoPositions(mightUpdatedPositions, cell2.index);

        // yield return StartCoroutine(movePositions(mightUpdatedPositions));

        LogManager.Instance.log("swap finish exchange");
        if (cell1 && cell2 && cell1.amount > 0 && cell2.amount > 0 && cell1.cellInfo.specialMode != null)
        {

            yield return StartCoroutine(swapBehavior(cell1, cell2));
        }

        if (cell1 && cell2 && cell1.amount > 0 && cell2.amount > 0 && cell2.cellInfo.specialMode != null)
        {
            yield return StartCoroutine(swapBehavior(cell2, cell1));
        }


        isMoving = false;

        LogManager.Instance.log("swap set is moving to false");

        AchievementManager.Instance.ShowAchievement("move");
        AchievementManager.Instance.clear("notMove");

    }
    void resetSwap()
    {
        if (currentSwapTime == 0)
        {

            AchievementManager.Instance.clear("move");
            AchievementManager.Instance.ShowAchievement("notMove");
        }
        currentSwapTime = 0;
        EventPool.Trigger("updateSwap");

    }
    public bool isOccupied(Vector2 index)
    {
        Vector2Int ind = Vector2Int.RoundToInt(index);
        return indexToCell.ContainsKey(ind) && indexToCell[ind] != null;
    }

    bool isPositionValid(Vector2 index)
    {

        Vector2Int ind = Vector2Int.RoundToInt(index);
        return ind.x <= gridSizex && ind.x >= -gridSizex && ind.y <= gridSizey && ind.y >= -gridSizey;
    }

    bool test = true;
    public void occupy(Vector2 index, GridCell cell)
    {
        
        Vector2Int ind = Vector2Int.RoundToInt(index);

        indexToCell[ind] = cell;
        cell.index = ind;
        if (cell.mask)
        {
            cell.mask.SetActive(true);
        }
        LogManager.Instance.logOccupy(index, cell);

        if (cell.cellInfo.isEnemy())
        {
            NewCellManager.Instance.ShowNewCell("tutorialEnemy");
        }
        if (test)
        {

            var testCell = Instantiate(Resources.Load<GameObject>("testCell"), getCellPosition(cell.index), Quaternion.identity);
            indexToTestCell[ind] = testCell;
        }


    }

    public void release(Vector2 index)
    {

        Vector2Int ind = Vector2Int.RoundToInt(index);
        if (!indexToCell.ContainsKey(ind))
        {
            Debug.LogWarning("release not existed index " + ind);
        }
        else
        {
            LogManager.Instance.logRelease(index, indexToCell[ind]);
            indexToCell.Remove(ind);
        }
        if (test)
        {

            Destroy(indexToTestCell[ind]);
            indexToTestCell[ind] = null;
        }
    }

    public bool isNextToOccupiedCells(Vector2 index)
    {
        if (getSurroundingCells(index).Count > 0)
        {
            return true;
        }
        return false;
    }

    public List<GridCell> getSurrounding3x3Cells(Vector2 index)
    {
        List<GridCell> cells = new List<GridCell>();
        foreach (var dir in dirs8)
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
    public IEnumerator destroy(GameObject go, bool showAnim = true)
    {
        testTime = 0;
        release(go.GetComponent<GridCell>().index);
        go.GetComponent<GridCell>().decreaseAmount(go.GetComponent<GridCell>().amount);
        if (showAnim)
        {

            go.transform.DOScale(Vector3.zero, animTime);
            go.transform.DOLocalMoveY(1, animTime);
            yield return new WaitForSeconds(animTime);
            testTime = 0;
        }
        else
        {

            yield return null;
        }

        LogManager.Instance.log("middle of destroy");
        if (go.GetComponent<GridCell>() && go.GetComponent<GridCell>().birdItem != null && go.GetComponent<GridCell>().birdItem.Length > 0)
        {
            //drop
            var cell = generateCell(go.GetComponent<GridCell>().index, go.GetComponent<GridCell>().birdItem);
            occupy(cell.GetComponent<GridCell>().index, cell.GetComponent<GridCell>());
            //yield return StartCoroutine(combineAround(cell.GetComponent<GridCell>()));

            yield return StartCoroutine(calculateCombinedResult(new List<GridCell>() { cell.GetComponent<GridCell>() }, null));
        }

        if (go.GetComponent<GridCell>())
        {
            if (go.GetComponent<GridCell>().cellInfo.isAlly())
            {

                AchievementManager.Instance.ShowAchievement("killFriend");
                SFXManager.Instance.play("allydie");
            }

        }

        Destroy(go);
    }
    IEnumerator moveCardAnim(GridCell cell, Vector2 targetPosition)
    {
        testTime = 0;
        if (!cell)
        {
            Debug.Log("???"); 
            yield return null;
            yield break;
        }
        SFXManager.Instance.play("cardmove");

        cell.transform.DOMove(getCellPosition(targetPosition), animTime);

        yield return new WaitForSeconds(animTime);
        testTime = 0;

        LogManager.Instance.logMove(targetPosition, cell);
        yield return null;
    }

    IEnumerator moveCardAndOccupyAnim(GridCell cell, Vector2 targetPosition)
    {
        testTime = 0;
        if (!cell)
        {
            Debug.LogError("no cell");
        }
        release(cell.index);

        occupy(targetPosition, cell);
        yield return StartCoroutine(moveCardAnim(cell, targetPosition));

        testTime = 0;
    }
    IEnumerator exchangeCardAnim(GridCell cell, GridCell cell2)
    {
        testTime = 0;
        LogManager.Instance.logExchange(cell, cell2);
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


        if(!cell || !cell2)
        {
            yield break;
        }
        yield return StartCoroutine(calculateCombinedResult(new List<GridCell>() { cell.GetComponent<GridCell>(), cell2.GetComponent<GridCell>() }, null));



        if (!cell || !cell2)
        {
            yield break;
        }
        //yield return StartCoroutine(combineAround(cell.GetComponent<GridCell>()));
        // yield return StartCoroutine(combineAround(cell2.GetComponent<GridCell>()));

        LogManager.Instance.log("exchangeCardAnim start destory");
        if (cell.cellInfo.isEmpty())
        {
            yield return StartCoroutine(destroy(cell.gameObject));
        }

        LogManager.Instance.log("exchangeCardAnim finish destory");

        if (!cell || !cell2)
        {
            yield break;
        }
        if (cell2.cellInfo.isEmpty())
        {
            yield return StartCoroutine(destroy(cell2.gameObject));
        }
        testTime = 0;
    }


    public IEnumerator calculateCombinedResult(List<GridCell> cells, List<GameObject> newCells, bool supportInPosiiton = true)
    {
        testTime = 0;
        bool showResult = newCells != null;
        Dictionary<Vector2, bool> hasCombined = new Dictionary<Vector2, bool>();
        Dictionary<Vector2, GameObject> positionToCell = new Dictionary<Vector2, GameObject>();

        Dictionary<Vector2, List<Vector2>> visited = new Dictionary<Vector2, List<Vector2>>();

        List<Vector2> currentCellsPositions = new List<Vector2>();


        HashSet<GameObject> willDestory = new HashSet<GameObject>();
        HashSet<GameObject> willOccupy = new HashSet<GameObject>();
        HashSet<Vector2> willOccupyPosition = new HashSet<Vector2>();
        HashSet<Vector2> willDestroyPosition = new HashSet<Vector2>();


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


                    // update newCell
                    // willDestory.Add(combineCell.gameObject);

                    if (combination.result.ContainsKey("generate1"))
                    {


                        if (!showResult)
                        {

                            var animCell = generateCell(cell.index, cell.type).GetComponent<GridCell>();
                            animCell.GetComponent<GridCell>().collider.enabled = false;
                            yield return StartCoroutine(moveCardAnim(animCell, newCell.index));
                            Destroy(animCell.gameObject);
                        }
                        if (positionToCell.ContainsKey(newCell.index) || willOccupyPosition.Contains(newCell.index))
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
                            SFXManager.Instance.play("craft");
                            var gCell = generateCell(newIndex, combinationGenerated);
                            willOccupy.Add(gCell);
                            yield return new WaitForSeconds(animTime);
                            testTime = 0;
                            //hasCombined[combineNewCell.index] = true;

                            willDestory.Add(newCell.gameObject);
                            willDestory.Add(cell.gameObject);


                            AchievementManager.Instance.ShowAchievement("craft");

                        }
                        willOccupyPosition.Add(newIndex);
                        willDestroyPosition.Add(newCell.index);
                        willDestroyPosition.Add(cell.index);
                        hasCombined[cell.index] = true;
                    }
                    else if (combination.result.ContainsKey("addHP"))
                    {
                        string value = combination.result["addHP"];
                        var theOneAddValue = value == "1" ? newCell : cell;
                        var theOneIsValue = value != "1" ? newCell : cell;
                        if (showResult)
                        {
                            if (positionToCell.ContainsKey(theOneAddValue.index))
                            {

                                positionToCell[theOneAddValue.index].GetComponent<GridCell>().addAmount(theOneIsValue.amount);
                            }
                            else
                            {
                                var animCell = generateCell(theOneAddValue.index, "addHP", theOneIsValue.amount).GetComponent<GridCell>();
                                newCells.Add(animCell.gameObject);
                                animCell.GetComponent<GridCell>().collider.enabled = false;

                                positionToCell[theOneAddValue.index] = animCell.gameObject;
                            }
                        }
                        else
                        {



                           // if (!showResult)
                            {
                                var animCell = generateCell(theOneIsValue.index, theOneIsValue.type).GetComponent<GridCell>();
                                animCell.GetComponent<GridCell>().collider.enabled = false;
                                yield return StartCoroutine(moveCardAnim(animCell, theOneAddValue.index));
                                Destroy(animCell.gameObject);
                            }

                            theOneAddValue.addAmount(theOneIsValue.amount);
                            willDestory.Add(theOneIsValue.gameObject);

                            //yield return StartCoroutine(destroy(cell.gameObject));
                        }
                        willDestroyPosition.Add(theOneIsValue.index);
                        hasCombined[theOneIsValue.index] = true;
                    }
                }
            }
            //if (hasCombined[cellIndex])
            //{
            //    willDestory.Add(cell.gameObject);

            //}
        }

        //foreach (var cell in cells)
        //{
        //    if (hasCombined.ContainsKey(cell.index) && hasCombined[cell.index] && !positionToCell.ContainsKey(cell.index))
        //    {
        //        var animCell = generateCell(cell.index, "leaf").GetComponent<GridCell>();
        //        if (showResult)
        //        {
        //            newCells.Add(animCell.gameObject);
        //        }
        //        else
        //        {
        //            willOccupy.Add(animCell.gameObject);
        //        }
        //    }
        //}
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
                    if (willOccupyPosition.Contains(cell.GetComponent<GridCell>().index))
                    {

                        StartCoroutine(destroy(cell.gameObject, false));
                    }
                    else
                    {

                        StartCoroutine(destroy(cell.gameObject));
                    }
                }

            }

            foreach (var pair in willDestroyPosition)
            {
                if (!willOccupyPosition.Contains(pair))
                {
                    var animCell = generateCell(pair, StageManager.Instance.getCurrentInfo().leafName).GetComponent<GridCell>();

                    NewCellManager.Instance.ShowNewCell(StageManager.Instance.getCurrentInfo().leafName);
                    willOccupy.Add(animCell.gameObject);
                }
            }
            foreach (var pair in willOccupy)
            {

                occupy(pair.GetComponent<GridCell>().index, pair.GetComponent<GridCell>());
            }

        }
        else
        {

            foreach (var pair in willDestroyPosition)
            {
                if (!willOccupyPosition.Contains(pair))
                {
                    var animCell = generateCell(pair, StageManager.Instance.getCurrentInfo().leafName).GetComponent<GridCell>();
                    NewCellManager.Instance.ShowNewCell(StageManager.Instance.getCurrentInfo().leafName);
                    newCells.Add(animCell.gameObject);
                }
            }
        }

        if (newCells != null)
        {
            foreach (var c in newCells)
            {

                foreach (var render in c.GetComponentsInChildren<SpriteRenderer>())
                {
                    render.sortingOrder += 100;
                }
            }
        }
        yield return null;
    }



    //IEnumerator combineAround(GridCell cell)
    //{
    //    yield return null;
    //    bool hasCombined = false;

    //    HashSet<GameObject> willDestory = new HashSet<GameObject>();
    //    foreach (var dir in dirs)
    //    {
    //        var newIndex = cell.index + dir;
    //        var newCell = getCellOnPosition(newIndex);
    //        if (newCell == null)
    //        {
    //            continue;
    //        }
    //        var combination = CombinationManager.Instance.getCombinationResult(newCell.type, cell.type);
    //        if (combination != null)
    //        {


    //            //if (!showResult)
    //            {

    //                var animCell = generateCell(cell.index, cell.type).GetComponent<GridCell>();
    //                animCell.GetComponent<GridCell>().collider.enabled = false;
    //                yield return StartCoroutine(moveCardAnim(animCell, newCell.index));
    //                Destroy(animCell.gameObject);
    //            }

    //            // update newCell
    //            // willDestory.Add(combineCell.gameObject);

    //            if (combination.result.ContainsKey("generate1"))
    //            {

    //                hasCombined = true;
    //                //if (positionToCell.ContainsKey(newCell.index))
    //                //{
    //                //    continue;
    //                //}
    //                var combinationGenerated = combination.result["generate1"];
    //                {


    //                    NewCellManager.Instance.ShowNewCell(combinationGenerated);

    //                    if (cell == null || cell.gameObject == null)
    //                    {
    //                        Debug.LogError("???");
    //                    }

    //                    //yield return StartCoroutine(destroy(newCell.gameObject));
    //                    //newCell.

    //                    //only generate, occupy later together
    //                    var gCell = generateCell(newIndex, combinationGenerated);
    //                    //willOccupy.Add(gCell);
    //                    yield return new WaitForSeconds(animTime);
    //                    //hasCombined[combineNewCell.index] = true;

    //                    willDestory.Add(newCell.gameObject);
    //                    willDestory.Add(cell.gameObject);
    //                }
    //            }
    //            else if (combination.result.ContainsKey("addHP"))
    //            {
    //                string value = combination.result["addHP"];
    //                var theOneAddValue = value == "1" ? newCell : cell;
    //                var theOneIsValue = value != "1" ? newCell : cell;
    //                {
    //                    theOneAddValue.addAmount(theOneIsValue.amount);
    //                    willDestory.Add(theOneIsValue.gameObject);


    //                    //yield return StartCoroutine(destroy(cell.gameObject));
    //                }
    //            }
    //        }
    //    }
    //    //if (hasCombined)
    //    //{
    //    //    willDestory.Add(cell.gameObject);

    //    //}

    //    foreach (var pair in willDestory)
    //    {
    //        //if(pair.Value == true)
    //        {

    //            if (pair == null || pair.gameObject == null)
    //            {
    //                Debug.LogError("???");
    //            }
    //            yield return StartCoroutine(destroy(pair.gameObject));
    //        }

    //    }
    //}
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
            }
        }
        return dir;
    }

    public Vector2 enemyTargets(GridCell gc, out GridCell target)
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
        if (target.cellInfo.isTrap())
        {
            AchievementManager.Instance.ShowAchievement("lure");
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

    IEnumerator decreaseBothCell(GridCell cell, GridCell enemy, List<Vector2> mightUpdatedPositions, List<GridCell> otherCells = null)
    {

        testTime = 0;
        var damage = Mathf.Min(cell.amount, enemy.amount);
        cell.decreaseAmount(damage);
        if (cell.amount <= 0)
        {
          //  addFarerCellIntoPositions(mightUpdatedPositions, cell.index);
            yield return StartCoroutine(destroy(cell.gameObject));
        }

        if (cell.cellInfo.attackMode == "killOrHeal" && damage < enemy.amount)
        {
            //heal
            enemy.addAmount(damage);
        }
        else
        {
            enemy.decreaseAmount(damage);
            if (enemy.amount <= 0)
            {
               // addFarerCellIntoPositions(mightUpdatedPositions, enemy.index);
                yield return StartCoroutine(destroy(enemy.gameObject));
            }
        }



        if (otherCells != null)
        {
            foreach (var c in otherCells)
            {
                c.decreaseAmount(damage);
                if (c.amount <= 0)
                {
                  //  addFarerCellIntoPositions(mightUpdatedPositions, c.index);
                    yield return StartCoroutine(destroy(c.gameObject));
                }
            }
        }

        yield return null;

        testTime = 0;
    }

    void playHealEffect(GridCell healer, GridCell target)
    {
        var go = Instantiate(Resources.Load<GameObject>("effect/healEffect"), getCellPosition(healer.index), Quaternion.identity);
        go.transform.DOMove(getCellPosition(target.index), GridController.Instance.animTime);
        Destroy(go, 1f);
        SFXManager.Instance.play("heal");

    }

    
        void playExplodeAttackEffect(GridCell trap)
    {

        var go = Instantiate(Resources.Load<GameObject>("effect/explode"), getCellPosition(trap.index), Quaternion.identity);
        AchievementManager.Instance.ShowAchievement("trap");
        SFXManager.Instance.play("shortburn");
    }

        void playTrapAttackEffect(GridCell trap)
    {

        SFXManager.Instance.play("birdTrap");
        var go = Instantiate(Resources.Load<GameObject>("effect/trapEffect"), getCellPosition(trap.index), Quaternion.identity);

        AchievementManager.Instance.ShowAchievement("trap");
        if(trap.cellInfo!=null && trap.cellInfo.attackMode == "attract")
        {

            AchievementManager.Instance.ShowAchievement("lureDeath");
        }
    }
    void playStompAttackEffect(GridCell trap)
    {

        var go = Instantiate(Resources.Load<GameObject>("effect/stomp"), getCellPosition(trap.index), Quaternion.identity);
    }

    void playWeaponAttackEffect(GridCell weapon, GridCell enemy)
    {
        var go = Instantiate(Resources.Load<GameObject>("effect/attack"), getCellPosition(weapon.index), Quaternion.identity);
        go.transform.DOMove(getCellPosition(enemy.index), GridController.Instance.animTime);
        Destroy(go, 1f);


        var go2 = Instantiate(Resources.Load<GameObject>("effect/attack"), getCellPosition(weapon.index), Quaternion.identity);
        go2.transform.DOMove(getCellPosition(enemy.index), GridController.Instance.animTime);
        go2.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("cell/" + weapon.type);
        go2.GetComponentInChildren<SpriteRenderer>().sortingOrder = 1;
        Destroy(go2, 1f);

        if(weapon.cellInfo.type == "whipe"|| weapon.cellInfo.type == "lasso")
        {

            SFXManager.Instance.play("whip");
        }
        else
        {

            SFXManager.Instance.play("attack");
        }

        AchievementManager.Instance.ShowAchievement("weapon");
    }

    IEnumerator moveWeapons()
    {

        testTime = 0;
        yield return null;
        List<GridCell> weapons = new List<GridCell>();
        foreach (var cell in GameObject.FindObjectsOfType<GridCell>())
        {
            if (cell.cellInfo != null && cell.cellInfo.isWeapon() && !cell.GetComponentInParent<TetrisShape>())
            {
                weapons.Add(cell);
            }
        }


        weapons.Sort(SortByDistanceToTarget);

        foreach (var weapon in weapons)
        {

            foreach (var enemy in getSurroundingCells(weapon.index))
            {
                if (enemy.cellInfo.isEnemy())
                {

                    if (weapon.cellInfo.attackMode == "exchangePosition")
                    {
                        yield return StartCoroutine(exchangeCardAnim(weapon, enemy));
                    }


                    List<Vector2> mightUpdatedPositions = new List<Vector2>();

                    var otherEnemies = new List<GridCell>();
                    if (weapon.cellInfo.attackMode == "allInDrection")
                    {
                        var eDir = enemy.index - weapon.index;
                        var checkPosition = enemy.index + eDir;
                        while (isPositionValid(checkPosition))
                        {
                            if (isOccupied(checkPosition))
                            {
                                var cell = getCellOnPosition(checkPosition);
                                if (cell.GetComponent<EnemyCell>())
                                {
                                    otherEnemies.Add(cell);
                                    playWeaponAttackEffect(weapon, cell);
                                }
                            }


                            checkPosition += eDir;

                        }
                    }
                    else
                    {
                        //playWeaponAttackEffect(scell, enemy);
                    }
                    playWeaponAttackEffect(weapon, enemy);
                    yield return new WaitForSeconds(animTime * 2);

                    yield return StartCoroutine(decreaseBothCell(weapon, enemy, mightUpdatedPositions, otherEnemies));

                    // 
                    if (weapon.cellInfo.attackMode == "exchangePosition")
                    {
                        break;
                    }

                    //yield return StartCoroutine(movePositions(mightUpdatedPositions));

                }
            }

        }

        testTime = 0;
    }

    IEnumerator showMoveAnim(GridCell sendCell, GridCell targetCell)
    {

        var animCell = generateCell(sendCell.index, sendCell.type).GetComponent<GridCell>();
        animCell.GetComponent<GridCell>().collider.enabled = false;
        yield return StartCoroutine(moveCardAnim(animCell, targetCell.index));
        Destroy(animCell.gameObject);
    }

    IEnumerator moveEnemiesPre()
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
        foreach(var enemy in enemies)
        {
            if (enemy.GetComponent<EnemyCell>().isFirst)
            {
                //dont move for the first step
                continue;
            }

            if (enemy.GetComponent<GridCell>().cellInfo.specialMode != null && enemy.GetComponent<GridCell>().cellInfo.specialMode.Contains("FlyAtPlace"))
            {

                int dangerousCount = 0;
                foreach (var e in getSurroundingCells(enemy.index))
                {
                    if (e.cellInfo.isTrap() || e.cellInfo.isWeapon())
                    {
                        dangerousCount++;
                    }
                }
                if (dangerousCount >= 2)
                {
                    //fly to a random position
                    var currentDistanceToCenter = distance(enemy.GetComponent<GridCell>().index);
                    List<Vector2> sameDistanceList = new List<Vector2>();
                    List<Vector2> notSameDistanceList = new List<Vector2>();
                    //find position that can swap
                    for (int i = -4; i <= 4; i++)
                    {
                        for (int j = -4; j <= 4; j++)
                        {
                            var vec = new Vector2(i, j);
                            var newDistanc = distance(vec);
                            if (!isOccupied(vec))
                            {

                                if (newDistanc == currentDistanceToCenter && vec != enemy.GetComponent<GridCell>().index)
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
                    Vector2 finalIndex = enemy.GetComponent<GridCell>().index;
                    if (sameDistanceList.Count > 0)
                    {
                        finalIndex = Utils.randomList(sameDistanceList);
                    }
                    else if (notSameDistanceList.Count > 0)
                    {

                        finalIndex = Utils.randomList(notSameDistanceList);
                    }


                    var go = Instantiate(Resources.Load<GameObject>("effect/fly"), getCellPosition(enemy.GetComponent<GridCell>().index), Quaternion.identity);


                    SFXManager.Instance.play("birdflap");
                    var d = finalIndex - enemy.GetComponent<GridCell>().index;
                    go.transform.DOMove(getCellPosition(d.normalized + enemy.GetComponent<GridCell>().index), GridController.Instance.animTime + 0.1f);
                    Destroy(go, 1f);

                    yield return moveCardAndOccupyAnim(enemy.GetComponent<GridCell>(), finalIndex);
                }
            }

            if (enemy.GetComponent<GridCell>().cellInfo.specialMode != null && enemy.GetComponent<GridCell>().cellInfo.specialMode.Contains("stomp"))
            {
                playStompAttackEffect(enemy.GetComponent<GridCell>());
                yield return new WaitForSeconds(animTime);
                foreach (var c in getSurroundingCells(enemy.GetComponent<GridCell>().index))
                {
                    if (c && c.amount > 0)
                    {

                        minuseHpByHalf(c);
                        if (c.amount <= 0)
                        {
                            yield return destroy(c.gameObject);
                        }
                    }
                }
            }
        }
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

            if (!enemy || enemy.amount <= 0)
            {
                continue;
            }


            if (enemy.GetComponent<EnemyCell>().isFirst)
            {
                enemy.GetComponent<EnemyCell>().isFirst = false;
                //dont move for the first step
                continue;
            }



            if (enemy.cellInfo.attackMode == "heal")
            {
                //steal nearby resources
                foreach (var e in getSurroundingCells(enemy.index))
                {
                    if (e.cellInfo.isEnemy())
                    {
                        e.addAmount(1);
                        playHealEffect(enemy, e);
                        yield return new WaitForSeconds(animTime);

                    }
                }
            }

            if (enemy.cellInfo.attackMode == "steal")
            {
                //steal nearby resources
                foreach (var resource in getSurroundingCells(enemy.index))
                {
                    if (resource.cellInfo.isFood())
                    {
                        SFXManager.Instance.play("steal");
                        yield return StartCoroutine(showMoveAnim(resource, enemy));
                        resource.decreaseAmount(1);
                        enemy.addAmount(1); //show add amount effect
                        if (resource.amount <= 0)
                        {
                            yield return StartCoroutine(destroy(resource.gameObject));
                        }

                    }
                }
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
                    if (cell.cellInfo.attackMode == "3x3")
                    {

                        playExplodeAttackEffect(cell);
                    }
                    else
                    {
                        playTrapAttackEffect(cell);
                    }
                    yield return new WaitForSeconds(animTime);

                    //yield return StartCoroutine(destroy(cell.gameObject));

                    if (cell.cellInfo.attackMode == "3x3")
                    {
                        yield return StartCoroutine(destroy(cell.gameObject));
                        foreach (var e in getSurrounding3x3Cells(cell.index))
                        {
                            if (e.cellInfo.isEnemy())
                            {
                                minuseHpByHalf(e);


                                if (e.amount <= 0)
                                {
                                    yield return StartCoroutine(destroy(e.gameObject));
                                }
                            }
                        }
                    }
                    else
                    {

                        StartCoroutine(decreaseBothCell(cell, enemy, mightUpdatedPositions));
                        //if (enemy && enemy.amount > 0)
                        //{
                        //    release(enemy.index);
                        //    occupy(trapIndex, enemy);
                        //}
                       // yield return StartCoroutine(movePositions(mightUpdatedPositions));
                    }


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
                        //addFarerCellIntoPositions(mightUpdatedPositions, pos);

                       // yield return StartCoroutine(movePositions(mightUpdatedPositions));

                    }
                    else
                    {

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
                                //addFarerCellIntoPositions(mightUpdatedPositions, pos);
                               // addFarerCellIntoPositions(mightUpdatedPositions, scell.index);

                                //yield return StartCoroutine(movePositions(mightUpdatedPositions));
                            }
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
                //addFarerCellIntoPositions(mightUpdatedPositions, pos);

                //yield return StartCoroutine(movePositions(mightUpdatedPositions));
            }


            if (!enemy || enemy.amount <= 0)
            {
                continue;
            }



            enemies.Sort(SortByDistanceToTarget);
        }
    }

    //void addFarerCellIntoPositions(List<Vector2> positions, Vector2 pos)
    //{
    //    var surroundingCells = getSurroundingCells(pos);
    //    foreach (var scell in surroundingCells)
    //    {
    //        if (distance(scell.index) > distance(pos))
    //        {
    //            if (!positions.Contains(scell.index))
    //            {
    //                positions.Add(scell.index);
    //            }
    //        }
    //    }
    //}

    //IEnumerator movePositions(List<Vector2> positions)
    //{
    //    yield return null;
    //    positions.Sort(SortByDistanceToPlayer);
    //    int test2 = 30;
    //    while (positions.Count > 0)
    //    {
    //        test2--;
    //        if (test2 < 0)
    //        {

    //            Debug.LogError("move too many times");
    //            break;
    //        }
    //        //get cell
    //        var pos = positions[0];
    //        positions.RemoveAt(0);
    //        var cell = getCellOnPosition(pos);
    //        if (cell == null)
    //        {
    //            //Debug.LogError("cell is null");
    //            continue;
    //        }

    //        //move cell to bottom
    //        var dir = getDir(pos);
    //        int test = 30;


    //        if (CheatManager.Instance.wouldMoveCells)
    //        {
    //            while (!isOccupied(pos - dir))
    //            {
    //                test--;
    //                if (test <= 0)
    //                {
    //                    Debug.LogError("move too many times");
    //                    break;
    //                }
    //                yield return StartCoroutine(moveCardAndOccupyAnim(getCellOnPosition(pos), pos - dir));

    //               // addFarerCellIntoPositions(positions, pos);

    //                pos = pos - dir;
    //                dir = getDir(pos);
    //            }
    //        }


    //        //yield return StartCoroutine(combineAround(cell.GetComponent<GridCell>()));

    //        //if (CheatManager.Instance.wouldMoveCells)
    //        //{
    //        //    if (!isOccupied(pos))
    //        //    {
    //        //        //if the cell is destroyed after combien, find cells that might be affected and add to positions
    //        //        addFarerCellIntoPositions(positions, pos);
    //        //    }
    //        //}
    //        positions.Sort(SortByDistanceToPlayer);
    //    }
    //}

    IEnumerator placeCellsAnim(List<GameObject> cells)
    {

        LogManager.Instance.log("placeCellsAnim set is moving to true");
        isMoving = true;
        //
        foreach (var cell in cells)
        {
            cell.GetComponent<GridCell>().collider.enabled = true;




            occupy(cell.GetComponent<GridCell>().index, cell.GetComponent<GridCell>());



            //if (cell.GetComponent<GridCell>().cellInfo.specialMode != null && cell.GetComponent<GridCell>().cellInfo.specialMode.Contains("FlyAtPlace"))
            //{
            //    //fly to a random position
            //    var currentDistanceToCenter = distance(cell.GetComponent<GridCell>().index);
            //    List<Vector2> sameDistanceList = new List<Vector2>();
            //    List<Vector2> notSameDistanceList = new List<Vector2>();
            //    //find position that can swap
            //    for (int i = -4; i <= 4; i++)
            //    {
            //        for (int j = -4; j <= 4; j++)
            //        {
            //            var vec = new Vector2(i, j);
            //            var newDistanc = distance(vec);
            //            if (!isOccupied(vec))
            //            {

            //                if (newDistanc == currentDistanceToCenter && vec != cell.GetComponent<GridCell>().index)
            //                {
            //                    sameDistanceList.Add(vec);
            //                }
            //                else
            //                {
            //                    notSameDistanceList.Add(vec);
            //                }
            //            }
            //        }
            //    }
            //    Vector2 finalIndex = cell.GetComponent<GridCell>().index;
            //    if (sameDistanceList.Count > 0)
            //    {
            //        finalIndex = Utils.randomList(sameDistanceList);
            //    }
            //    else if (notSameDistanceList.Count > 0)
            //    {

            //        finalIndex = Utils.randomList(notSameDistanceList);
            //    }


            //    var go = Instantiate(Resources.Load<GameObject>("effect/fly"), getCellPosition(cell.GetComponent<GridCell>().index), Quaternion.identity);
            //    var dir = finalIndex - cell.GetComponent<GridCell>().index;
            //    go.transform.DOMove(getCellPosition(dir.normalized + cell.GetComponent<GridCell>().index), GridController.Instance.animTime + 0.1f);
            //    Destroy(go, 1f);

            //    yield return moveCardAndOccupyAnim(cell.GetComponent<GridCell>(), finalIndex);

            //}

            //if (cell.GetComponent<GridCell>().cellInfo.specialMode != null && cell.GetComponent<GridCell>().cellInfo.specialMode.Contains("stomp"))
            //{
            //    playStompAttackEffect(cell.GetComponent<GridCell>());
            //    yield return new WaitForSeconds(animTime);
            //    foreach (var c in getSurrounding3x3Cells(cell.GetComponent<GridCell>().index))
            //    {
            //        minuseHpByHalf(c);
            //        if (c.amount <= 0)
            //        {
            //            yield return destroy(c.gameObject);
            //        }
            //    }
            //}



            foreach (var spriteRenderer in cell.GetComponentsInChildren<SpriteRenderer>())
            {
                spriteRenderer.sortingOrder -= 50;
            }
        }
        yield return null;



        //List<Vector2> positions = new List<Vector2>();
        ////find combinations from current to previous
        //foreach (var cell in cells)
        //{
        //    if (cell == null)
        //    {
        //        Debug.Log("cell is null");
        //    }
        //    positions.Add(cell.GetComponent<GridCell>().index);
        //}

        List<GridCell> gridCells = new List<GridCell>();
        foreach (var cell in cells)
        {
            gridCells.Add(cell.GetComponent<GridCell>());
        }

        yield return StartCoroutine(GridGeneration.Instance.calculateCombinedResult(gridCells, null));
        //yield return StartCoroutine(movePositions(positions));
        yield return StartCoroutine(moveEnemiesPre());
        yield return StartCoroutine(moveWeapons());
        yield return StartCoroutine(moveEnemies());

        yield return new WaitForSeconds(0.1f);
        LogManager.Instance.log("placeCellsAnim set is moving to false");
        isMoving = false;
        resetSwap();
        allAttracts = null;
        round++;
        AchievementManager.Instance.clear("round");
        if (round >= 2)
        {

            //show tutorial drag
            NewCellManager.Instance.ShowNewCell("tutorialRotate");
        }
        if (round > 3)
        {
            //show tutorial drag
            NewCellManager.Instance.ShowNewCell("tutorialSwap");

        }
        if (round > 6)
        {
            //show tutorial drag
            NewCellManager.Instance.ShowNewCell("tutorialRightClick");

        }

        EventPool.Trigger("moveAStep");

        TetrisGeneration.Instance.generateATetrisShape();

    }

    public Vector2 getIndexFromPosition(Vector2 cellIndex)
    {
        return Vector2Int.RoundToInt((cellIndex - (Vector2)mainBoard.position) / cellSize);
    }

    public Vector2 getCellPosition(Vector2 cellIndex)
    {
        return cellIndex * cellSize + (Vector2)mainBoard.position;
    }
    public GameObject generateCellAndOccupy(Vector2 index, string type, int amount = -1)
    {
        var res = generateCell(index, type, amount);
        occupy(index, res.GetComponent<GridCell>());
        return res;
    }

    public GameObject generateTargetCell(Vector2 index)
    {
        Vector2 position = getCellPosition(index);

        GameObject res;
        res = Instantiate(targetCellPrefab, position, Quaternion.identity, mainBoard);
        return res;
    }

    public GameObject generateCell(Vector2 index, string type, int amount = -1, bool asjustBoard = true)
    {
        Vector2 position = getCellPosition(index);
        if (!asjustBoard)
        {
            position -= (Vector2)mainBoard.position;
        }
        GameObject res;
        if (type == "addHP")
        {

            res = Instantiate(modifyPrefab, position, Quaternion.identity, mainBoard);
        }
        //else if (type == "destroy")
        //{
        //    res = Instantiate(modifyPrefab, position, Quaternion.identity, mainBoard);
        //}
        else
        //if (type == "empty")
        //{

        //    res = Instantiate(emptyPrefab, position, Quaternion.identity, mainBoard);
        //}
        //else if (CellManager.Instance.isCell(type))
        {

            res = Instantiate(cellPrefab, position, Quaternion.identity, mainBoard);
        }

        LogManager.Instance.logGenerate(index, type);
        //else
        //{

        //    res = Instantiate(itemPrefab, cellParents[index].position, Quaternion.identity, cellParents[index]);
        //}

        //  res.transform.localScale = Vector3.one;
        //res.transform.DOPunchScale(Vector3.one, animTime);
        var scalex = res.transform.localScale.x;
        res.transform.localScale = new Vector3(0, res.transform.localScale.y, res.transform.localScale.z);
        res.transform.DOScaleX(scalex, animTime);

        var typeSplit = type.Split('_');
        if (typeSplit.Length > 1)
        {
            type = typeSplit[0];
            amount = int.Parse(typeSplit[1]);
        }

        res.GetComponent<GridCell>().init(type, index, amount);

        return res;
    }

    float testTime = 0;
    float testMaxTime = 2f;
    // Update is called once per frame
    void Update()
    {
        if(isMoving)
        {
            testTime+=Time.deltaTime;
            if (testTime >= testMaxTime)
            {
                testTime = 0;
                isMoving = false;
            }
        }
        else
        {

            testTime = 0;
        }
    }
}
