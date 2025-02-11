using JetBrains.Annotations;

public class DropObject : CellObject
{
    public bool isBoots;
    public bool isHelmet;
    public override void PlayerEntered()
    {
        if (isBoots)
        {
            GameManager.Instance.playerController.hasBoots = true;
            GameManager.Instance.AddHealth(10);

            Destroy(gameObject);
        }
        else
        {
            GameManager.Instance.playerController.hasHalmet = true;
            GameManager.Instance.AddHealth(10);
            Destroy(gameObject);
        }
    }
}