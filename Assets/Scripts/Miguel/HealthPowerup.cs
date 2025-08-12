using UnityEngine;

[CreateAssetMenu(fileName = "HealthPowerup", menuName = "Scriptable Objects/HealthPowerup")]
public class HealthPowerup : Powerup
{
    public int healAmount = 25;

    public override void Apply(IPlayerController player)
    {
        player.Heal(healAmount);
        Debug.Log($"{powerupName} applied: healed {healAmount} HP.");
    }
}



