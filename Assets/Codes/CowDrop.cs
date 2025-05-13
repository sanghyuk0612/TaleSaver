using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;

using UnityEngine;

public class CowDrop : MonoBehaviour
{
    public float fastFallSpeed = 15f;
    public float slowFallSpeed = 2f;
    public float slowFallDistance = 1f;

    private float landingY;
    private bool hasLanded = false;
    private bool isSlowFalling = false;
    private float slowFallStartY;
    private Rigidbody2D rb;
    public GameObject impactFXPrefab;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetLandingY(float y)
    {
        landingY = y;
    }

    private void Update()
    {
        if (!hasLanded)
        {
            // 빠르게 착지 지점까지 낙하
            transform.position += Vector3.down * fastFallSpeed * Time.deltaTime;

            if (transform.position.y <= landingY)
            {
                hasLanded = true;
                isSlowFalling = true;
                slowFallStartY = transform.position.y;

                // 소 착지 시
                if (impactFXPrefab != null)
                {
                    Instantiate(impactFXPrefab, transform.position, Quaternion.identity);
                }

                // 중력 비활성화
                if (rb != null)
                {
                    rb.gravityScale = 0f;
                    rb.velocity = Vector2.zero;
                }

                // 카메라 흔들기
                //SkillManager skillManager = FindObjectOfType<SkillManager>();
                //if (skillManager != null)
                //{
                    //skillManager.StartCoroutine(skillManager.ShakeCamera(0.3f, 0.2f));
                //}
            }
        }
        else if (isSlowFalling)
        {
            transform.position += Vector3.down * slowFallSpeed * Time.deltaTime;

            if (transform.position.y <= slowFallStartY - slowFallDistance)
            {
                isSlowFalling = false;

                // 착지 후 정지 후 사라짐
                StartCoroutine(FadeOutAndDestroy());
            }
        }
    }

    private IEnumerator FadeOutAndDestroy()
    {
        yield return new WaitForSeconds(0f); // 1초간 정지
        Destroy(gameObject);
    }
}

