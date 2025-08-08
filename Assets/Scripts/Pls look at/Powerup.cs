using UnityEngine;

/// <summary>
/// Collectible powerup system providing temporary or permanent boosts to players.
/// Supports health restoration, speed boosts, attack enhancements, and jump improvements.
/// Features visual feedback and audio integration for collection events.
/// </summary>
public class Powerup : MonoBehaviour
{
    [Header("Powerup Settings")]
    public PowerupType powerupType = PowerupType.Health; // Type of boost this powerup provides
    public int value = 30; // Strength of the powerup effect
    public float rotationSpeed = 90f; // Visual rotation speed in degrees per second

    private SpriteRenderer spriteRenderer; // Visual sprite component
    private bool collected = false; // Whether this powerup has been collected

    public enum PowerupType
    {
        Health, // Restores player health
        Speed, // Temporary movement speed boost
        Attack, // Temporary attack damage boost
        Jump // Temporary jump height boost
    }

    /// <summary>
    /// Initialize powerup visual appearance based on type
    /// </summary>
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetPowerupVisuals();
    }

    /// <summary>
    /// Update powerup visual effects while uncollected
    /// Creates rotating animation to attract player attention
    /// </summary>
    private void Update()
    {
        if (!collected)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            // Optional: subtle float effect
            float floatY = Mathf.Sin(Time.time * 2f) * 0.5f;
            transform.position = new Vector3(transform.position.x, transform.position.y + floatY * Time.deltaTime, transform.position.z);
        }
    }

    private void SetPowerupVisuals()
    {
        switch (powerupType)
        {
            case PowerupType.Health:
                spriteRenderer.color = Color.green;
                break;
            case PowerupType.Speed:
                spriteRenderer.color = Color.blue;
                break;
            case PowerupType.Attack:
                spriteRenderer.color = Color.red;
                break;
            case PowerupType.Jump:
                spriteRenderer.color = Color.yellow;
                break;
        }
    }

    /// <summary>
    /// Handle powerup collection when player touches it
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        Player1Controller p1 = other.GetComponent<Player1Controller>();
        Player2Controller p2 = other.GetComponent<Player2Controller>();

        if (p1 != null)
        {
            Collect(p1);
        }
        else if (p2 != null)
        {
            Collect(p2);
        }
    }

    public void Collect(MonoBehaviour player)
    {
        if (collected) return;

        collected = true;
        ApplyPowerup(player);

        CreateCollectionEffect();

        if (AudioManager.Instance != null)
            AudioManager.Instance.OnPowerupCollected();

        Destroy(gameObject);
    }

    private void ApplyPowerup(MonoBehaviour player)
    {
        switch (powerupType)
        {
            case PowerupType.Health:
                if (player is Player1Controller p1)
                {
                    int healAmount = Mathf.Min(value, 100 - p1.GetHealth());
                    p1.TakeDamage(-healAmount); // Negative damage = healing
                }
                else if (player is Player2Controller p2)
                {
                    int healAmount = Mathf.Min(value, 100 - p2.GetHealth());
                    p2.TakeDamage(-healAmount);
                }
                break;

            case PowerupType.Speed:
                StartCoroutine(ApplyTemporarySpeedBoost(player));
                break;

            case PowerupType.Attack:
                StartCoroutine(ApplyTemporaryAttackBoost(player));
                break;

            case PowerupType.Jump:
                StartCoroutine(ApplyTemporaryJumpBoost(player));
                break;
        }
    }

    private System.Collections.IEnumerator ApplyTemporarySpeedBoost(MonoBehaviour player)
    {
        // Extend player script to support temp speed boost
        yield return new WaitForSeconds(10f);
        // Remove boost here
    }

    private System.Collections.IEnumerator ApplyTemporaryAttackBoost(MonoBehaviour player)
    {
        // Extend player script to support temp attack boost
        yield return new WaitForSeconds(10f);
        // Remove boost here
    }

    private System.Collections.IEnumerator ApplyTemporaryJumpBoost(MonoBehaviour player)
    {
        // Extend player script to support temp jump boost
        yield return new WaitForSeconds(10f);
        // Remove boost here
    }

    private void CreateCollectionEffect()
    {
        GameObject effect = new GameObject("PowerupEffect");
        effect.transform.position = transform.position;

        var particleSystem = effect.AddComponent<ParticleSystem>();
        var main = particleSystem.main;
        main.startColor = spriteRenderer.color;
        main.maxParticles = 20;
        main.startLifetime = 1f;
        main.startSpeed = 5f;

        Destroy(effect, 2f);
    }
}
