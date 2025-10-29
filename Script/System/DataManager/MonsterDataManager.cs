using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CSV ���Ͽ��� ���� �����͸� �ε��ϰ� �����ϴ� �̱��� �Ŵ���
/// </summary>
public class MonsterDataManager : MonoBehaviour
{
    public static MonsterDataManager Instance { get; private set; }

    [Header("CSV ����")]
    public TextAsset csvFile; // CSV ������ Inspector���� �Ҵ�

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
                Debug.LogWarning("[MonsterDataManager] CSV ������ �Ҵ���� �ʾҽ��ϴ�.");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// CSV ���� �Ľ�
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

            // CSV ����: ID,�̸�,����,����,����Ÿ��,��������,���Ÿ�����,���ݼӵ�,�̵��ӵ�,
            //          �����Ÿ�,��,��ø,����,�ִ�ü��,���ݷ�,����,ũ��Ƽ��Ȯ��,ũ��Ƽ�õ�����,
            //          ȸ��Ȯ��,���߷�,�������ġ,������,������������̺�
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

            // ��� ������ ���̺� �Ľ�
            if (parts.Count > 22 && !string.IsNullOrWhiteSpace(parts[22]))
            {
                monster.dropItems = ParseDropTable(parts[22].Trim());
            }

            // �����ͺ��̽��� �߰�
            if (!monsterDatabase.ContainsKey(monster.monsterID))
            {
                monsterDatabase.Add(monster.monsterID, monster);
            }
            else
            {
                Debug.LogWarning($"[MonsterDataManager] �ߺ� ���� ID: {monster.monsterID}");
            }
        }

        Debug.Log($"[MonsterDataManager] CSV���� {monsterDatabase.Count}���� ���� �ε� �Ϸ�");
    }

    /// <summary>
    /// ��� ������ ���̺� �Ľ�
    /// ����: ������ID:Ȯ��:����;������ID:Ȯ��:����;...
    /// </summary>
    private List<DropItem> ParseDropTable(string tableStr)
    {
        List<DropItem> drops = new List<DropItem>();

        if (string.IsNullOrWhiteSpace(tableStr))
            return drops;

        // �����ݷ����� �� ������ �и�
        string[] items = tableStr.Split(';');

        foreach (string itemStr in items)
        {
            if (string.IsNullOrWhiteSpace(itemStr))
                continue;

            // �ݷ����� ������ID:Ȯ��:���� �и�
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
                Debug.LogWarning($"[MonsterDataManager] �߸��� ��� ������ ����: {itemStr}");
            }
        }

        return drops;
    }

    /// <summary>
    /// ���� Ÿ�� �Ľ�
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
                Debug.LogWarning($"[MonsterDataManager] �� �� ���� ���� Ÿ��: {typeStr}");
                return MonsterType.Normal;
        }
    }

    // ==========================================
    // ������ �Ľ� ���� �޼���
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
        return str == "true" || str == "1" || str == "yes" || str == "o" || str == "��";
    }

    // ==========================================
    // ��ȸ �޼���
    // ==========================================

    /// <summary>
    /// ���� ID�� ������ ��������
    /// </summary>
    public MonsterData GetMonsterData(string monsterID)
    {
        if (monsterDatabase.TryGetValue(monsterID, out MonsterData data))
        {
            return data;
        }

        Debug.LogWarning($"[MonsterDataManager] ���͸� ã�� �� ����: {monsterID}");
        return null;
    }

    /// <summary>
    /// ��� ���� ������ ��������
    /// </summary>
    public Dictionary<string, MonsterData> GetAllMonsters()
    {
        return monsterDatabase;
    }

    /// <summary>
    /// Ư�� Ÿ���� ���͸� ��������
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
    /// Ư�� ���� ������ ���� ��������
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