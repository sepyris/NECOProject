using UnityEngine;
using UnityEngine.EventSystems;

public class UiDragger : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    // 이동시킬 프로그램 창 전체의 RectTransform (Panel)
    [SerializeField]
    private RectTransform windowRectTransform;

    // 드래그를 시작하는 상태표시줄의 RectTransform
    private RectTransform statusRectTransform;

    // 마우스와 윈도우 위치 사이의 초기 오프셋
    private Vector2 pointerOffset;

    void Awake()
    {
        // 이 스크립트가 붙은 오브젝트의 RectTransform을 가져옵니다.
        statusRectTransform = GetComponent<RectTransform>();

        // windowRectTransform이 할당되지 않았다면, 부모(프로그램 창)를 사용
        if (windowRectTransform == null)
        {
            windowRectTransform = transform.parent.GetComponent<RectTransform>();
            if (windowRectTransform == null)
            {
                Debug.LogError("WindowDragger: 이동시킬 windowRectTransform을 할당하거나 부모에서 찾을 수 없습니다.");
                enabled = false;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 localPointerPosition;

        // 마우스 클릭 위치를 윈도우의 '부모'(대부분 Canvas) 로컬 좌표로 변환합니다.
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            windowRectTransform.parent.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition))
        {
            // 윈도우의 현재 위치와 변환된 마우스 위치 사이의 '거리(오프셋)'를 저장합니다.
            // 이 오프셋 덕분에 마우스 클릭 위치에 관계없이 창이 부드럽게 따라옵니다.
            pointerOffset = (Vector2)windowRectTransform.localPosition - localPointerPosition;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPointerPosition;

        // 현재 마우스 위치를 다시 윈도우의 '부모' 로컬 좌표로 변환합니다.
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            windowRectTransform.parent.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition))
        {
            // 저장된 오프셋을 더하여 윈도우의 새로운 위치를 계산하고 할당합니다.
            windowRectTransform.localPosition = localPointerPosition + pointerOffset;
        }
    }
}