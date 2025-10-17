using Definitions;
using System.Collections.Generic;

// 데이터 구조체를 GameSave 네임스페이스 안에 정의합니다.
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

        // 무결성 검증용 해시 (파일 변조/교체 방지)
        public string integrityHash = "";
        // 전역 상태 정보
        public int totalCurrency = 0;//캐릭터가 가진 돈
        public int playerLevel = 1;//캐릭터의 레벨
    }
}