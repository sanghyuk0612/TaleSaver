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
            transform.position += Vector3.down * fastFallSpeed * Time.deltaTime;

            if (transform.position.y <= landingY)
            {
                hasLanded = true;
                transform.position = new Vector3(transform.position.x, landingY, transform.position.z);

                // 먼지 애니메이션 생성
                if (impactFXPrefab != null)
                {
                    Vector3 fxPosition = transform.position + new Vector3(0f, 1.5f, 0f); // y축 0.3f 위로 올림
                    Debug.Log($" Dust FX 생성 위치: {transform.position}");
                    Instantiate(impactFXPrefab, transform.position, Quaternion.identity);

                }

                // 3초 뒤 소 제거
                StartCoroutine(FadeOutAndDestroy());
            }
        }
    }


    private IEnumerator FadeOutAndDestroy()
    {
        yield return new WaitForSeconds(0.3f); // 1초간 정지
        Destroy(gameObject);
    }
}

