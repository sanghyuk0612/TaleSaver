using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FireballBehavior : MonoBehaviour
{
   private int damage;
   private float radius;
   private Vector3 direction;
   private float speed = 10f; // Fireball 속도
   private Vector3 startPosition; // Fireball의 시작 위치
    public void Initialize(int damage, float radius, Vector3 direction)
   {
       this.damage = damage;
       this.radius = radius;
       this.direction = direction.normalized;
       this.startPosition = transform.position; // 시작 위치 저장
   }
    private void Update()
   {
       // Fireball 이동
       transform.position += direction * speed * Time.deltaTime;
        // 범위 내 적 탐지
       DetectEnemies();
        // Fireball이 radius 만큼 진행했는지 확인
       if (Vector3.Distance(startPosition, transform.position) >= radius)
       {
           Destroy(gameObject); // radius에 도달하면 Fireball 제거
       }
   }
    private void DetectEnemies()
   {
       // 범위 내 적 탐지
       Collider[] hitEnemies = Physics.OverlapSphere(transform.position, radius);
       foreach (var enemy in hitEnemies)
       {
           if (enemy.CompareTag("Enemy"))
           {
               // 적에게 데미지 입히기
               IDamageable damageable = enemy.GetComponent<IDamageable>();
               if (damageable != null)
               {
                   // 넉백 방향 계산
                   Vector3 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                   float knockbackForce = 5f; // 넉백 힘 설정

                   damageable.TakeDamage(damage, knockbackDirection, knockbackForce); // 넉백과 함께 데미지 적용
               }
           }
       }
   }
    private void OnDrawGizmosSelected()
   {
       // 범위 시각화
       Gizmos.color = Color.red;
       Gizmos.DrawWireSphere(transform.position, radius);
   }
}