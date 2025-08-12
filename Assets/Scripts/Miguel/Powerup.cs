using UnityEngine;

/// <summary>
/// Base abstract Powerup ScriptableObject.
/// Each powerup defines its own Apply method for flexible behavior.
/// </summary>
[CreateAssetMenu(fileName = "Powerup", menuName = "Scriptable Objects/Powerup")]
public abstract class Powerup : ScriptableObject
{
    public string powerupName = "New Powerup";
    public Sprite icon;
    public float duration = 10f; // Duration in seconds (0 for instant effect)

    /// <summary>
    /// Apply the powerup effect to the target player controller.
    /// </summary>
   
    public virtual void Collect(IPlayerController player)
    {
        Apply(player);
        Debug.Log($"{powerupName} collected by {player.GetPlayerName()}");
    }

    public abstract void Apply(IPlayerController player);
}
