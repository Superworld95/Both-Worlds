
using UnityEngine;

[CreateAssetMenu(fileName = "AttackBoostPowerup", menuName = "Scriptable Objects/AttackBoostPowerup")]
public class AttackBoostPowerup : Powerup
{
    public float attackMultiplier = 2f;

    public override void Apply(IPlayerController player)
    {
        player.SetAttackBoost(attackMultiplier, duration);
        Debug.Log($"{powerupName} applied: attack x{attackMultiplier} for {duration}s.");
    }
}
