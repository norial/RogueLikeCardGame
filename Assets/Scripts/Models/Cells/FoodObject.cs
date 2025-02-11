using UnityEngine;

public class FoodObject : CellObject
{
    public float amountOfFood;
    public AudioClip foodSound;
    public override void PlayerEntered()
    {
        GameManager.Instance.FoodPicked(amountOfFood);
        GameManager.Instance.UpdateFoodBar();
        SoundManager.Instance.PlaySound(foodSound, transform);
        Destroy(gameObject);
    }
}