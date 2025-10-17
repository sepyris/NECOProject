using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemPersistent : MonoBehaviour
{
    void Awake()
    {
        // �̹� �ٸ� EventSystem�� �����ϸ� �ڽ��� ����
        if (FindObjectsOfType<EventSystem>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}