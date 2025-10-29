using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CSV 파일에서 몬스터 데이터를 로드하고 관리하는 싱글톤 매니저
/// </summary>
public class MonsterDataManager : MonoBehaviour
{
    public static MonsterDataManager Instance { get; private set; }

    [Header("CSV 파일")]
    public TextAsset csvFile; // CSV 파일을 Inspector에서 할당

    private Dictionary<string, MonsterData> monsterDatabase = new Dictionary<string, MonsterData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (csvFile != null)
            {
                ParseCSV(csvFile.text);
            }
            else
            {
                Debug.LogWarning("[MonsterDataManager] CSV 파일이 할당되지 않았습니다.");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// CSV 내용 파싱
    /// </summary>
    void ParseCSV(string csvText)
    {
        monsterDatabase.Clear();

        var lines = CSVUtility.GetLinesFromCSV(csvText);
        bool skipHeader = true;

        foreach (var raw in lines)
        {
            if (skipHeader) { skipHeader = false; continue; }
            if (string.IsNullOrWhiteSpace(raw)) continue;

            string trimmed = raw.TrimStart();
            if (trimmed.StartsWith("#")) continue;

            var parts = CSVUtility.SplitCSVLine(raw);

            // CSV 구조: ID,이름,설명,레벨,몬스터타입,선공여부,원거리여부,공격속도,이동속도,
            //          감지거리,힘,민첩,지력,최대체력,공격력,방어력,크리티컬확률,크리티컬데미지,
            //          회피확률,명중률,보상경험치,보상골드,보상아이템테이블
            if (parts.Count < 23) continue;

            MonsterData monster = new MonsterData
            {
                monsterID = parts[0].Trim(),
                monsterName = parts[1].Trim(),
                description = parts[2].Trim(),
                level = ParseInt(parts[3].Trim(), 1),
                monsterType = ParseMonsterType(parts[4].Trim()),
                isAggressive = ParseBool(parts[5].Trim()),
                isRanged = ParseBool(parts[6].Trim()),
                attackSpeed = ParseFloat(parts[7].Trim(), 1.0f),
                moveSpeed = ParseFloat(parts[8].Trim(), 1.0f),
                detectionRange = ParseFloat(parts[9].Trim(), 0f),
                strength = ParseInt(parts[10].Trim(), 0),
                dexterity = ParseInt(parts[11].Trim(), 0),
                intelligence = ParseInt(parts[12].Trim(), 0),
                maxHealth = ParseInt(parts[13].Trim(), 100),
                attackPower = ParseInt(parts[14].Trim(), 10),
                defense = ParseInt(parts[15].Trim(), 0),
                criticalRate = ParseFloat(parts[16].Trim(), 0f),
                criticalDamage = ParseFloat(parts[17].Trim(), 150f),
                evasionRate = ParseFloat(parts[18].Trim(), 0f),
                accuracy = ParseFloat(parts[19].Trim(), 100f),
                dropExp = ParseInt(parts[20].Trim(), 0),
                dropGold = ParseInt(parts[21].Trim(), 0)
            };

            // 드롭 아이템 테이블 파싱
            if (parts.Count > 22 && !string.IsNullOrWhiteSpace(parts[22]))
            {
                monster.dropItems = ParseDropTable(parts[22].Trim());
            }

            // 데이터베이스에 추가
            if (!monsterDatabase.ContainsKey(monster.monsterID))
            {
                monsterDatabase.Add(monster.monsterID, monster);
            }
            else
            {
                Debug.LogWarning($"[MonsterDataManager] 중복 몬스터 ID: {monster.monsterID}");
            }
        }

        Debug.Log($"[MonsterDataManager] CSV에서 {monsterDatabase.Count}개의 몬스터 로드 완료");
    }

    /// <summary>
    /// 드롭 아이템 테이블 파싱
    /// 형식: 아이템ID:확률:개수;아이템ID:확률:개수;...
    /// </summary>
    private List<DropItem> ParseDropTable(string tableStr)
    {
        List<DropItem> drops = new List<DropItem>();

        if (string.IsNullOrWhiteSpace(tableStr))
            return drops;

        // 세미콜론으로 각 아이템 분리
        string[] items = tableStr.Split(';');

        foreach (string itemStr in items)
        {
            if (string.IsNullOrWhiteSpace(itemStr))
                continue;

            // 콜론으로 아이템ID:확률:개수 분리
            string[] itemParts = itemStr.Trim().Split(':');

            if (itemParts.Length >= 3)
            {
                DropItem drop = new DropItem
                {
                    itemID = itemParts[0].Trim(),
                    dropRate = ParseFloat(itemParts[1].Trim(), 0f),
                    quantity = ParseInt(itemParts[2].Trim(), 1)
                };

                drops.Add(drop);
            }
            else
            {
                Debug.LogWarning($"[MonsterDataManager] 잘못된 드롭 아이템 형식: {itemStr}");
            }
        }

        return drops;
    }

    /// <summary>
    /// 몬스터 타입 파싱
    /// </summary>
    private MonsterType ParseMonsterType(string typeStr)
    {
        switch (typeStr.ToLower())
        {
            case "normal":
                return MonsterType.Normal;
            case "elite":
                return MonsterType.Elite;
            case "boss":
                return MonsterType.Boss;
            default:
                Debug.LogWarning($"[MonsterDataManager] 알 수 없는 몬스터 타입: {typeStr}");
                return MonsterType.Normal;
        }
    }

    // ==========================================
    // 안전한 파싱 헬퍼 메서드
    // ==========================================

    private int ParseInt(string str, int defaultValue = 0)
    {
        if (int.TryParse(str, out int result))
            return result;
        return defaultValue;
    }

    private float ParseFloat(string str, float defaultValue = 0f)
    {
        if (float.TryParse(str, out float result))
            return result;
        return defaultValue;
    }

    private bool ParseBool(string str)
    {
        str = str.ToLower();
        return str == "true" || str == "1" || str == "yes" || str == "o" || str == "예";
    }

    // ==========================================
    // 조회 메서드
    // ==========================================

    /// <summary>
    /// 몬스터 ID로 데이터 가져오기
    /// </summary>
    public MonsterData GetMonsterData(string monsterID)
    {
        if (monsterDatabase.TryGetValue(monsterID, out MonsterData data))
        {
            return data;
        }

        Debug.LogWarning($"[MonsterDataManager] 몬스터를 찾을 수 없음: {monsterID}");
        return null;
    }

    /// <summary>
    /// 모든 몬스터 데이터 가져오기
    /// </summary>
    public Dictionary<string, MonsterData> GetAllMonsters()
    {
        return monsterDatabase;
    }

    /// <summary>
    /// 특정 타입의 몬스터만 가져오기
    /// </summary>
    public List<MonsterData> GetMonstersByType(MonsterType type)
    {
        List<MonsterData> result = new List<MonsterData>();

        foreach (var monster in monsterDatabase.Values)
        {
            if (monster.monsterType == type)
            {
                result.Add(monster);
            }
        }

        return result;
    }

    /// <summary>
    /// 특정 레벨 범위의 몬스터 가져오기
    /// </summary>
    public List<MonsterData> GetMonstersByLevelRange(int minLevel, int maxLevel)
    {
        List<MonsterData> result = new List<MonsterData>();

        foreach (var monster in monsterDatabase.Values)
        {
            if (monster.level >= minLevel && monster.level <= maxLevel)
            {
                result.Add(monster);
            }
        }

        return result;
    }
}