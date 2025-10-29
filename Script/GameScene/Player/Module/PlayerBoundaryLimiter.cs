using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �÷��̾ 'WorldBorder' �±��� Collider2D ������ ����� ���ϰ� �ϴ� ���.
/// �÷��̾��� BoxCollider2D ũ����� ����Ͽ� ��迡�� �ڿ������� ����.
/// </summary>
public class PlayerBoundaryLimiter
{
    private Rigidbody2D rb;
    private Collider2D worldBorder;
    private BoxCollider2D playerCollider;

    public PlayerBoundaryLimiter(Rigidbody2D rb, BoxCollider2D playerCollider = null)
    {
        this.rb = rb;
        this.playerCollider = playerCollider;
        FindWorldBorder();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindWorldBorder();
    }

    private void FindWorldBorder()
    {
        GameObject borderObj = GameObject.FindGameObjectWithTag("WorldBorder");
        if (borderObj != null)
        {
            worldBorder = borderObj.GetComponent<Collider2D>();
            if (worldBorder == null)
                Debug.LogWarning("[PlayerBoundaryLimiter] 'WorldBorder' �±� ������Ʈ�� Collider2D�� �����ϴ�!");
        }
        else
        {
            worldBorder = null;
            Debug.LogWarning("[PlayerBoundaryLimiter] 'WorldBorder' �±� ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    public void ApplyBoundaryLimit()
    {
        if (rb == null || worldBorder == null)
            return;

        Bounds borderBounds = worldBorder.bounds;
        Vector2 pos = rb.position;

        // �÷��̾� �ݶ��̴� ũ�� ���
        Vector2 halfSize = Vector2.zero;
        Vector2 offset = Vector2.zero;

        if (playerCollider != null)
        {
            // �ݶ��̴� ũ��� ������ ����
            halfSize = playerCollider.size * 0.5f;
            offset = playerCollider.offset;
        }

        // ��� ���� (�÷��̾� �ݶ��̴��� ��������ŭ ����)
        float minX = borderBounds.min.x + halfSize.x - offset.x;
        float maxX = borderBounds.max.x - halfSize.x - offset.x;
        float minY = borderBounds.min.y + halfSize.y - offset.y;
        float maxY = borderBounds.max.y - halfSize.y - offset.y;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        rb.position = pos;
    }

    public void Cleanup()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
