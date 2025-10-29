using UnityEngine;
using UnityEngine.EventSystems;

public class UiDragger : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    // �̵���ų ���α׷� â ��ü�� RectTransform (Panel)
    [SerializeField]
    private RectTransform windowRectTransform;

    // �巡�׸� �����ϴ� ����ǥ������ RectTransform
    private RectTransform statusRectTransform;

    // ���콺�� ������ ��ġ ������ �ʱ� ������
    private Vector2 pointerOffset;

    void Awake()
    {
        // �� ��ũ��Ʈ�� ���� ������Ʈ�� RectTransform�� �����ɴϴ�.
        statusRectTransform = GetComponent<RectTransform>();

        // windowRectTransform�� �Ҵ���� �ʾҴٸ�, �θ�(���α׷� â)�� ���
        if (windowRectTransform == null)
        {
            windowRectTransform = transform.parent.GetComponent<RectTransform>();
            if (windowRectTransform == null)
            {
                Debug.LogError("WindowDragger: �̵���ų windowRectTransform�� �Ҵ��ϰų� �θ𿡼� ã�� �� �����ϴ�.");
                enabled = false;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 localPointerPosition;

        // ���콺 Ŭ�� ��ġ�� �������� '�θ�'(��κ� Canvas) ���� ��ǥ�� ��ȯ�մϴ�.
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            windowRectTransform.parent.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition))
        {
            // �������� ���� ��ġ�� ��ȯ�� ���콺 ��ġ ������ '�Ÿ�(������)'�� �����մϴ�.
            // �� ������ ���п� ���콺 Ŭ�� ��ġ�� ������� â�� �ε巴�� ����ɴϴ�.
            pointerOffset = (Vector2)windowRectTransform.localPosition - localPointerPosition;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPointerPosition;

        // ���� ���콺 ��ġ�� �ٽ� �������� '�θ�' ���� ��ǥ�� ��ȯ�մϴ�.
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            windowRectTransform.parent.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition))
        {
            // ����� �������� ���Ͽ� �������� ���ο� ��ġ�� ����ϰ� �Ҵ��մϴ�.
            windowRectTransform.localPosition = localPointerPosition + pointerOffset;
        }
    }
}