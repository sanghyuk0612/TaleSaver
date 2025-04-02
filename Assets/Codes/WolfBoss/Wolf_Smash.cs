using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf_Smash : MonoBehaviour
{
    public float radius = 3f; // 공격 범위
    // Start is called before the first frame update
    public int attackDamage=10;
    private float nextDamageTime;
    public float knockbackForce=3f;
    public float damageCooldown = 0.1f;
    void Start()
{
    nextDamageTime = Time.time;

    // CircleCollider2D 가져오기 또는 추가
    CircleCollider2D collider = GetComponent<CircleCollider2D>();
    if (collider == null)
    {
        collider = gameObject.AddComponent<CircleCollider2D>();
    }

    // // 스프라이트 크기 조절
    // SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
    // if (spriteRenderer != null)
    // {
    //     float baseSize = spriteRenderer.sprite.bounds.size.x; // 기본 스프라이트 크기
    //     float scaleFactor = (radius * 2) / baseSize; // 반지름 기반 스케일 조정
    //     transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
    // }

    // // ⚡ 콜라이더 크기를 스프라이트 크기에 맞춤
    // collider.radius = radius / transform.lossyScale.x;
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
