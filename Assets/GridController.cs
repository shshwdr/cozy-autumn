using DG.Tweening;
using Doozy.Examples;
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


    public List<int> moveCountToLeaf;
    int currentLeafIndex = 0;

   public  int moveCount = 0;
    public Transform bossParent;

    List<Transform> cellParents = new List<Transform>();

    GridCell playerCell;
    GridCell allyCell;
    GridEmpty emptyCell;
    public int playerCellIndex { get { return playerCell.index; } }
    public int allyCellIndex { get { return allyCell? allyCell.index:-1; } }
    int emptyCellIndex { get { return emptyCell.index; } }

    int originalPlayerCell = 4;
    int originalEmptyCell = 0;

    public int snakeChance = 20;
    public int nutChance = 40;
    public int trapChance = 15;


    public GameObject cellPrefab;
    public GameObject emptyPrefab;
    public GameObject itemPrefab;
    public GameObject bkPrefab;

    public Transform mainBoard;

    public GameObject fireVFX;

    public Boss boss;


    public float animTime = 1f;
   public  bool isMoving = false;
    // Start is called before the first frame update
    void Start()
    {
        // initMainBoard();
        StartCoroutine(test());

        bk.Find("leaf" + currentLeafIndex).GetComponent<ParticleSystem>().Play();
        AchievementManager.Instance.clearAll();
    }

    IEnumerator test()
    {
        yield return new WaitForSeconds(0.1f);

        if (AchievementManager.Instance.visitedList.Count == 0)
        {

            //PopupManager.Instance.showEvent("Help the squirrel to collect acorns and survive the autumn and winter, and try to make the place cozy for it.", "OK");
        }

        yield return new WaitForSeconds(0.1f);
        initMainBoard();
    }

    void step()
    {
        moveCount++;
        if(currentLeafIndex<moveCountToLeaf.Count && moveCount >= moveCountToLeaf[currentLeafIndex])
        {
            currentLeafIndex++;
            updateLeafFalling();
        }
    }
    public Transform bk;
    void updateLeafFalling()
    {
        Debug.Log("update leaf " + currentLeafIndex);
        var parent = bk.Find("leaf" + currentLeafIndex);
        foreach(var child in parent.GetComponentsInChildren<ParticleSystem>())
        {
            child.Play();
        }
        if (currentLeafIndex > 0)
        {

            var parent2 = bk.Find("leaf" + (currentLeafIndex-1));
            foreach (var child in parent2.GetComponentsInChildren<ParticleSystem>())
            {
                child.Stop();
            }
        }
        var sky = bk.Find("sky");
        if (currentLeafIndex >= 6)
        {
            DOTween.To(() => sky.GetComponent<SpriteRenderer>().color, x => sky.GetComponent<SpriteRenderer>().color = x, new Color(255,255,255), 1);

            
        }
        else if (currentLeafIndex >= 4)
        {
            //turn color of key to grey 209

            DOTween.To(() => sky.GetComponent<SpriteRenderer>().color, x => sky.GetComponent<SpriteRenderer>().color = x, new Color(188, 188, 188), 1);
        }
    }

    private void OnDestroy()
    {
        //var sky = bk.Find("sky");
        //sky.GetComponent<SpriteRenderer>().DOKill();
    }

    public void showDangerousCell(string dangerName, List<int> indices)
    {
        foreach(var index in indices)
        {
            cellParents[index].GetComponent<GridBackground>().addDangerous(dangerName);
        }
    }

    public Vector3 getCenterOfCells(List<int> indices)
    {
        Vector3 res = Vector3.zero;
        foreach (var i in indices)
        {
            var dangerousCell = cellParents[i];
            res += dangerousCell.transform.position;
        }
        return res / indices.Count;
    }


    public void hideDangerousCell(string dangerName, List<int> indices)
    {
        foreach (var index in indices)
        {
            cellParents[index].GetComponent<GridBackground>().removeDangerous(dangerName);
        }
    }
    // if need to support allies here.. change this
    public IEnumerator attackAndMovePlayer(List<int> dangerousIndices, EnemyCell enemy)
    {
        yield return null;
        //find player
        yield return StartCoroutine(enemy.activeAttack(true));

            //move player to a safe place
            if (dangerousIndices.Contains(emptyCellIndex))
            {
                for (int i = 0; i < 9; i++)
                {
                    if (!dangerousIndices.Contains(i))
                    {
                        yield return StartCoroutine(exchangeCard(playerCell, i));
                    }
                }
            }
            else
            {
                yield return StartCoroutine(exchangeCard(playerCell, emptyCellIndex));
            }
       
    }

    void initMainBoard()
    {
        isMoving = true;
        float xStartPosiiton = -cellSize * (cellCountX / 2) + mainBoard.position.x;
        float yStartPosiiton = -cellSize * (cellCountY / 2) + mainBoard.position.y;
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

                if (originalEmptyCell == index)
                {
                    emptyCell = generateCell(index, "empty").GetComponent<GridEmpty>();

                }
                else if (originalPlayerCell == index)
                {
                    playerCell = generateCell(index, "player").GetComponent<GridCell>();

                }
                else
                {
                    generateCell(index, "leaf"+ currentLeafIndex);
                }
                index++;

                SFXManager.Instance.play("cardshow");
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

    int getIndex(int x,int y)
    {
        return x * cellCountX + y;
    }

    List<int> getAdjacentCellsIndex(int i)
    {
        List<int> res = new List<int>();

        var ix = i / cellCountX;
        var iy = i % cellCountY;

        if (ix-1 >= 0)
        {
            res.Add(getIndex(ix - 1, iy));
        }
        if (iy - 1 >= 0)
        {
            res.Add(getIndex(ix, iy-1));
        }

        if (ix + 1 < cellCountX)
        {
            res.Add(getIndex(ix + 1, iy));
        }

        if (iy + 1 < cellCountY)
        {
            res.Add(getIndex(ix, iy + 1));
        }

        return res;
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
            if (indices.Contains(cell.index) && isType(cell, type) )
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
        if(!cell || cell.cellInfo == null)
        {
            Debug.LogError("?");
        }
        return cell.cellInfo.type == type;
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

    public bool isCharacterAround(int index)
    {
        return isPlayerAround (index) || isAllyAround(index);
    }

    public bool isAllyAround(int index)
    {
        return (allyCellIndex != -1 && isTwoIndexCrossAdjacent(index, allyCellIndex));
    }

    public bool isPlayerAround(int index)
    {
        return isTwoIndexCrossAdjacent(index, playerCellIndex);
    }

    public List<GridCell> getPlayerAdjacentCells()
    {
        return getAdjacentCells(playerCellIndex);
    }
    public List<GridCell> getAllyAdjacentCells()
    {
        return getAdjacentCells(allyCellIndex);
    }

    public List<GridCell> getAdjacentCells(int index)
    {

        var res = new List<GridCell>();
        var indices = getAdjacentCellsIndex(index);
        foreach (var i in indices)
        {
            foreach (var cell in cellParents[i].GetComponentsInChildren<GridCell>())
            {
                res.Add(cell);
            }
        }
        return res;
    }

    public Transform getPlayerTransform()
    {
        return playerCell.transform;
    }

    public Transform getAllyTransform()
    {
        return allyCell? allyCell.transform:null;
    }

    public GridCell getAllyGridCell()
    {
        return allyCell;
    }
    GameObject generateCell(int index, string type, int amount = -1)
    {
        GameObject res;
        if(type == "empty")
        {

            res = Instantiate(emptyPrefab, cellParents[index].position, Quaternion.identity, cellParents[index]);
        }
        else if (CellManager.Instance.isCell(type))
        {

            res = Instantiate(cellPrefab, cellParents[index].position, Quaternion.identity, cellParents[index]);
        }
        else
        {

            res = Instantiate(itemPrefab, cellParents[index].position, Quaternion.identity, cellParents[index]);
        }

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

    //void generateItem(int index, string type)
    //{

    //    var child = Instantiate(Resources.Load<GameObject>("cell/" + type), cellParents[index].position, Quaternion.identity, cellParents[index]);
    //    //child.GetComponent<GridCell>().init(index);
    //}

    public void addEmpty(int index)
    {
        generateCell(index, "leaf"+ currentLeafIndex);
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

    public void exploreCell(GridCell cell)
    {

        if (isMoving)
        {

            return;
        }
        isMoving = true;
        StartCoroutine(exploreCellAnim(cell));
    }

    public void moveCell(GridCell cell, bool canDraw)
    {

        if (isMoving)
        {

            return;
        }
        isMoving = true;
        StartCoroutine(moveCellAnim(cell, canDraw));
    }

    void finishMove()
    {
        isMoving = false;
        Debug.Log("empty index " + emptyCell);

        FindObjectOfType<AchievementManager>().ShowAchievement("notMoving");

        GameObject.FindObjectOfType<DayCell>(true).updateText();
    }
    public bool hasPlayerMoved = false;
    void destroy(GameObject go)
    {
        go.transform.DOScale(Vector3.zero, animTime);
        go.transform.DOLocalMoveY(1, animTime);
        Destroy(go, animTime);
    }

    IEnumerator trapCellCalculation(EnemyCell cell, GridItem trap)
    {
        yield return null;
        {
            var trapDamage = trap.amount;
            var cellHp = cell.hp;
            var damage = Mathf.Min(trapDamage, cellHp);
            cell.getDamage(damage);
            trap.decreaseAmount(damage);
            var trapName = trap.type;
            SFXManager.Instance.play(trapName);
            destroy(trap.gameObject);

            var go = Instantiate(Resources.Load<GameObject>("effect/trapEffect"), cell.transform.parent.position, Quaternion.identity);
            // go.transform.DOPunchScale(Vector3.one, animTime);

            RulePopupManager.Instance.showRule("snakeToTrap");
            FindObjectOfType<AchievementManager>().ShowAchievement("trapped");

            yield return new WaitForSeconds(animTime);
        }
    }
    bool canBeHeated(GridCell cell)
    {
        return cell.GetComponent<GridCell>().cellInfo.isPlayer() || cell.GetComponent<GridCell>().cellInfo.isAlly() || cell.GetComponent<GridCell>().cellInfo.isEnemy() || cell.GetComponent<GridCell>().cellInfo.type == "branch"
            || cell.GetComponent<GridCell>().cellInfo.type == "nut";

    }
    //todo: add ally logic later if needed
    IEnumerator hotCellCalculation(GridCell cell)
    {
        yield return null;
        {
            if (cell && cell.GetComponent<GridCell>().cellInfo.isPlayer())
            {

                RulePopupManager.Instance.showRule("playerOnHot");
                ResourceManager.Instance.consumeResource("nut", 3, cell.transform.position);

                SFXManager.Instance.play("shortburn");

                SFXManager.Instance.play("scream");
                Instantiate(fireVFX, cellParents[cell.index].position, Quaternion.identity);
                yield return new WaitForSeconds(animTime);
            }
            else if (cell && cell.GetComponent<GridCell>().cellInfo.isEnemy() && cell.GetComponent<EnemyCell>().canBeAttacked())
            {
                RulePopupManager.Instance.showRule("enemyOnHot");
                cell.GetComponent<EnemyCell>().getDamage(5);
                SFXManager.Instance.play("shortburn");
                Instantiate(fireVFX, cellParents[cell.index].position, Quaternion.identity);
                yield return new WaitForSeconds(animTime);
            }

            else if (cell && cell.GetComponent<GridCell>().cellInfo.type == "branch")
            {
                //generate a fire
                generateCell(cell.index, "fire");
                SFXManager.Instance.play("shortburn");
                destroy(cell.gameObject);
                Instantiate(fireVFX, cellParents[cell.index].position, Quaternion.identity);
                yield return new WaitForSeconds(animTime);

            }
            else if (cell && cell.GetComponent<GridCell>().cellInfo.type == "nut")
            {

                RulePopupManager.Instance.showRule("fire+nut");
                generateCell(cell.index, "cookedNut");
                SFXManager.Instance.play("shortburn");
                destroy(cell.gameObject);
                Instantiate(fireVFX, cellParents[cell.index].position, Quaternion.identity);
                yield return new WaitForSeconds(animTime);
            }


        }
    }


    public IEnumerator triggerTrapOnCell(int i,EnemyCell cell)
    {
        yield return null;
            var trap = cellParents[i].GetComponentInChildren<GridItem>();
            bool combination = trap != null && (trap.cellInfo.type.Contains("trap") || trap.cellInfo.type.Contains("Trap"));
            if (combination)
            {
                yield return StartCoroutine(trapCellCalculation(cell, trap));
            }
    }

    IEnumerator characterAttack(GridCell characterCell,int index)
    {
        yield return null;
        if (!characterCell)
        {
            yield break;
        }
        if (characterCell.hasEquipment())
        {
            bool attackWithWeapon = false;


            int damage = characterCell.equipementDamage;
            foreach (var cell in getAdjacentCells(index))
            {
                if (cell.cellInfo.isEnemy() && cell.GetComponent<EnemyCell>().canBeAttacked())
                {

                    damage = Mathf.Min(damage, cell.amount);
                    attackWithWeapon = true;
                }
            }

            foreach (var cell in getAdjacentCells(index))
            {
                if (cell.cellInfo.isEnemy() && cell.GetComponent<EnemyCell>().canBeAttacked())
                {

                    cell.GetComponent<EnemyCell>().getDamage(damage);
                    if (characterCell.equipment != null)
                    {

                        SFXManager.Instance.play("hit" + characterCell.equipment);
                    }

                    var go = Instantiate(Resources.Load<GameObject>("effect/attack"), cellParents[characterCell.index].transform.position, Quaternion.identity);
                    go.transform.DOMove(cellParents[cell.index].transform.position, GridController.Instance.animTime + 0.1f);
                    Destroy(go, 1f);


                    var go2 = Instantiate(Resources.Load<GameObject>("effect/attack"), cellParents[characterCell.index].transform.position, Quaternion.identity);
                    go2.transform.DOMove(cellParents[cell.index].transform.position, GridController.Instance.animTime + 0.1f);
                    go2.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("cell/" + characterCell.equipment);
                    go2.GetComponentInChildren<SpriteRenderer>().sortingOrder = 1;
                    Destroy(go2, 1f);

                    FindObjectOfType<AchievementManager>().ShowAchievement("slash2");
                    yield return new WaitForSeconds(animTime);
                }
            }
            if (attackWithWeapon)
            {
                characterCell.attackWithEquipement(damage);
                FindObjectOfType<AchievementManager>().ShowAchievement("slash");
            }
        }
    }

    IEnumerator attackAndMove()
    {
       // yield break;
        //  cellParents[cell.index].GetComponentsInChildren<GridItem>()
        //calculate hot place
        for (int i = 0; i < cellParents.Count; i++)
        {
            foreach(var cell in cellParents[i].GetComponentsInChildren<GridCell>())
            {

                //check if cell is on hot cell
                if (cell && cellParents[cell.index].GetComponent<GridBackground>().isHot && canBeHeated(cell))
                {
                    yield return StartCoroutine(hotCellCalculation(cell));
                }
            }
        }

        //calculate trap
        for (int i = 0; i < cellParents.Count; i++)
        {
            foreach (var cell in cellParents[i].GetComponentsInChildren<GridCell>())
            {
                if(cell != null && cell.cellInfo.isEnemy() && cell.GetComponent<EnemyCell>().canBeAttacked())
                {
                    yield return StartCoroutine( triggerTrapOnCell(i,cell.GetComponent<EnemyCell>()));
                }
            }
        }

        //calculate player attack
        yield return StartCoroutine(characterAttack(allyCell, allyCellIndex));
        yield return StartCoroutine(characterAttack(playerCell, playerCellIndex));

        //calcualte enemy attack

        for (int i = 0; i < cellParents.Count; i++)
        {
            foreach (var cell in cellParents[i].GetComponentsInChildren<GridCell>())
            {
                if (cell.cellInfo.isEnemy() && cell.GetComponent<EnemyCell>().willAttack())
                {

                    yield return StartCoroutine(cell.GetComponent<EnemyCell>().startAttack());
                }
            }
        }
        //calculate enemy move


        for (int i = 0; i < cellParents.Count; i++)
        {
            
            foreach (var cell in cellParents[i].GetComponentsInChildren<GridCell>())
            {
                if (cell.cellInfo.isEnemy() && !cell.isFreezed && cell.GetComponent<EnemyCell>().willMove())
                {

                    yield return StartCoroutine(cell.GetComponent<EnemyCell>().startMove());
                    
                }
                if (cell.cellInfo.isEnemy())
                {

                    cell.GetComponent<EnemyCell>().finishedMove();
                }
            }
        }

        //re calculate fire

        for (int i = 0; i < cellParents.Count; i++)
        {
            foreach (var cell in cellParents[i].GetComponentsInChildren<GridCell>())
            {

                //check if cell is on hot cell
                if (cell && cellParents[cell.index].GetComponent<GridBackground>().isHot && cell.cellInfo.isEnemy())
                {
                    yield return StartCoroutine(hotCellCalculation(cell));
                }
            }
        }

    }

    public string combineResult(GridCell cell)
    {
        var targetCell = emptyCell.transform.parent.GetComponentInChildren<GridItem>();
        var cell2String = targetCell ? targetCell.type : "empty";
        if (cell && cell.cellInfo!=null && cell.cellInfo.isPlayer())
        {
            if (cell2String == "empty")
            {
                return "playerMove";
            }
            else if (cell2String == "nut")
            {
                return "playerToNut";
            }
            else if (cell2String == "axe")
            {
                return "playerEquipaxe";
            }
            else if (cell2String == "bat")
            {
                return "playerEquipbat";
            }else if(targetCell&&cellParents[targetCell.index].GetComponent<GridBackground>().isHot)
            {
                return "playerOnHot";
            }
        }
        else
        {

            if (!cell || cell.cellInfo == null)
            {
                return null;
            }
            var combination = CombinationManager.Instance.getCombinationResult(cell.cellInfo.type, cell2String);



            return combination !=null ? combination.rules:null;
        }
        return null;

    }

    public bool hasEqualOrMoreCardsWithType(string type, int count)
    {
        if(count == 0)
        {
            return false;
        }
        GridCell[] cells = GameObject.FindObjectsOfType<GridCell>();
        int t = 0;
        foreach(GridCell c in cells)
        {
            if(c.cellInfo!=null && c.cellInfo.type== type)
            {
                t++;
            }
        }
        return t >= count;
    }

    IEnumerator moveOthers()
    {


        yield return new WaitForSeconds(GridController.Instance.animTime);

        yield return StartCoroutine(attackAndMove());



        if (boss)
        {

            yield return StartCoroutine(boss.onNextStep());

        }

        finishMove();
        step();
        EventPool.Trigger("moveAStep");
        yield break;
    }
    
    IEnumerator exploreCellAnim(GridCell cell)
    {
        yield return null;
        if (!cell.cellInfo.isEmpty())
        {
            cell.failedToMove();
            isMoving = false;
            yield break;
        }
            //draw card
            string card = "";

        if (boss)
        {

            card = DeckManager.Instance.drawBossCard();
        }
        else
        {
            //draw a card
            card = DeckManager.Instance.drawCard(cell.cellInfo.isEmpty());


        }

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
                foreach (var c in GameObject.FindObjectsOfType<GridCell>())
                {
                    if (c.cellInfo.type != "fire" && !isAdjacentToFire(c.index) && !c.GetComponent<GridItem>() && !c.GetComponent<GridBackground>() && !c.GetComponent<GridEmpty>())
                    {
                        canFreezeCells.Add(c);
                    }
                }

                if (canFreezeCells.Count > 0)
                {
                    var freeCell = Utils.randomList(canFreezeCells);
                    freeCell.freeze();

                    SFXManager.Instance.play("iceshowup");
                }
            }
        }
        else if (GameObject.FindObjectsOfType<GridCell>().Length > 0)
        {
            //ice spread

            if (ShopManager.Instance.hasPurchased("fire") && Random.Range(0, 2) > 0)
            {
            }
            else
            {
                foreach (var c in GameObject.FindObjectsOfType<GridCell>())
                {
                    if (!c.GetComponent<GridItem>() && !c.GetComponent<ShopCell>() && c.cellInfo.type != "fire" && !isAdjacentToFire(c.index) && !c.isFreezed && isAdjacentToIce(c.index) && !c.GetComponent<GridItem>() && !c.GetComponent<GridBackground>() && !c.GetComponent<GridEmpty>())
                    {
                        c.freeze();

                        SFXManager.Instance.play("icespread");
                        RulePopupManager.Instance.showRule("iceSpread");
                        //if freezed everything, game over

                        freezedCellCount = freezeCount();
                        if (freezedCellCount >= 8)
                        {

                            GameManager.Instance.gameover();
                            AchievementManager.Instance.clear("freezeToDeath");
                        }
                        break;
                    }
                }

            }
        }

        // if it is cell card, don't move it, but destroy and replace it to the cell card
        // empty position not change.
        if (cardInfo.isBoss())
        {
            // if it is boss, just create boss and do  nothing

            var go2 = Instantiate(Resources.Load<GameObject>("boss/" + cardInfo.type), bossParent.position, Quaternion.identity, bossParent);
            go2.GetComponent<Boss>().init(cardInfo.type);


            go2.transform.DOPunchScale(Vector3.one, animTime);

            yield return new WaitForSeconds(GridController.Instance.animTime);
            finishMove();
            step();
            EventPool.Trigger("moveAStep");


            DeckManager.Instance.removeAllCardFromDeck(card);

            yield break;
        }

        if (card == "empty")
        {

            yield return StartCoroutine(exchangeCard(cell, emptyCellIndex));
        }
        else
        {
            Debug.Log("generate " + card);
            //generate a snake
            var go = generateCell(cell.index, card);
            destroy(cell.gameObject);


            if (cardInfo.isAlly())
            {
                if (allyCell)
                {
                    Debug.LogError("does not support multiple ally now");
                }
                else
                {
                    allyCell = go.GetComponent<GridCell>();
                }
            }

            //destory what's underground when spawn enemy.. comment for now.
            if (go.GetComponent<GridCell>().cellInfo.isEnemy())
            {
                //foreach (var item in cellParents[cell.index].GetComponentsInChildren<GridItem>())
                //{
                //    Destroy(item.gameObject);
                //}
            }
            else
            {
                SFXManager.Instance.play("showup");
            }
        }


        yield return  StartCoroutine(moveOthers());

    }
    IEnumerator moveCellAnim(GridCell cell, bool forceMove)
    {
        TextWhenShowCell.Instance.hideText();
       yield return null;
        AchievementManager.Instance.clear("round");
        //if is moving player, consume
        if(!cell || cell.cellInfo == null)
        {
            Debug.LogError("???");
        }
        if (cell.cellInfo.isPlayer())
        {
            ResourceManager.Instance.consumeResource("nut", 1, cell.transform.position);
            RulePopupManager.Instance.showRule("playerMove");
        }
        if (cell.cellInfo.isAlly())
        {
            ResourceManager.Instance.consumeResource("nut", 1, cell.transform.position);
            //RulePopupManager.Instance.showRule("playerMove");
        }



        //move current cell to position
        var originEmptyIndex = emptyCellIndex;
        var originalMovingCellIndex = cell.index;



        var emptyPosition = cellParents[originEmptyIndex].position;


        bool willGetIntoShop = false;

        var targetCell = cellParents[originEmptyIndex].GetComponentInChildren<GridItem>();
        var cell1String = cell.type;
        var cell2String = targetCell ? targetCell.type : "empty";

        if (cell.GetComponent<GridCell>().cellInfo.isAlly())
        {
            yield return StartCoroutine(exchangeCard(cell, emptyCellIndex));

            SFXManager.Instance.play("squirrelmove");
            if (targetCell)
            {
                //if (targetCell.cellInfo.isResource())
                //{

                //    SFXManager.Instance.play("collect" + targetCell.cellInfo.categoryDetail);
                //    var resource = new List<PairInfo<int>>() { };
                //    resource.Add(new PairInfo<int>(targetCell.cellInfo.categoryDetail, targetCell.amount));
                //    CollectionManager.Instance.AddCoins(targetCell.transform.position, resource);
                //    destroy(targetCell.gameObject);

                //    switch (targetCell.type)
                //    {
                //        case "nut":

                //            RulePopupManager.Instance.showRule("playerToNut");
                //            break;
                //        case "cookedNut":
                //            RulePopupManager.Instance.showRule("eatHotNut");
                //            break;
                //    }

                //}
                //else 
                if (targetCell.cellInfo.isWeapon())
                {
                    cell.GetComponent<GridCell>().equip(cell2String, targetCell.cellInfo.categoryValue);

                    RulePopupManager.Instance.showRule("playerEquip" + cell2String);
                    destroy(targetCell.gameObject);
                }
                //else if (targetCell.cellInfo.type == "shop")
                //{

                //    destroy(targetCell.gameObject);
                //    StartCoroutine(getIntoShop());
                //    willGetIntoShop = true;
                //}
            }
        }
        else if (cell.GetComponent<GridCell>().cellInfo.isPlayer())
        {
            yield return StartCoroutine(exchangeCard(cell, emptyCellIndex));

            AchievementManager.Instance.clear("move");
            hasPlayerMoved = true;

            SFXManager.Instance.play("squirrelmove");
            if (targetCell)
            {
                if (targetCell.cellInfo.isResource())
                {

                    SFXManager.Instance.play("collect" + targetCell.cellInfo.categoryDetail);
                    var resource = new List<PairInfo<int>>() { };
                    resource.Add(new PairInfo<int>(targetCell.cellInfo.categoryDetail, targetCell.amount));
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
                    cell.GetComponent<GridCell>().equip(cell2String,targetCell.cellInfo.categoryValue);

                    RulePopupManager.Instance.showRule("playerEquip" + cell2String);
                    destroy(targetCell.gameObject);
                }
                else if (targetCell.cellInfo.type == "shop")
                {

                    destroy(targetCell.gameObject);
                    yield return StartCoroutine(getIntoShop());
                    willGetIntoShop = true;
                }
            }
        }
        else if (forceMove)
        {
            yield return StartCoroutine(exchangeCard(cell, emptyCellIndex));
        }
        else
        {
            hasPlayerMoved = false;
            //calculate combination result
            var combination = CombinationManager.Instance.getCombinationResult(cell1String, cell2String);
            if (combination != null)
            {

                yield return StartCoroutine(exchangeCard(cell, emptyCellIndex));
                if (combination.rules!=null && combination.rules.Length > 0)
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
                        //case "destroy1":
                        //    cell.decreaseAmount();
                        //    if (cell.amount == 0)
                        //    {

                        //        addEmpty(originEmptyIndex);
                        //        destroy(cell.gameObject);
                        //    }
                        //    break;
                        case "destroy2":
                            destroy(targetCell.gameObject);
                            break;
                        case "addHot":
                            break;
                        case "generate1":
                            cell.decreaseAmount();
                            if (cell.amount == 0)
                            {

                                //generate new item in target position, generate empty in origin position
                                addEmpty(originalMovingCellIndex);
                            }
                            else
                            {
                                generateCell(originalMovingCellIndex, cell.type, cell.amount);
                            }
                            destroy(cell.gameObject);
                            if (targetCell && targetCell.type == pair.Value)
                            {
                                targetCell.addAmount();
                            }
                            else
                            {
                                generateCell(originEmptyIndex, pair.Value);
                            }

                            moveCard(emptyCell, originEmptyIndex);

                            SFXManager.Instance.play("showup");
                            break;
                        case "generate2":
                            //generate new item in original position
                            //addEmpty(originEmptyIndex);
                            //destroy(cell.gameObject);

                            if (cellParents[originalMovingCellIndex].GetComponentInChildren<GridItem>())
                            {
                                destroy( cellParents[originalMovingCellIndex].GetComponentInChildren<GridItem>().gameObject);
                            }

                            generateCell(originalMovingCellIndex, pair.Value);

                            SFXManager.Instance.play("showup");
                            break;
                        case "increaseObjectHP":
                            cell.GetComponent<FireCell>().addHp(int.Parse(pair.Value));
                            break;
                        case "trap":

                            break;
                        default:
                            Debug.LogError("not support combination restul " + pair.Key);
                            break;
                    }
                }
            }
            else
            {

               // if (canDrawCard) {
                    //generate cell, if already a cell, don't generate and add it back to the deck.
                    if (cell.cellInfo.isEmpty())
                    {


                        yield return StartCoroutine(exploreCellAnim(cell));

                    yield break;

                }
                    else
                    {
                        yield return StartCoroutine(exchangeCard(cell, emptyCellIndex));
                        // DeckManager.Instance.waitingCards(card);
                    }
                //}
               // else
               // {
               //     yield return StartCoroutine(exchangeCard(cell, emptyCellIndex));
               // }



                //cell.transform.DOMove(emptyPosition, animTime);
                // generate(originalMovingCellIndex, card);
                // yield return new WaitForSeconds(animTime);






                //StartCoroutine(moveOthers());
                // yield break;
            }

            // if generate new item on new position, don't update emptyCell
            //if (combination != null && combination.result.ContainsKey("generate1"))
            //{

            //}
            //else
            //{
            //    emptyCell = movingCellIndex;

            //}


        }



        if (cell.cellInfo.type == "fire")
        {
            cell.GetComponent<FireCell>().getDamage(1);

            cellParents[originalMovingCellIndex].GetComponent<GridBackground>().heat();
            Instantiate(fireVFX, cellParents[originalMovingCellIndex].position, Quaternion.identity);

            RulePopupManager.Instance.showRule("fireMove");
        }


        //yield return new WaitForSeconds(animTime);

        //if (boss)
        //{

        //    yield return StartCoroutine(boss.onNextStep());
        //}

        //if (!willGetIntoShop)
        //{

        //    yield return StartCoroutine(attackAndMove());
        //    step();
        //    EventPool.Trigger("moveAStep");
        //}




        yield return StartCoroutine(moveOthers());
       //// yield break;
       // finishMove();




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
        int py = playerCellIndex % cellCountY;
        int newx = x;
        int newy = y;
        if(x== px)
        {
            newy = (int)(Mathf.Sign(py - y)) + y;
        }
        else
        {

            newx = (int)(Mathf.Sign(px - x)) + x;
        }
        int res = (newx * cellCountX + newy);
        Debug.Log("get target " + x+" "+ y+" "+ px+" "+ py+" "+newx+" "+newy+" "+ res);


        if (res < 0 || res > 8)
        {
            Debug.LogError("res wrong to find target index to position");
        }

        return res;
    }

    void moveCard(GridCell cell, int index)
    {

        SFXManager.Instance.play("cardmove");
        cell.index = index;
        cell.transform.parent = cellParents[index];

        var cellPosition = cellParents[index].position;
        cell.transform.position = cellPosition;
    }

    public IEnumerator exchangeCard(GridCell cell1, int cell2Index)
    {
        SFXManager.Instance.play("cardmove");
        var originalIsMoving = isMoving;
        isMoving = true;
        if (cell1.index == cell2Index)
        {
            Debug.Log("that's not correct "+ cell2Index);
        }

        var cell1Index = cell1.index;
        var cell2s = cellParents[cell2Index].GetComponentsInChildren<GridCell>();
        GridCell cell2 = null;
        foreach(var c in cell2s)
        {
            if (c.GetComponent<GridItem>())
            {
                continue;
            }
            if (c.GetComponent<EnemyCell>() && c.GetComponent<EnemyCell>().isDead)
            {
                continue;
            }
            cell2 = c;
        }
        if(cell2 == null)
        {
            Debug.Log("cell2 is null");
        }
        var cell1Position = cellParents[cell1Index].position;
        var cell2Position = cellParents[cell2Index].position;
        //cell.GetComponent<SortingGroup>().sortingOrder = 100;
        cell1.transform.DOMove(cell2Position, animTime);
        cell2.transform.DOMove(cell1Position, animTime);
        cell1.index = cell2Index;
        cell2.index = cell1Index;

        cell1.transform.parent = cellParents[cell1.index];
        cell2.transform.parent = cellParents[cell2.index];
        Debug.Log("cell1 " + cell2Index + " cell2 " + cell1Index);
        yield return new WaitForSeconds(animTime);
        if(originalIsMoving == false)
        {

            isMoving = false;
        }
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


            SFXManager.Instance.play("cardshow");
        }

        yield return new WaitForSeconds(animTime);

        isMoving = false;
        EventPool.Trigger("moveAStep");
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

            SFXManager.Instance.play("cardgone");
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
