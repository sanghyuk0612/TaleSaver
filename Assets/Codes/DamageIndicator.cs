using UnityEngine;
using TMPro;
using System.Collections;

public class DamageIndicator : MonoBehaviour
{
    private TextMeshPro damageText;
    private float moveSpeed = 1f;
    private float fadeSpeed = 1f;
    private float lifeTime = 1f;

    private void Awake()
    {
        damageText = GetComponent<TextMeshPro>();
    }

    public void Initialize(int damage, Color color)
    {
        if (damageText != null)
        {
            damageText.text = damage.ToString();
            damageText.color = color;
            Debug.Log($"데미지 인디케이터 초기화: damage={damage}, color={color}");
            StartCoroutine(FadeOut());
        }
        else
        {
            Debug.LogError("TextMeshPro 컴포넌트가 없습니다!");
        }
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + Vector3.up * 2f; // 위로 1유닛 이동
        Color startColor = damageText.color;

        while (elapsedTime < lifeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1f - (elapsedTime / lifeTime);
            
            // 위치 이동
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / lifeTime);
            
            // 색상 페이드 아웃
            damageText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            
            yield return null;
        }

        Destroy(gameObject);
    }
}