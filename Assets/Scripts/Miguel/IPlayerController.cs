
public interface IPlayerController 
{
    int GetHealth();
    void TakeDamage(int damage);
    void Heal(int amount);
    void EnableDoubleJump(bool enabled);
    void SetSpeedBoost(float multiplier, float duration);
    void SetAttackBoost(float multiplier, float duration);

    string GetPlayerName();
}
