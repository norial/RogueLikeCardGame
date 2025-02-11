using UnityEngine;
using UnityEngine.Tilemaps;

public class ExitObject : CellObject
{
    public Tile exitTile;
    public AudioClip exitAudioClip;

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);
        GameManager.Instance.boardManager.SetCellTile(cell, exitTile);
    }

    public override void PlayerEntered()
    {
        Debug.Log("player reached exit");

        SoundManager.Instance.PlaySound(exitAudioClip, transform);

        GameManager.Instance.InitializeLevel();
    }
}