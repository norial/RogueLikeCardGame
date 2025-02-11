using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DropItem
{
    public CellObject dropObject;
    [Range(0f, 1f)] public float dropChance;
}

public class EnemyModel : CellObject
{
    public int enemyHp;
    public DropItem[] dropItems;

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    public override void Init(Vector2Int cell)
    {
        var ListOfObject = new List<CellObject>();
        base.Init(cell);
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        GameManager.Instance.TurnManager.OnTick += TurnHappened;
    }

    public void OnDestroy()
    {
        GameManager.Instance.TurnManager.OnTick -= TurnHappened;
    }

    public override void DamageCell(Vector2Int cell)
    {
        base.DamageCell(cell);
        var celldata = GameManager.Instance.boardManager.GetCellData(cell);
        enemyHp -= 1;
        _animator.Play("TakingDamageAnimation");

        if (enemyHp <= 0)
        {
            Destroy(gameObject);
            EnemyDrop(cell);
            celldata.breakable = false;
            celldata.passable = true;
        }
    }
    public override bool PlayerTryToEnter(Vector2Int cell)
    {
        base.PlayerTryToEnter(cell);
        var enemyObject = GameManager.Instance.boardManager.GetCellData(cell);
        return enemyObject.passable;

    }

    private void EnemyDrop(Vector2Int cellCoord)
    {
        foreach (var drop in dropItems)
        {
            float randomFloat = UnityEngine.Random.value;

            if (randomFloat < drop.dropChance)
            {
                var newObject = Instantiate(drop.dropObject);
                newObject.Init(cellCoord);
                newObject.transform.position = GameManager.Instance.boardManager.CellToWorld(cellCoord);

                var data = GameManager.Instance.boardManager.GetCellData(cellCoord);
                data.containedObject = newObject;
                data.passable = true;
                data.breakable = false;
                break;
            }
        }
    }

    IEnumerator<bool> MoveTo(Vector2Int coord)
    {
        var board = GameManager.Instance.boardManager;
        var targetCell = board.GetCellData(coord);

        if (targetCell == null
            || !targetCell.passable
            || targetCell.containedObject != null)
        {
            yield return false;
        }

        var startPos = transform.position;
        var targetPos = GameManager.Instance.boardManager.CellToWorld(coord);
        float duration = 0.1f;
        float elapsed = 0f;

        Rotate(coord.x);



        var currentCell = board.GetCellData(_cell);
        currentCell.containedObject = null;
        currentCell.breakable = false;
        currentCell.passable = true;

        targetCell.containedObject = this;
        targetCell.passable = false;
        targetCell.breakable = true;
        _cell = coord;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return true;
        }

        yield return true;
    }

    private void Rotate(int cellPositionX)
    {
        if (cellPositionX == _cell.x + Vector2.left.x)
        {
            _spriteRenderer.flipX = false;
        }
        else if (cellPositionX == _cell.x + Vector2.right.x)
        {
            _spriteRenderer.flipX = true;
        }
    }

    void TurnHappened()
    {
        var playerCell = GameManager.Instance.playerController.GetPlayerPosition();

        int xDist = playerCell.x - _cell.x;
        int yDist = playerCell.y - _cell.y;

        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);

        if ((xDist == 0 && absYDist == 1)
            || (yDist == 0 && absXDist == 1))
        {
            GameManager.Instance.ChangeFood(-3);
            _animator.Play("AttackAnimation");
            GameManager.Instance.playerController.PlayAnimation("TakeDamageAnimation");
        }
        else
        {
            if (absXDist > absYDist)
            {
                if (!TryMoveInX(xDist))
                {
                    TryMoveInY(yDist);
                }
            }
            else
            {
                if (!TryMoveInY(yDist))
                {
                    TryMoveInX(xDist);
                }
            }
        }
    }

    bool TryMoveInX(int xDist)
    {
        Vector2Int newPos = xDist > 0 ? _cell + Vector2Int.right : _cell + Vector2Int.left;
        return TryStartMove(newPos);
    }

    bool TryMoveInY(int yDist)
    {
        Vector2Int newPos = yDist > 0 ? _cell + Vector2Int.up : _cell + Vector2Int.down;
        return TryStartMove(newPos);
    }

    bool TryStartMove(Vector2Int newPos)
    {
        var board = GameManager.Instance.boardManager;
        var targetCell = board.GetCellData(newPos);

        if (targetCell != null && targetCell.passable && targetCell.containedObject == null)
        {
            StartCoroutine(MoveTo(newPos));
            return true;
        }
        return false;
    }
}