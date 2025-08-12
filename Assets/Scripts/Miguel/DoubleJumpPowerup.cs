using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DoubleJumpPowerup", menuName = "Scriptable Objects/DoubleJumpPowerup")]
public class DoubleJumpPowerup : Powerup
{
    public override void Apply(IPlayerController player)
    {
        player.EnableDoubleJump(true);
        Debug.Log($"{powerupName} applied: double jump enabled for {duration}s.");

        MonoBehaviour mb = player as MonoBehaviour;
        if (mb != null)
        {
            mb.StartCoroutine(DisableAfterDuration(player));
        }
        else
        {
            Debug.LogWarning("Player does not inherit MonoBehaviour, coroutine cannot be started.");
        }
    }

    private IEnumerator DisableAfterDuration(IPlayerController player)
    {
        yield return new WaitForSeconds(duration);
        player.EnableDoubleJump(false);
        Debug.Log($"{powerupName} expired: double jump disabled.");
    }
}
