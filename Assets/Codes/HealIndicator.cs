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
        if (healText == null)
        {
            Debug.LogError("TextMeshPro 컴포넌트를 찾을 수 없습니다!");
        }
    }

    public void Initialize(int heal, Color color)
    {
        if (healText != null)
        {
            healText.text = heal.ToString();
            healText.fontMaterial.SetColor("_FaceColor", color);
            Debug.Log($"힐 인디케이터 초기화: heal={heal}, color={color}");
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