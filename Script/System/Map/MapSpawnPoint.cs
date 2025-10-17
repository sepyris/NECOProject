// MapSpawnPoint.cs (도착 씬의 스폰 지점 오브젝트에 부착)
using UnityEngine;

public class MapSpawnPoint : MonoBehaviour
{
    // MapTransition.cs의 targetSpawnPointID와 일치해야 하는 고유 ID
    public string spawnPointID;

    // 에디터에서 스폰 지점을 쉽게 식별하기 위한 기즈모
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.7f);
    }
}