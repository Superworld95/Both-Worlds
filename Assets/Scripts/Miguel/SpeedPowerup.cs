using UnityEngine;

[CreateAssetMenu(fileName = "SpeedPowerup", menuName = "Scriptable Objects/SpeedPowerup")]
public class SpeedPowerup : Powerup
{
    public float speedMultiplier = 1.5f;

    public override void Apply(IPlayerController player)
    {
        player.SetSpeedBoost(speedMultiplier, duration);
        Debug.Log($"{powerupName} applied: speed x{speedMultiplier} for {duration}s.");
    }
}


