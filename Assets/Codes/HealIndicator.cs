using UnityEngine;
using TMPro;
using System.Collections;

public class HealIndicator : MonoBehaviour
{
    private TextMeshPro healText;
    private float moveSpeed = 1f;
    private float fadeSpeed = 1f;
    private float lifeTime = 1f;

    private void Awake()
    {
        healText = GetComponent<TextMeshPro>();
    }

    public void Initialize(int heal, Color color)
    {
        healText.text = heal.ToString();
        healText.color = color;
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + Vector3.up * 2f; // 위로 2유닛 이동
        Color startColor = healText.color;

        while (elapsedTime < lifeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1f - (elapsedTime / lifeTime);
            
            // 위치 이동
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / lifeTime);
            
            // 색상 페이드 아웃
            healText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            
            yield return null;
        }

        Destroy(gameObject);
    }
} 