using System.Collections.Generic;

/// <summary>
/// 채집 도구 타입
/// </summary>
public enum GatherToolType
{
    None,       // 도구 불필요
    Pickaxe,    // 곡괭이
    Sickle,     // 낫
    FishingRod, // 낚시대
    Axe         // 도끼
}

/// <summary>
/// 채집물 데이터
/// </summary>
[System.Serializable]
public class GatherableData
{
    public string gatherableID;        // ID
    public string gatherableName;      // 이름
    public string description;         // 설명
    public GatherToolType requiredTool; // 필요한 채집 도구
    public float gatherTime;           // 채집 소요 시간 (초)
    public List<DropItem> dropItems = new List<DropItem>(); // 드랍 아이템 테이블
}