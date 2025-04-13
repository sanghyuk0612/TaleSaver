using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 offset;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 시작 시 클릭한 위치와 오브젝트 중심의 거리 저장
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out offset);
    }

    public RectTransform boundaryRect;  // BoardCanvas
    public float margin = 30f;          // 사방 최소 여백

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localMousePosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out localMousePosition))
        {
            Vector2 newPos = localMousePosition - offset;

            Rect boundary = boundaryRect.rect;
            float popupWidth = rectTransform.rect.width;
            float popupHeight = rectTransform.rect.height;

            float pivotX = rectTransform.pivot.x;
            float pivotY = rectTransform.pivot.y;

            //  좌측 경계: 오른쪽이 margin 이상 남아야
            float minX = boundary.xMin + margin - (1 - pivotX) * popupWidth;

            //  우측 경계: 왼쪽이 margin 이상 남아야
            float maxX = boundary.xMax - margin - pivotX * popupWidth;

            //  하단 경계: Top이 margin 이상 보여야
            float minY = boundary.yMin + margin - (1 - pivotY) * popupHeight;

            //  상단 경계: Bottom이 margin 이상 보여야
            float maxY = boundary.yMax - margin - pivotY * popupHeight;

            // 위치 제한
            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

            rectTransform.anchoredPosition = newPos;
        }
    }



}
