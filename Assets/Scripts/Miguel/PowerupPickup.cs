using UnityEngine;

public class PowerupPickup : MonoBehaviour
{
    public Powerup powerup;

    private void OnTriggerEnter2D(Collider2D other)
    {
        IPlayerController player = other.GetComponent<IPlayerController>();
        if (player != null && powerup != null)
        {
            powerup.Apply(player);
            Destroy(gameObject); //Removes the item from scene
        }
    }
}
