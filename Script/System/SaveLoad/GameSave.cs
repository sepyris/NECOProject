using Definitions;
using System.Collections.Generic;

// ������ ����ü�� GameSave ���ӽ����̽� �ȿ� �����մϴ�.
namespace GameSave
{
    [System.Serializable]
    public struct SubSceneData
    {
        public string currentSceneName;

        public float positionX;
        public float positionY;
        public float positionZ;
        public int health;
        public List<string> inventoryItems;

        public static SubSceneData Default() => new SubSceneData { health = 100, inventoryItems = new List<string>() };
    }

    [System.Serializable]
    public class GlobalSaveData
    {
        public SubSceneData subSceneState = SubSceneData.Default();
        public string currentSceneName = Def_Name.SCENE_NAME_DEFAULT_GAME;

        // ���Ἲ ������ �ؽ� (���� ����/��ü ����)
        public string integrityHash = "";
        // ���� ���� ����
        public int totalCurrency = 0;//ĳ���Ͱ� ���� ��
        public int playerLevel = 1;//ĳ������ ����
    }
}