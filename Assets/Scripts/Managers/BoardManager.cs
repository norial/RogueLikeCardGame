using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public int width;
    public int height;
    public Tile[] groundTiles;
    public Tile[] wallTiles;
    public PlayerController playerController;
    public GameObject moveIndicatorPrefab;
    public List<FoodObject> foodPrefabs;
    public List<WallObject> wallPrefabs;
    public List<EnemyModel> enemyPrefabs;
    public ExitObject exitPrefab;
    public int enemiesAmount = 2;

    private Tilemap _tileMap;
    private CellData[,] _boardData;
    private List<Vector2Int> _emptyCells;
    private Grid _grid;

    public void Init()
    {
        _tileMap = GetComponentInChildren<Tilemap>();
        _grid = GetComponentInChildren<Grid>();
        _boardData = new CellData[width, height];
        _emptyCells = new List<Vector2Int>();
        int tileNumber;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                _boardData[x, y] = new CellData();

                if (x == 0 || y == 0 || y == height - 1 || x == width - 1)
                {
                    tileNumber = Random.Range(0, wallTiles.Length);
                    _tileMap.SetTile(new Vector3Int(x, y, 0), wallTiles[tileNumber]);
                    _boardData[x, y].passable = false;
                }
                else
                {
                    tileNumber = Random.Range(0, groundTiles.Length);
                    _tileMap.SetTile(new Vector3Int(x, y, 0), groundTiles[tileNumber]);
                    _boardData[x, y].passable = true;
                    _emptyCells.Add(new Vector2Int(x, y));
                }
            }
        }
        SetExitOnWorld(exitPrefab);

        _emptyCells.Remove(new Vector2Int(1, 1));
        SetObjectsOnWorld(wallPrefabs, false, true, 11, 25);
        SetObjectsOnWorld(foodPrefabs, true, false, 5, 14);
        SetObjectsOnWorld(enemyPrefabs, false, true, 1, enemiesAmount);
    }

    public void Clean()
    {
        if (_boardData == null)
            return;


        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                var cellData = _boardData[x, y];

                if (cellData.containedObject != null)
                {
                    Destroy(cellData.containedObject.gameObject);
                }

                SetCellTile(new Vector2Int(x, y), null);
            }
        }
    }

    private void SetObjectsOnWorld<T>(List<T> objectPrefab, bool passable, bool breakable, int minAmountOfObjects = 1, int maxAmountOfObjects = 2) where T : CellObject
    {
        var ObjectAmount = Random.Range(minAmountOfObjects, maxAmountOfObjects);

        for (var i = 0; i < ObjectAmount; i++)
        {
            var randomIndex = Random.Range(0, _emptyCells.Count);
            var randomObjectIndex = Random.Range(0, objectPrefab.Count);
            var cellCoord = _emptyCells[randomIndex];

            _emptyCells.RemoveAt(randomIndex);
            CellData data = _boardData[cellCoord.x, cellCoord.y];
            PrepareCellData(data, objectPrefab[randomObjectIndex], cellCoord, passable, breakable);
        }
    }

    private void SetExitOnWorld(ExitObject exitPrefab)
    {
        var xPos = width - 2;
        var yPos = height - 2;
        var cellCoord = new Vector2Int(xPos, yPos);

        _emptyCells.Remove(cellCoord);
        CellData data = _boardData[cellCoord.x, cellCoord.y];
        PrepareCellData(data, exitPrefab, cellCoord, true, false);
    }

    private void PrepareCellData<T>(CellData data, T gameObject, Vector2Int cellCoord, bool passable, bool breakable) where T : CellObject
    {
        var newObject = Instantiate(gameObject);
        newObject.Init(cellCoord);
        newObject.transform.position = CellToWorld(cellCoord);
        data.containedObject = newObject;
        data.passable = passable;
        data.breakable = breakable;
    }

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return _grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    public Vector2Int WorldToCell(Vector2 worldPosition)
    {
        return (Vector2Int)_grid.WorldToCell(worldPosition);
    }

    public bool IsCellPassable(Vector2Int cell)
    {
        if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
        {
            Debug.LogWarning($"Cell {cell} is out of bounds!");
            return false; 
        }

        return _boardData[cell.x, cell.y].passable; 
    }
    public bool IsCellBreakable(Vector2Int cell)
    {
        if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
        {
            Debug.LogWarning($"Cell {cell} is out of bounds!");
            return false;
        }

        return _boardData[cell.x, cell.y].breakable;
    }

    public CellData GetCellData(Vector2Int cellCoord)
    {
        return _boardData[cellCoord.x, cellCoord.y];
    }

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        _tileMap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return _tileMap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }
}
