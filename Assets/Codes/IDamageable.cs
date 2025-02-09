using UnityEngine;

public interface IDamageable
{
    void RestoreHealth(int health);
    void TakeDamage(int damage, Vector2 knockbackDirection, float knockbackForce);
}
