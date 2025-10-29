// MonsterSpawnArea.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterSpawnArea : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private int maxMonsterCount = 5; // 최대 몬스터 수
    [SerializeField] private float spawnInterval = 10f; // 스폰 체크 간격 (초)
    [SerializeField] private int monstersPerSpawn = 1; // 한 번에 스폰할 몬스터 수
    [Tooltip("부족 발생 시 한 번에 부족분을 전부 스폰할지 여부. 체크하면 monstersPerSpawn 무시.")]
    [SerializeField] private bool spawnAllMissingOnShortage = true;

    [Header("Spawn Area")]
    [SerializeField] private bool useCircleArea = false; // false: Box, true: Circle
    [SerializeField] private float circleRadius = 5f; // Circle 반지름

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    private List<GameObject> spawnedMonsters = new List<GameObject>();
    private Collider2D areaCollider;

    // 루트에 생성할 컨테이너 (스케일 영향 방지용)
    private Transform monstersContainer;

    void Awake()
    {
        EnsureAreaCollider();
        CreateMonstersContainer();
    }

    // 인스펙터에서 값 변경 시 자동 보정
    private void OnValidate()
    {
        EnsureAreaCollider();
    }

    // 컴포넌트 추가 시 기본 콜라이더 생성
    private void Reset()
    {
        EnsureAreaCollider();
    }

    // 콜라이더 존재 확인 및 필요 시 생성
    private void EnsureAreaCollider()
    {
        if (areaCollider != null) return;

        // 이미 있는 콜라이더 우선 검색
        CircleCollider2D existingCircle = GetComponent<CircleCollider2D>();
        BoxCollider2D existingBox = GetComponent<BoxCollider2D>();

        if (useCircleArea)
        {
            CircleCollider2D circle = existingCircle ?? gameObject.AddComponent<CircleCollider2D>();
            circle.radius = circleRadius;
            circle.isTrigger = true;
            areaCollider = circle;
        }
        else
        {
            BoxCollider2D box = existingBox ?? gameObject.AddComponent<BoxCollider2D>();
            // 기본 사이즈가 0이면 합리적 기본값 설정(필요시 인스펙터에서 조정)
            if (box.size == Vector2.zero)
            {
                box.size = Vector2.one * 5f;
            }
            box.isTrigger = true;
            areaCollider = box;
        }
    }

    private void CreateMonstersContainer()
    {
        if (monstersContainer != null) return;

        GameObject existing = GameObject.Find($"{name}_Monsters");
        if (existing != null)
        {
            monstersContainer = existing.transform;
            monstersContainer.localScale = Vector3.one;
            return;
        }

        GameObject containerGO = new GameObject($"{name}_Monsters");
        monstersContainer = containerGO.transform;
        monstersContainer.SetParent(null);
        monstersContainer.localScale = Vector3.one;
    }

    void Start()
    {
        // 초기 스폰
        SpawnMonsters(maxMonsterCount);

        // 주기적 스폰 시작
        StartCoroutine(SpawnRoutine());
    }

    /// <summary>
    /// 주기적으로 몬스터 수를 확인하고 부족하면 스폰
    /// </summary>
    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // 죽은 몬스터 참조 정리
            spawnedMonsters.RemoveAll(monster => monster == null);

            // 몬스터 수 확인
            int currentCount = spawnedMonsters.Count;
            int missing = Mathf.Max(0, maxMonsterCount - currentCount);

            int spawnCount;
            if (missing <= 0)
            {
                spawnCount = 0;
            }
            else if (spawnAllMissingOnShortage)
            {
                // 부족분을 한 번에 전부 스폰
                spawnCount = missing;
            }
            else
            {
                // 기존 동작: monstersPerSpawn 단위로 보충
                spawnCount = Mathf.Min(monstersPerSpawn, missing);
            }

            if (spawnCount > 0)
            {
                Debug.Log($"[SpawnArea] 몬스터 부족 감지: {currentCount}/{maxMonsterCount}. {spawnCount}마리 스폰.");
                SpawnMonsters(spawnCount);
            }
        }
    }

    /// <summary>
    /// 몬스터 스폰
    /// </summary>
    private void SpawnMonsters(int count)
    {
        if (monsterPrefab == null)
        {
            Debug.LogError("[SpawnArea] 몬스터 프리팹이 설정되지 않았습니다!");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            // 현재 몬스터 수 확인
            if (spawnedMonsters.Count >= maxMonsterCount)
            {
                Debug.LogWarning($"[SpawnArea] 최대 몬스터 수({maxMonsterCount}) 도달. 스폰 중단.");
                break;
            }

            Vector2 spawnPosition = GetRandomPositionInArea();

            // 프리팹을 monstersContainer 아래에 인스턴스화(스케일 영향 없음)
            GameObject monster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity, monstersContainer);

            // 몬스터에게 스폰 영역 알려주기
            MonsterController monsterController = monster.GetComponent<MonsterController>();
            if (monsterController != null)
            {
                monsterController.SetSpawnArea(areaCollider);
            }

            spawnedMonsters.Add(monster);
            Debug.Log($"[SpawnArea] 몬스터 스폰 완료: {spawnPosition}");
        }
    }

    /// <summary>
    /// 영역 내 랜덤 위치 반환 (콜라이더.bounds 기반)
    /// </summary>
    private Vector2 GetRandomPositionInArea()
    {
        if (areaCollider == null)
        {
            // 폴백: 기존 방식
            if (useCircleArea)
            {
                Vector2 randomDirection = Random.insideUnitCircle * circleRadius;
                return (Vector2)transform.position + randomDirection;
            }
            else
            {
                float randomX = Random.Range(-transform.localScale.x / 2f, transform.localScale.x / 2f);
                float randomY = Random.Range(-transform.localScale.y / 2f, transform.localScale.y / 2f);
                return (Vector2)transform.position + new Vector2(randomX, randomY);
            }
        }

        Bounds b = areaCollider.bounds;
        if (useCircleArea)
        {
            Vector2 randomDirection = Random.insideUnitCircle * circleRadius;
            return (Vector2)b.center + randomDirection;
        }
        else
        {
            float randomX = Random.Range(b.min.x, b.max.x);
            float randomY = Random.Range(b.min.y, b.max.y);
            return new Vector2(randomX, randomY);
        }
    }

    /// <summary>
    /// 몬스터가 죽었을 때 호출됨
    /// </summary>
    public void OnMonsterDied(GameObject monster)
    {
        if (spawnedMonsters.Contains(monster))
        {
            spawnedMonsters.Remove(monster);
            Debug.Log($"[SpawnArea] 몬스터 사망 알림 받음. 남은 몬스터: {spawnedMonsters.Count}/{maxMonsterCount}");
        }
    }

    /// <summary>
    /// 강제 스폰 (테스트용)
    /// </summary>
    [ContextMenu("Spawn One Monster")]
    public void SpawnOneMonster()
    {
        SpawnMonsters(1);
    }

    /// <summary>
    /// 모든 몬스터 제거
    /// </summary>
    [ContextMenu("Clear All Monsters")]
    public void ClearAllMonsters()
    {
        foreach (GameObject monster in spawnedMonsters)
        {
            if (monster != null)
            {
                Destroy(monster);
            }
        }
        spawnedMonsters.Clear();
        Debug.Log("[SpawnArea] 모든 몬스터 제거 완료.");
    }

    // 에디터용 기즈모 (스폰 영역 표시)
    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // 반투명 빨강

        if (useCircleArea)
        {
            Gizmos.DrawWireSphere(transform.position, circleRadius);
        }
        else
        {
            // bounds가 있으면 bounds 크기로, 없으면 transform.localScale로 그림
            if (areaCollider != null)
            {
                Bounds b = areaCollider.bounds;
                Gizmos.DrawWireCube(b.center, b.size);
            }
            else
            {
                Gizmos.DrawWireCube(transform.position, transform.localScale);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.red;

        if (useCircleArea)
        {
            Gizmos.DrawWireSphere(transform.position, circleRadius);
        }
        else
        {
            if (areaCollider != null)
            {
                Bounds b = areaCollider.bounds;
                Gizmos.DrawWireCube(b.center, b.size);
            }
            else
            {
                Gizmos.DrawWireCube(transform.position, transform.localScale);
            }
        }
    }
}