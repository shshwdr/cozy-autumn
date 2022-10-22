using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TetrisShape : MonoBehaviour
{

    List<Vector2> tetrisShape;
    List<GameObject> tetrises = new List<GameObject>();

    List<Vector2> tetrisShapeAfterRotation;
    int rotateTime = 0;
    bool isDragging = false;
    bool isUnlocked = false;
    Vector3 dragOriginalPosition;

    Vector2 currentFinalPosition;


    public void getReady()
    {
        isUnlocked = true;


        foreach(var tetri in tetrises)
        {
            var cell = tetri.GetComponent<GridCell>();
            NewCellManager.Instance.ShowNewCell(cell.type);
        }
    }

    public void init(List<Vector2> shape)
    {
        tetrisShape = new List<Vector2>(shape);

        tetrisShapeAfterRotation = new List<Vector2>(shape);
        for (int i = 0; i < tetrisShape.Count; i++)
        {
            var index = tetrisShape[i];
            var card = DeckManager.Instance.drawCard(false);
            var go = GridGeneration.Instance.generateCell(index+(Vector2)transform.position, card, -1,false);
            go.transform.parent = transform;
            go.transform.position = transform.position +(Vector3) index * GridGeneration.Instance.cellSize;
            tetrises.Add(go);
            go.GetComponent<GridCell>().collider.enabled = false;
        }
    }



    // Start is called before the first frame update
    void Start()
    {

    }
    void clearGeneratedCombineResult()
    {

        foreach (var c in generatedCombineResult)
        {
            Destroy(c);
        }
        generatedCombineResult.Clear();
    }
    List<GameObject> generatedCombineResult = new List<GameObject>();
    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (isDragging)
            {
                var mousePosition = new Vector3(mousePos.x, mousePos.y, dragOriginalPosition.z);
                mousePosition -= GridGeneration.Instance.mainBoard.position;
                mousePosition /= GridGeneration.Instance.cellSize;
                var newFinalPosition = new Vector2(Mathf.Round(mousePosition.x), Mathf.Round(mousePosition.y)); 
                newFinalPosition *= GridGeneration.Instance.cellSize;
                newFinalPosition +=(Vector2) GridGeneration.Instance.mainBoard.position;
                if (currentFinalPosition != newFinalPosition)
                {
                    currentFinalPosition = newFinalPosition;
                    transform.position = currentFinalPosition;
                    if (generatedCombineResult.Count != 0)
                    {
                        clearGeneratedCombineResult();
                    }
                    bool canPlaceCell = canPlace();
                    if (canPlaceCell)
                    {
                        //show how nearby cells would update
                        
                        StartCoroutine( GridGeneration.Instance.calculateCombinedResult(allCells(), generatedCombineResult));
                    }
                }

            }
        }
        

        if (Input.GetMouseButtonDown(1) && isUnlocked)
        {
            rotate90Degree();
            if (generatedCombineResult.Count != 0)
            {
                clearGeneratedCombineResult();
            }
            bool canPlaceCell = canPlace();
            if (canPlaceCell)
            {
                //show how nearby cells would update

                StartCoroutine(GridGeneration.Instance.calculateCombinedResult(allCells(), generatedCombineResult));
            }
        }
        //if (Input.GetMouseButtonDown(0))
        //{
        //    if (canPlaceCell)
        //    {

        //        tryPlace();
        //    }
        //}
    }

    List<GridCell> allCells()
    {
        List<GridCell> res = new List<GridCell>();
        foreach(var c in tetrises)
        {
            res.Add(c.GetComponent<GridCell>());

        }
        return res;
    }

    private void OnMouseDown()
    {

        if (!GridGeneration.Instance.canMoveCell())
        {
            return;
        }
        if (!isUnlocked)
        {
            return;
        }
        isDragging = true;
        dragOriginalPosition = transform.position;

        foreach(var cell in tetrises)
        {
        cell.GetComponent<GridCell>().mask.SetActive(true);
        }
    }
    private void OnMouseUp()
    {

        foreach (var cell in tetrises)
        {
            cell.GetComponent<GridCell>().mask.SetActive(false);
        }
        if (!isUnlocked)
        {
            return;
        }
        bool canPlaceCell = canPlace();

        if (canPlaceCell)
        {
            tryPlace();
            isUnlocked = false;
        }
        else
        {
            isDragging = false;

            transform.position = dragOriginalPosition;
        }
        clearColor();
    }

    void updateColor(Transform trans, bool isValid)
    {
        var color = getColor(isValid);
        foreach(var spriterender in trans.GetComponentsInChildren<SpriteRenderer>())
        {
            spriterender.color = color;
        }
    }

    Color getColor(bool isValid)
    {
        if(!isUnlocked || !isDragging)
        {

            return Color.white;
        }
        if (isValid)
        {

            return Color.white;
        }
        else
        {
            return new Color(255, 255, 255, 0.6f);

        }
    }

    void clearColor()
    {
        foreach(var t in tetrises)
        {
            updateColor(t.transform, true);
        }
    }

    bool canPlace()
    {
        bool res = true;
        bool surroundExistedCell = false;
        bool outOfBorder = false;
        for (int i = 0; i < tetrisShape.Count; i++)
        {
            var index = GridGeneration.Instance. getIndexFromPosition(tetrisShapeAfterRotation[i] + currentFinalPosition);
            tetrises[i].GetComponent<GridCell>().index = index;

            if (Mathf.Abs(index.x )> GridGeneration.Instance.gridSizex || Mathf.Abs( index.y) > GridGeneration.Instance.gridSizey)
            {
                outOfBorder = true;
                //updateColor(tetrises[i].GetComponent<GridCell>().transform,false);
                //tetrises[i].GetComponent<GridCell>().bk.GetComponent<SpriteRenderer>().color = getColor(false);
                continue;
            }

            surroundExistedCell |= GridGeneration.Instance.isNextToOccupiedCells(index);

            if (GridGeneration.Instance.isOccupied(index))
            {
                res = false;
                //updateColor(tetrises[i].GetComponent<GridCell>().transform, false);
                //tetrises[i].GetComponent<GridCell>().bk.GetComponent<SpriteRenderer>().color = getColor(false);
            }
            else
            {
                if(tetrises[i] == null || tetrises[i].GetComponent<GridCell>() == null || tetrises[i].GetComponent<GridCell>().cellInfo == null || tetrises[i].GetComponent<GridCell>().bk == null || tetrises[i].GetComponent<GridCell>().bk.GetComponent<SpriteRenderer>() == null)
                {
                    Debug.LogError("???");
                }

                //updateColor(tetrises[i].GetComponent<GridCell>().transform, true);
                //tetrises[i].GetComponent<GridCell>().bk.GetComponent<SpriteRenderer>().color = getColor(true);
            }
        }
        var finalRes = res && surroundExistedCell && !outOfBorder;
        if (!finalRes)
        {
            for (int i = 0; i < tetrisShape.Count; i++)
            {

                updateColor(tetrises[i].GetComponent<GridCell>().transform, false);
               // tetrises[i].GetComponent<GridCell>().bk.GetComponent<SpriteRenderer>().color = getColor(false);
            }
        }
        else
        {
            for (int i = 0; i < tetrisShape.Count; i++)
            {

                updateColor(tetrises[i].GetComponent<GridCell>().transform, true);
                // tetrises[i].GetComponent<GridCell>().bk.GetComponent<SpriteRenderer>().color = getColor(false);
            }
        }



        return finalRes;
    }

    void tryPlace()
    {
        clearGeneratedCombineResult();
        for (int i = 0; i < tetrisShape.Count; i++)
        {
            var index = tetrisShape[i];
            var go = tetrises[i];
            go.GetComponent<GridCell>().index = GridGeneration.Instance.getIndexFromPosition(tetrisShapeAfterRotation[i] +currentFinalPosition);
            go.transform.parent = null;
            go.transform.position =GridGeneration.Instance.getCellPosition(go.GetComponent<GridCell>().index );
            DeckManager.Instance.placedCardCount++;
        }
        //DeckManager.Instance.updatePlacedCard();

        GridGeneration.Instance.placeCells(tetrises);
        Destroy(gameObject);
        
    }
    void rotate90Degree()
    {
        rotateTime++;
        rotateTime %= 4;
        for (int i = 0; i < tetrisShape.Count; i++)
        {
            var index = tetrisShape[i];
            var go = tetrises[i];
            tetrisShapeAfterRotation[i] = RotatePointAroundPoint(index, Vector2.zero, rotateTime * 90) *GridGeneration.Instance.cellSize;

            tetrises[i].GetComponent<GridCell>().index = index;
            go.transform.DOLocalMove(tetrisShapeAfterRotation[i], 0.3f);

            //go.transform.localPosition = RotatePointAroundPoint(index, Vector2.zero, rotateTime * 90);

        }
    }

    Vector3 RotatePointAroundPoint(Vector3 point1, Vector3 point2, float angle)
    {
        angle *= Mathf.Deg2Rad;
        var x = Mathf.Cos(angle) * (point1.x - point2.x) - Mathf.Sin(angle) * (point1.y - point2.y) + point2.x;
        var y = Mathf.Sin(angle) * (point1.x - point2.x) + Mathf.Cos(angle) * (point1.y - point2.y) + point2.y;
        return Vector3Int.RoundToInt( new Vector3 (x, y));
    }


}
