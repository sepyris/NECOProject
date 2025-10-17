#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq;

public static class LocKeyGenerator
{
    // CSV ���� ��� (Resources ���� ��)
    private const string CSV_PATH = "Assets/Script/System/CSV/localization.csv";
    // ������ C# ���� ���
    private const string OUTPUT_PATH = "Assets/Script/Generated/LocKeys.cs";

    // Unity ������ �޴��� ��ư �߰�
    [MenuItem("Tools/Localization/Generate Localization Keys")]
    public static void GenerateKeys()
    {
        // 1. CSV ���� �б�
        if (!File.Exists(CSV_PATH))
        {
            Debug.LogError("Localization CSV file not found at: " + CSV_PATH);
            return;
        }

        string[] lines = File.ReadAllLines(CSV_PATH);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV file is empty or only contains headers.");
            return;
        }

        // 2. Ű ��� ���� (ù ��° ���� �ǳʶٰ�, �� ������ ù ��° �׸�)
        var keys = lines
            .Skip(1) // ���(ù ��) �ǳʶٱ�
            .Select(line => line.Split(',').FirstOrDefault()?.Trim()) // ��ǥ�� �и� �� ù �׸�(KEY) ����
            .Where(key => !string.IsNullOrEmpty(key)) // ������� ���� Ű�� ����
            .Distinct() // �ߺ� ����
            .ToList();

        // 3. C# ���� ���� ���� (StringBuilder ���)
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("// �� ������ LocKeyGenerator�� ���� �ڵ����� �����Ǿ����ϴ�.");
        sb.AppendLine("// CSV ������ �����Ǹ� 'Tools/Localization/Generate Localization Keys'�� �����ϼ���.");
        sb.AppendLine();
        sb.AppendLine("public static class LocKeys");
        sb.AppendLine("{");

        foreach (string key in keys)
        {
            // ��ȿ�� C# �ĺ������� ���� �˻�(���ڷ� �����ϸ� ������ھ� �߰�)
            string safeKey = key;
            if (string.IsNullOrEmpty(safeKey)) continue;
            // ���顤Ư������ ����(���� ó��)
            safeKey = new string(safeKey.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
            if (char.IsDigit(safeKey.FirstOrDefault())) safeKey = "_" + safeKey;

            sb.AppendLine($"    public const string {safeKey} = \"{key}\";");
        }

        sb.AppendLine("}");

        // 4. ���� ���� �� Unity ������ ������Ʈ
        string directory = Path.GetDirectoryName(OUTPUT_PATH);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(OUTPUT_PATH, sb.ToString(), Encoding.UTF8);

        Debug.Log($"Localization Keys generated successfully: {keys.Count} keys written to {OUTPUT_PATH}");

        // Unity �����Ϳ� ������ ���� �����Ǿ����� �˸�
        AssetDatabase.Refresh();
    }
}
#endif