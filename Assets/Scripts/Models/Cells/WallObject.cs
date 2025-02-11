using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallObject : CellObject
{
    public List<Tile> wallTiles;
    public int wallHp;
    public int wallTileIndex;
    public FoodObject foodObject;
    public float dropChance;

    private Tile _originalTile;
    public override void Init(Vector2Int cell)
    {
        base.Init(cell);
        _originalTile = GameManager.Instance.boardManager.GetCellTile(cell);
        wallHp = 3;
        wallTileIndex = wallHp - 1;
        GameManager.Instance.boardManager.SetCellTile(cell, wallTiles[wallTileIndex]);
    }

    public override bool PlayerTryToEnter(Vector2Int cell)
    {
        base.PlayerTryToEnter(cell);
        var wallCell = GameManager.Instance.boardManager.GetCellData(cell);
        return wallCell.passable;
    }

    public override void DamageCell(Vector2Int cell)
    {
        wallHp -= 1;

        if (wallHp <= 0)
        {
            var tile = GetOriginalTile();
            GameManager.Instance.boardManager.SetCellTile(cell, tile);
            Destroy(gameObject);
            var wallObject = GameManager.Instance.boardManager.GetCellData(cell);
            wallObject.passable = true;
            wallObject.breakable = false;
            EnemyDrop(foodObject, dropChance, cell);
            return;
        }

        wallTileIndex -= 1;
        GameManager.Instance.boardManager.SetCellTile(cell, wallTiles[wallTileIndex]);
    }

    public Tile GetOriginalTile()
    {
        return _originalTile;
    }

    private void EnemyDrop<T>(T cellObject, float dropChance, Vector2Int cellCoord) where T : CellObject
    {
        var randomFloat = Random.value;
        if (randomFloat < dropChance)
        {
            var newObject = Instantiate(cellObject);
            newObject.Init(cellCoord);
            newObject.transform.position = GameManager.Instance.boardManager.CellToWorld(cellCoord);
            var data = GameManager.Instance.boardManager.GetCellData(cellCoord);
            data.containedObject = newObject;
            data.passable = true;
            data.breakable = false;
        }
    }
}