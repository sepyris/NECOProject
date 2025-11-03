using Definitions;
using GameData.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class DatabaseGenerater
{
    [Tooltip("CSV파일을 선택해서 메뉴 클릭")]
    [MenuItem("Assets/Create/Game Data/Convert CSV to ScriptableObject", false, 1)]
    public static void ConvertSelectedCSV()
    {
        Object[] selectedObjects = Selection.objects;
        //TextAsset csvFile = Selection.activeObject as TextAsset;

        foreach (Object obj in selectedObjects)
        {
            TextAsset csvFile = obj as TextAsset;
            if (csvFile == null) continue;

            string directory = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(csvFile));
            string normalizedPath = directory.Replace('\\', '/');
            string soPath = normalizedPath + "/Database/" + csvFile.name + "Database.asset";
            if (csvFile.name.Contains(Def_CSV.ITEMS))
            {
                ParseItemDataCSV(csvFile.text, soPath);
            }
            else if (csvFile.name.Contains(Def_CSV.QUESTS))
            {
                ParseQuestDataCSV(csvFile.text, soPath);
            }
            else if (csvFile.name.Contains(Def_CSV.DIALOGUES))
            {
                ParseDialogDataCSV(csvFile.text, soPath);
            }
            else if (csvFile.name.Contains(Def_CSV.GATHERABLES))
            {
                ParseGatherableDataCSV(csvFile.text, soPath);
            }
            else if (csvFile.name.Contains(Def_CSV.NPCINFO))
            {
                ParseNPCInfoDataCSV(csvFile.text, soPath);
            }
            else if (csvFile.name.Contains(Def_CSV.MONSTER))
            {
                ParseMonsterDataCSV(csvFile.text, soPath);
            }
            else if (csvFile.name.Contains(Def_CSV.MAPINFO))
            {
                ParseMapInfoDataCSV(csvFile.text, soPath);
            }
        }
        
    }

    //DialogueData 파싱
    private static void ParseDialogDataCSV(string csvText, string soPath)
    {
        DialogueSequenceSO database = AssetDatabase.LoadAssetAtPath<DialogueSequenceSO>(soPath);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<DialogueSequenceSO>();
            AssetDatabase.CreateAsset(database, soPath);
        }
        database.Items.Clear();
        var lines = GetLinesFromCSV(csvText);
        bool skipHeader = true;
        DialogueSequence currentSequence = null;

        foreach (var raw in lines)
        {
            if (skipHeader) { skipHeader = false; continue; }
            if (string.IsNullOrWhiteSpace(raw)) continue;

            string trimmed = raw.TrimStart();
            if (trimmed.StartsWith("#")) continue;

            // ⭐ CSV 구조: npcId, DialogueType, QuestID, Text (4개 컬럼) ⭐
            var parts = SplitCSVLine(raw);
            if (parts.Count < 4) continue;

            string npcId = parts[0].Trim();
            string dialogueType = parts[1].Trim();
            string questId = parts[2].Trim();
            string text = parts[3].Trim();

            // 새로운 시퀀스 시작 (npcId와 dialogueType이 모두 있는 경우)
            if (!string.IsNullOrEmpty(npcId) && !string.IsNullOrEmpty(dialogueType))
            {
                currentSequence = new DialogueSequence
                {
                    npcId = npcId,
                    dialogueType = dialogueType,
                    questId = questId
                };
                database.Items.Add(currentSequence);
            }

            // 대사 추가 (text가 비어있지 않으면)
            // ⭐ Speaker 정보는 저장하지 않음 - 런타임에 npcId로 조회 ⭐
            if (currentSequence != null && !string.IsNullOrEmpty(text))
            {
                currentSequence.lines.Add(new DialogueLine
                {
                    Text = text
                });
            }
        }

        if (database != null)
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"CSV 파싱 및 ScriptableObject **{soPath}** 생성 완료! ({database.Items.Count}개 데이터)");
        }
        else
        {
            Debug.LogWarning($"변환할 수 없는 CSV 파일입니다");
        }
    }

    private static void ParseGatherableDataCSV(string csvText, string soPath)
    {
        GatherableDataSO database = AssetDatabase.LoadAssetAtPath<GatherableDataSO>(soPath);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<GatherableDataSO>();
            AssetDatabase.CreateAsset(database, soPath);
        }
        database.Items.Clear();
        var lines = GetLinesFromCSV(csvText);
        bool skipHeader = true;

        foreach (var raw in lines)
        {
            if (skipHeader) { skipHeader = false; continue; }
            if (string.IsNullOrWhiteSpace(raw)) continue;

            string trimmed = raw.TrimStart();
            if (trimmed.StartsWith("#")) continue;

            var parts = SplitCSVLine(raw);

            // CSV 구조: ID,이름,설명,채집도구,채집속도,보상아이템테이블
            if (parts.Count < 6) continue;

            GatherableData gatherable = new()
            {
                gatherableID = parts[0].Trim(),
                gatherableName = parts[1].Trim(),
                description = parts[2].Trim(),
                requiredTool = ParseGatherTool(parts[3].Trim()),
                gatherTime = ParseFloat(parts[4].Trim(), 1.0f)
            };

            // 보상 아이템 테이블 파싱
            if (parts.Count > 5 && !string.IsNullOrWhiteSpace(parts[5]))
            {
                gatherable.dropItems = new List<ItemReward>();
                string rewardsStr = parts[5].Trim();
                if (!string.IsNullOrEmpty(rewardsStr))
                {
                    var rewards = rewardsStr.Split(';');
                    foreach (var r in rewards)
                    {
                        gatherable.dropItems.Add(new ItemReward(r));
                    }
                }
            }
            database.Items.Add(gatherable);
        }

        if (database != null)
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"CSV 파싱 및 ScriptableObject **{soPath}** 생성 완료! ({database.Items.Count}개 데이터)");
        }
        else
        {
            Debug.LogWarning($"변환할 수 없는 CSV 파일입니다");
        }
    }

    private static void ParseItemDataCSV(string csvText, string soPath)
    {
        ItemDataSO database = AssetDatabase.LoadAssetAtPath<ItemDataSO>(soPath);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<ItemDataSO>();
            AssetDatabase.CreateAsset(database, soPath);
        }
        database.Items.Clear();

        var lines = GetLinesFromCSV(csvText);
        bool skipHeader = true;

        foreach (var raw in lines)
        {
            if (skipHeader) { skipHeader = false; continue; }
            if (string.IsNullOrWhiteSpace(raw)) continue;

            string trimmed = raw.TrimStart();
            if (trimmed.StartsWith("#")) continue;

            var parts = SplitCSVLine(raw);
            // CSV 구조: itemID, itemName, itemType, description, maxStack, buyPrice, sellPrice, iconPath
            if (parts.Count < 8) continue;

            // 모든 파트는 SplitCSVLine에서 따옴표를 고려하여 분리되었으므로,
            // 이제 각 파트의 앞뒤 공백만 제거합니다.
            string itemId = parts[0].Trim();
            string itemName = parts[1].Trim();
            string itemTypeStr = parts[2].Trim();
            string description = parts[3].Trim();
            string maxStackStr = parts[4].Trim();
            string buyPriceStr = parts[5].Trim();
            string sellPriceStr = parts[6].Trim();
            string iconPath = parts[7].Trim();

            ItemData item = new()
            {
                itemId = itemId,
                itemName = itemName,
                itemType = ParseItemType(itemTypeStr),
                description = description,
                // int.Parse 대신 안전한 TryParse를 사용하거나, 값이 비어있을 경우 0으로 처리하는 것이 좋습니다.
                // 여기서는 기존 코드를 따라 int.Parse를 유지하되, TryParse를 권장합니다.
                maxStack = ParseInt(maxStackStr),
                buyPrice = ParseInt(buyPriceStr),
                sellPrice = ParseInt(sellPriceStr),
                iconPath = iconPath
            };

            // 소비 아이템 데이터 (8번째 인덱스: healAmount)
            if (parts.Count > 8)
            {
                int.TryParse(parts[8].Trim(), out item.healAmount);
            }

            // 장비 데이터 (9번째 이후)
            if (parts.Count > 9 && !string.IsNullOrEmpty(parts[9].Trim()))
            {
                item.equipSlot = ParseEquipSlot(parts[9].Trim());
            }
            if (parts.Count > 10)
            {
                int.TryParse(parts[10].Trim(), out item.attackBonus);
            }
            if (parts.Count > 11)
            {
                int.TryParse(parts[11].Trim(), out item.defenseBonus);
            }
            if (parts.Count > 12)
            {
                int.TryParse(parts[12].Trim(), out item.strBonus);
            }
            if (parts.Count > 13)
            {
                int.TryParse(parts[13].Trim(), out item.dexBonus);
            }
            if (parts.Count > 14)
            {
                int.TryParse(parts[14].Trim(), out item.intBonus);
            }
            database.Items.Add(item);
        }

        if (database != null)
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"CSV 파싱 및 ScriptableObject **{soPath}** 생성 완료! ({database.Items.Count}개 데이터)");
        }
        else
        {
            Debug.LogWarning($"변환할 수 없는 CSV 파일입니다");
        }
    }

    private static void ParseQuestDataCSV(string csvText, string soPath)
    {
        QuestDataSO database = AssetDatabase.LoadAssetAtPath<QuestDataSO>(soPath);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<QuestDataSO>();
            AssetDatabase.CreateAsset(database, soPath);
        }
        database.Items.Clear();
        var lines = GetLinesFromCSV(csvText);
        bool skipHeader = true;

        foreach (var raw in lines)
        {
            if (skipHeader) { skipHeader = false; continue; }
            if (string.IsNullOrWhiteSpace(raw)) continue;

            var parts = SplitCSVLine(raw);
            if (parts.Count < 9) continue;

            QuestData quest = new()
            {
                questId = parts[0].Trim(),
                questName = parts[1].Trim(),
                description = parts[2].Trim().Replace("\\n", "\n"),
                rewardExp = ParseInt(parts[7].Trim(),0),
                rewardGold = ParseInt(parts[8].Trim(),0)
            };

            // 선행 조건 파싱
            string prereqType = parts[3].Trim();
            string prereqValue = parts[4].Trim();

            if (!string.IsNullOrEmpty(prereqType) && prereqType != "None")
            {
                if (System.Enum.TryParse(prereqType, out PrerequisiteType pType))
                {
                    quest.prerequisite.type = pType;
                    quest.prerequisite.value = prereqValue;
                }
            }

            // Objectives 파싱
            quest.objectives = new List<QuestObjective>();
            string objectivesStr = parts[5].Trim();
            if (!string.IsNullOrEmpty(objectivesStr))
            {
                var objectives = objectivesStr.Split(';');
                foreach (var obj in objectives)
                {
                    var seg = obj.Split(':');
                    if (seg.Length < 3) continue;

                    if (System.Enum.TryParse(seg[0].Trim(), out QuestType qType))
                    {
                        quest.objectives.Add(new QuestObjective
                        {
                            type = qType,
                            targetId = seg[1].Trim(),
                            requiredCount = int.Parse(seg[2].Trim()),
                            currentCount = 0
                        });
                    }
                }
            }

            // Reward Items 파싱
            quest.rewards = new List<ItemReward>();
            string rewardsStr = parts[6].Trim();
            if (!string.IsNullOrEmpty(rewardsStr))
            {
                var rewards = rewardsStr.Split(';');
                foreach (var r in rewards)
                {
                    quest.rewards.Add(new ItemReward(r));
                }
            }
            database.Items.Add(quest);
        }

        if (database != null)
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"CSV 파싱 및 ScriptableObject **{soPath}** 생성 완료! ({database.Items.Count}개 데이터)");
        }
        else
        {
            Debug.LogWarning($"변환할 수 없는 CSV 파일입니다");
        }
    }

    private static void ParseNPCInfoDataCSV(string csvText, string soPath)
    {
        NPCInfoSO database = AssetDatabase.LoadAssetAtPath<NPCInfoSO>(soPath);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<NPCInfoSO>();
            AssetDatabase.CreateAsset(database, soPath);
        }
        database.Items.Clear();
        var lines = GetLinesFromCSV(csvText);
        bool skipHeader = true;

        foreach (var raw in lines)
        {
            if (skipHeader) { skipHeader = false; continue; }
            if (string.IsNullOrWhiteSpace(raw)) continue;

            var parts = SplitCSVLine(raw);
            if (parts.Count < 5) continue;

            string npcId = parts[0].Trim();
            if (string.IsNullOrEmpty(npcId)) continue;

            Npcs info = new()
            {
                npcId = npcId,
                npcName = parts[1].Trim(),
                npcTitle = parts[2].Trim(),
                npcDescription = parts[3].Trim(),
                mapId = parts[4].Trim()

            };
            database.Items.Add(info);
        }

        if (database != null)
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"CSV 파싱 및 ScriptableObject **{soPath}** 생성 완료! ({database.Items.Count}개 데이터)");
        }
        else
        {
            Debug.LogWarning($"변환할 수 없는 CSV 파일입니다");
        }
    }

    private static void ParseMonsterDataCSV(string csvText, string soPath)
    {
        MonsterDataSO database = AssetDatabase.LoadAssetAtPath<MonsterDataSO>(soPath);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<MonsterDataSO>();
            AssetDatabase.CreateAsset(database, soPath);
        }
        database.Items.Clear();
        var lines = GetLinesFromCSV(csvText);
        bool skipHeader = true;

        foreach (var raw in lines)
        {
            if (skipHeader) { skipHeader = false; continue; }
            if (string.IsNullOrWhiteSpace(raw)) continue;

            string trimmed = raw.TrimStart();
            if (trimmed.StartsWith("#")) continue;

            var parts = SplitCSVLine(raw);

            // CSV 구조: ID,이름,설명,레벨,몬스터타입,선공여부,원거리여부,공격속도,이동속도,
            //          감지거리,힘,민첩,지력,최대체력,공격력,방어력,크리티컬확률,크리티컬데미지,
            //          회피확률,명중률,보상경험치,보상골드,보상아이템테이블
            if (parts.Count < 23) continue;

            MonsterData monster = new()
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
                monster.dropItems = new List<ItemReward>();
                string rewardsStr = parts[22].Trim();
                if (!string.IsNullOrEmpty(rewardsStr))
                {
                    var rewards = rewardsStr.Split(';');
                    foreach (var r in rewards)
                    {
                        monster.dropItems.Add(new ItemReward(r));
                    }
                }
            }
            database.Items.Add(monster);
        }

        if (database != null)
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"CSV 파싱 및 ScriptableObject **{soPath}** 생성 완료! ({database.Items.Count}개 데이터)");
        }
        else
        {
            Debug.LogWarning($"변환할 수 없는 CSV 파일입니다");
        }
    }

    private static void ParseMapInfoDataCSV(string csvText, string soPath)
    {
        MapInfoSO database = AssetDatabase.LoadAssetAtPath<MapInfoSO>(soPath);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<MapInfoSO>();
            AssetDatabase.CreateAsset(database, soPath);
        }
        database.Items.Clear();
        var lines = GetLinesFromCSV(csvText);
        bool skipHeader = true;

        foreach (var raw in lines)
        {
            if (skipHeader) { skipHeader = false; continue; }
            if (string.IsNullOrWhiteSpace(raw)) continue;

            var parts = SplitCSVLine(raw);
            if (parts.Count < 5) continue;

            string mapId = parts[0].Trim();
            if (string.IsNullOrEmpty(mapId)) continue;

            Maps info = new()
            {
                mapId = mapId,
                mapName = parts[1].Trim(),
                mapType = parts[2].Trim(),
                mapDescription = parts[3].Trim(),
                parentMapId = parts[4].Trim()
            };
            database.Items.Add(info);
        }

        if (database != null)
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"CSV 파싱 및 ScriptableObject **{soPath}** 생성 완료! ({database.Items.Count}개 데이터)");
        }
        else
        {
            Debug.LogWarning($"변환할 수 없는 CSV 파일입니다");
        }
    }




    // ==========================================
    // 안전한 파싱 헬퍼 메서드
    // ==========================================

    private static float ParseFloat(string str, float defaultValue = 0f)
    {
        if (float.TryParse(str, out float result))
            return result;
        return defaultValue;
    }
    private static int ParseInt(string str, int defaultValue = 0)
    {
        if (int.TryParse(str, out int result))
            return result;
        return defaultValue;
    }

    private static bool ParseBool(string str)
    {
        str = str.ToLower();
        return str == "true";
    }


    /// <summary>
    /// 채집 도구 타입 파싱
    /// </summary>
    private static GatherToolType ParseGatherTool(string toolStr)
    {
        switch (toolStr.ToLower())
        {
            case "pickaxe":
                return GatherToolType.Pickaxe;
            case "sickle":
                return GatherToolType.Sickle;
            case "fishingrod":
                return GatherToolType.FishingRod;
            case "axe":
                return GatherToolType.Axe;
            case "none":
                return GatherToolType.None;
            default:
                Debug.LogWarning($"[GatherableDataManager] 알 수 없는 채집 도구 타입: {toolStr}");
                return GatherToolType.None;
        }
    }

    /// <summary>
    /// 아이템 타입 파싱
    /// </summary>
    private static ItemType ParseItemType(string typeStr)
    {
        switch (typeStr.ToLower())
        {
            case "equipment": return ItemType.Equipment;
            case "consumable": return ItemType.Consumable;
            case "material": return ItemType.Material;
            case "questitem": return ItemType.QuestItem;
            default:
                Debug.LogWarning($"[ItemDataManager] 알 수 없는 아이템 타입: {typeStr}");
                return ItemType.Material;
        }
    }

    /// <summary>
    /// 장비 슬롯 파싱
    /// </summary>
    private static EquipmentSlot ParseEquipSlot(string slotStr)
    {
        return slotStr.ToLower() switch
        {
            "weapon" => EquipmentSlot.Weapon,
            "armor" => EquipmentSlot.Armor,
            "accessory" => EquipmentSlot.Accessory,
            _ => EquipmentSlot.None,
        };
    }

    /// <summary>
    /// 몬스터 타입 파싱
    /// </summary>
    private static MonsterType ParseMonsterType(string typeStr)
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



    // 💡 함수를 public static으로 선언해야 다른 파일에서 접근 가능합니다.

    public static List<string> GetLinesFromCSV(string csvText)
    {
        List<string> lines = new();
        string currentLine = "";
        bool inQuotes = false;

        // ... (함수 내용 유지) ...
        for (int i = 0; i < csvText.Length; i++)
        {
            char c = csvText[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }

            // 줄바꿈 문자 확인
            if (c == '\n')
            {
                // 큰따옴표 밖에 있을 때만 줄바꿈을 레코드의 끝으로 인식합니다.
                if (!inQuotes)
                {
                    // 캐리지 리턴(\r)이 포함되어 있다면 제거
                    lines.Add(currentLine.Trim('\r'));
                    currentLine = "";
                    continue;
                }
            }

            currentLine += c;
        }

        // 마지막 줄 추가
        if (!string.IsNullOrEmpty(currentLine.Trim('\r', '\n')))
        {
            lines.Add(currentLine.Trim('\r'));
        }

        return lines;
    }

    public static List<string> SplitCSVLine(string line)
    {
        List<string> result = new();
        bool inQuotes = false;
        string current = "";

        // ... (함수 내용 유지) ...
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }

        result.Add(current);
        return result;
    }
}