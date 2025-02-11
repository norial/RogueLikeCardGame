using Unity;
using UnityEngine;

public class TurnManager
{
    private int _turnCount;
    public event System.Action OnTick;

    public TurnManager()
    {
        _turnCount = 0;
    }

    public void Tick()
    {
        OnTick?.Invoke();
        _turnCount += 1;
        Debug.Log($"It's {_turnCount} turn");
    }
}