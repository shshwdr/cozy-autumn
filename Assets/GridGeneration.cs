using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGeneration : Singleton<GridGeneration>
{
    public float cellSize = 2.1f;
    Vector2 playerPosition = new Vector2 (0, 0);
    [HideInInspector] public GridCell playerCell;
    public Transform mainBoard;
    public GameObject cellPrefab;
    public float animTime = 0.3f;

    Dictionary<Vector2, GridCell> indexToCell = new Dictionary<Vector2, GridCell>();

    Vector2[] dirs = new Vector2[] { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };
    // Start is called before the first frame update
    void Start()
    {

        playerCell = generateCell(playerPosition, "player").GetComponent<GridCell>();
        playerCell.renderer.sprite = Resources.Load<Sprite>("cell/" + CharacterManager.Instance.currentChar);
        StartCoroutine(placeCellsAnim(new List<GameObject>() { playerCell.gameObject }));
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
        release(cell.index);
        
        occupy(targetPosition,cell);
        yield return StartCoroutine(moveCardAnim(cell, targetPosition));
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
                    if(combinationGenerated == newCell.type)
                    {
                        newCell.addAmount();
                    }
                    else
                    {

                        generateCellAndOccupy(newIndex, combinationGenerated);
                        yield return StartCoroutine(destroy(newCell.gameObject));
                    }
                }
            }
        }
        if (hasCombined)
        {
            yield return StartCoroutine( destroy(cell.gameObject));
            
        }
    }
    static int distance(Vector2 index)
    {
        index = Vector2Int.RoundToInt(index);
        return (int)(Mathf.Abs( index.x) + Mathf.Abs(index.y));
    } 
    static int SortByDistanceToPlayer(GameObject g1, GameObject g2)
    {
        GridCell gc1 = g1.GetComponent<GridCell>();
        GridCell gc2 = g2.GetComponent<GridCell>();
        return distance(gc1.index) .CompareTo( distance(gc2.index));
    }

    static int SortByDistanceToPlayer(Vector2 v1, Vector2 v2)
    {
        return distance(v1).CompareTo(distance(v2));
    }

    Vector2 getDir(Vector2 pos)
    {
        if(pos == Vector2.zero)
        {
            return Vector2.zero;
        }

        var dir = new Vector2(0, 1);
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

    IEnumerator placeCellsAnim(List<GameObject> cells)
    {
        //
        foreach(var cell in cells)
        {
            occupy(cell.GetComponent<GridCell>().index, cell.GetComponent<GridCell>());
            foreach (var spriteRenderer in cell.GetComponentsInChildren<SpriteRenderer>())
            {
                spriteRenderer.sortingOrder -= 50;
            }
        }
        yield return null;

        //sort cells
        cells.Sort(SortByDistanceToPlayer);


        List<Vector2> positions = new List<Vector2>();

        List<int> combinedCells = new List<int>();
        //find combinations from current to previous
        foreach (var cell in cells)
        {
            if(cell == null)
            {
                Debug.Log("cell is null");
            }
            positions.Add(cell.GetComponent<GridCell>().index);
            yield return StartCoroutine(combineAround(cell.GetComponent<GridCell>()));
        }

        

        for (int i = 0; i < positions.Count; i++)
        {
            var pos = positions[i];
            var dir = getDir(pos);
            int test = 30;
            if (!isOccupied(positions[i]))
            {
                while (isOccupied(pos + dir))
                {
                    test--;
                    if (test <= 0)
                    {
                        Debug.LogError("move too many times");
                        break;
                    }
                    yield return StartCoroutine(moveCardAndOccupyAnim(getCellOnPosition(pos + dir), pos));

                    pos = pos + dir;
                }
            }
            else
            {
                while(!isOccupied(pos - dir))
                {
                    test--;
                    if (test <= 0)
                    {
                        Debug.LogError("move too many times");
                        break;
                    }
                    yield return StartCoroutine(moveCardAndOccupyAnim(getCellOnPosition(pos), pos-dir));

                    pos = pos - dir;
                    dir = getDir(pos);
                }
            }
        }


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
