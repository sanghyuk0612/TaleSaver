using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfUnderAttack : MonoBehaviour
{
    // Start is called before the first frame update
    public int attackDamage;
    private float nextDamageTime;
    public float knockbackForce=4f;
    public float damageCooldown = 0.1f;
    void Start()
    {
        nextDamageTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= nextDamageTime)
            {
                IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                    knockbackDir.y = 0.5f;
                    
                    damageable.TakeDamage(attackDamage, knockbackDir, knockbackForce);
                    nextDamageTime = Time.time + damageCooldown;
                }
            }
        }
    }
}
