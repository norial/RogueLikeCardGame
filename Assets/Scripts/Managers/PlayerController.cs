using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public GameObject moveIndicatorPrefab;
    public GameObject breakeIndicatorPrefab;
    public PlayerStateEnum playerState;
    public AudioClip[] movementAudioClip;
    public AudioClip[] attackAudioClip;
    public bool hasHalmet;
    public bool hasBoots;

    private bool _canMove;
    private BoardManager _boardManager;
    private Vector2Int _cellPosition;
    private List<GameObject> _moveIndicators = new List<GameObject>();
    private List<GameObject> _breakIndicators = new List<GameObject>();
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private void Update()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerState == PlayerStateEnum.GameStarted)
        {
            StartCoroutine("CheckMovement");
        }
        else
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }

            return;
        }
    }

    private IEnumerator CheckMovement()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && _moveIndicators.Count == 0)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2Int clickedCell = _boardManager.WorldToCell(mouseWorldPos);

            if (clickedCell == _cellPosition)
            {
                ShowMoveOptions();
            }
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            GameManager.Instance.TurnManager.Tick();
        }

        if (Mouse.current.rightButton.wasPressedThisFrame && _canMove)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2Int clickedCell = _boardManager.WorldToCell(mouseWorldPos);

            if (_moveIndicators.Exists(indicator => _boardManager.WorldToCell(indicator.transform.position) == clickedCell))
            {
                var selectedCell = _boardManager.GetCellData(clickedCell);
                GameManager.Instance.TurnManager.Tick();
                StartCoroutine("MoveTo", clickedCell);

                if (selectedCell.containedObject != null)
                {
                    selectedCell.containedObject.PlayerEntered();
                }
            }
            else if (_breakIndicators.Exists(indicator => _boardManager.WorldToCell(indicator.transform.position) == clickedCell))
            {
                var selectedCell = _boardManager.GetCellData(clickedCell);

                var canEnter = selectedCell.containedObject.PlayerTryToEnter(clickedCell);
                if (!canEnter)
                {
                    DamageCell(selectedCell, clickedCell);
                }
                GameManager.Instance.TurnManager.Tick();
            }
        }
        yield return null;
    }

    private void DamageCell(CellData selectedCell, Vector2Int clickedCell)
    {
        selectedCell.containedObject.DamageCell(clickedCell);

        RotatePlayer(clickedCell.x);

        _animator.Play("AttackAnimation");

        var randomAttackAudioIndex = UnityEngine.Random.Range(0, attackAudioClip.Length);

        SoundManager.Instance.PlaySound(attackAudioClip[randomAttackAudioIndex], transform);

        ClearInidcators();
    }

    public void PlayAnimation(string animationName)
    {
        _animator.Play(animationName);
    }

    private void RotatePlayer(int cellPositionX)
    {
        if (cellPositionX == _cellPosition.x + Vector2.left.x)
        {
            _spriteRenderer.flipX = true;
        }
        else if (cellPositionX == _cellPosition.x + Vector2.right.x)
        {
            _spriteRenderer.flipX = false;
        }
    }

    public void Spawn(BoardManager boardManager, Vector2Int cellPosition)
    {
        StopAllCoroutines();
        ClearInidcators();
        _boardManager = boardManager;
        _cellPosition = cellPosition;

        transform.position = _boardManager.CellToWorld(_cellPosition);
    }

    private void ShowMoveOptions()
    {
        ClearInidcators();
        Vector2Int[] directions;

        if (!hasBoots)
        {
            directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        }
        else
        {
            directions = new Vector2Int[]
            {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new(-1, -1), new(-1, 1), new(1, -1), new(1, 1),
            new(0, 2), new(0, -2), new(2, 0), new(-2, 0)
            };
        }

        foreach (var dir in directions)
        {
            Vector2Int newPos = _cellPosition + dir;

            if (!CanMoveToCell(_cellPosition, newPos))
                continue;

            if (_boardManager.IsCellPassable(newPos))
            {
                GameObject indicator = Instantiate(moveIndicatorPrefab, _boardManager.CellToWorld(newPos), Quaternion.identity);
                _moveIndicators.Add(indicator);
            }

            if (_boardManager.IsCellBreakable(newPos))
            {
                GameObject indicator = Instantiate(breakeIndicatorPrefab, _boardManager.CellToWorld(newPos), Quaternion.identity);
                _breakIndicators.Add(indicator);
            }
        }

        _canMove = true;
    }

    public bool CanMoveToCell(Vector2Int start, Vector2Int end)
    {
        Vector2Int direction = NormalizeDirection(end - start);
        Vector2Int current = start;

        while (current != end)
        {
            current += direction;

            if (!_boardManager.IsCellPassable(current))
            {
                Vector2Int nextCell = current + direction;
                if (nextCell == end)
                {
                    if ((_boardManager.IsCellPassable(nextCell) && !_boardManager.IsCellBreakable(nextCell)))
                    {
                        Debug.Log($"Путь заблокирован на {nextCell}");
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public Vector2Int NormalizeDirection(Vector2Int vector)
    {
        if (vector == Vector2Int.zero) return Vector2Int.zero;

        int x = vector.x == 0 ? 0 : (vector.x > 0 ? 1 : -1);
        int y = vector.y == 0 ? 0 : (vector.y > 0 ? 1 : -1);

        return new Vector2Int(x, y);
    }

    private void ClearInidcators()
    {
        foreach (var indicator in _moveIndicators)
        {
            Destroy(indicator);
        }

        foreach (var indicator in _breakIndicators)
        {
            Destroy(indicator);
        }

        _moveIndicators.Clear();
        _breakIndicators.Clear();
    }

    private IEnumerator MoveTo(Vector2Int newPosition)
    {
        var startPos = transform.position;
        var targetPos = _boardManager.CellToWorld(newPosition);
        float duration = 0.1f;
        float elapsed = 0f;

        RotatePlayer(newPosition.x);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        transform.position = targetPos;
        _cellPosition = newPosition;
        ClearInidcators();
        _canMove = false;

        var movementAudio = UnityEngine.Random.Range(0, movementAudioClip.Length);
        SoundManager.Instance.PlaySound(movementAudioClip[movementAudio], transform);

        yield return null;
    }

    public Vector2Int GetPlayerPosition()
    {
        return _cellPosition;
    }

    public void GameOver()
    {
        playerState = PlayerStateEnum.GameOver;
    }
}
