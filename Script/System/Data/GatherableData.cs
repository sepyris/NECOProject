using System.Collections.Generic;

/// <summary>
/// ä�� ���� Ÿ��
/// </summary>
public enum GatherToolType
{
    None,       // ���� ���ʿ�
    Pickaxe,    // ���
    Sickle,     // ��
    FishingRod, // ���ô�
    Axe         // ����
}

/// <summary>
/// ä���� ������
/// </summary>
[System.Serializable]
public class GatherableData
{
    public string gatherableID;        // ID
    public string gatherableName;      // �̸�
    public string description;         // ����
    public GatherToolType requiredTool; // �ʿ��� ä�� ����
    public float gatherTime;           // ä�� �ҿ� �ð� (��)
    public List<DropItem> dropItems = new List<DropItem>(); // ��� ������ ���̺�
}