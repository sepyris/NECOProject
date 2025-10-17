// MapSpawnPoint.cs (���� ���� ���� ���� ������Ʈ�� ����)
using UnityEngine;

public class MapSpawnPoint : MonoBehaviour
{
    // MapTransition.cs�� targetSpawnPointID�� ��ġ�ؾ� �ϴ� ���� ID
    public string spawnPointID;

    // �����Ϳ��� ���� ������ ���� �ĺ��ϱ� ���� �����
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.7f);
    }
}