using UnityEngine;

public class KeepAlive : MonoBehaviour
{
    public static KeepAlive Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // �ߺ� ����
        }
    }
}
