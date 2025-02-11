using UnityEngine;
using UnityEngine.Tilemaps;

public class CellObject : MonoBehaviour
{
    protected Vector2Int _cell;

    public virtual void Init(Vector2Int cell)
    {
        _cell = cell;
    }

    public virtual void PlayerEntered()
    {
    }

    public virtual void DamageCell(Vector2Int cell)
    {

    }
    
    public virtual bool PlayerTryToEnter(Vector2Int Cell)
    {
        return true;
    }
}
