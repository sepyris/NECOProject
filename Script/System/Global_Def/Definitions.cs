// Def_Name.cs (통합 상수)
namespace Definitions
{
    public static class Def_Name
    {
        // 입력 축 이름
        public const string HORIZONTAL = "Horizontal";
        public const string VERTICAL = "Vertical";
        public const string GAME_CAMERA = "GameCamera";

        // 씬 이름 (기본 마스터 접두 유지)
        public const string SCENE_NAME_START_GAME = "Game_";
        public const string SCENE_NAME_DEFAULT_GAME = "Game_Town_MainVillage";

        // 카테고리별 접두 (사용 예: Game_Town_Village01)
        public const string SCENE_PREFIX_TOWN = "Game_Town_";
        public const string SCENE_PREFIX_FIELD = "Game_Field_";
        public const string SCENE_PREFIX_DUNGEON = "Game_Dungeon_";

        // Tag 이름
        public const string PLAYER_TAG = "Player";
        public const string WORLD_BORDER_TAG = "WorldBorder";

        //폰트 이름
        public const string FONT_ENG = "NotoSans";
        public const string FONT_KOR = "NotoSansKR";
        public const string FONT_JPN = "NotoSansJP";
        public const string FONT_CNA = "NotoSansCN";
        public const string FONT_ENG_SDF = "NotoSans SDF";
        public const string FONT_KOR_SDF = "NotoSansKR SDF";
        public const string FONT_JPN_SDF = "NotoSansJP SDF";
        public const string FONT_CNA_SDF = "NotoSansCN SDF";
    }
    // UI / 로그용 상수 모음
    public static class Def_UI
    {
        // LocalizationManager 관련
        public const string LOCALIZATION_CSV_NOT_FOUND = "[Localization] CSV 파일을 찾을 수 없습니다: Resources/{0}.csv";
        public const string LOCALIZATION_CSV_EMPTY = "[Localization] CSV 파일이 비어있거나 헤더만 있습니다.";
        public const string LOCALIZATION_HEADER_INVALID = "[Localization] CSV 헤더가 올바르지 않습니다. 최소 KEY, LANG 필요.";
        public const string LOCALIZATION_DUPLICATE_KEY = "[Localization] 중복된 키 발견: {0} (무시됨)";
        public const string LOCALIZATION_LOADED = "[Localization] {0}개 키 로드 완료. 기본 언어: {1}";
        public const string LOCALIZATION_NO_DATA = "[Localization] 다국어 데이터가 로드되지 않았습니다!";
        public const string LOCALIZATION_FALLBACK_WARNING = "[Localization] '{0}'의 {1} 번역이 없어 기본 언어({2}) 사용";
        public const string LOCALIZATION_KEY_NOT_FOUND = "[Localization] 키를 찾을 수 없습니다: {0}";
        public const string LOCALIZATION_ALREADY_SET = "[Localization] 이미 {0} 언어가 설정되어 있습니다.";
        public const string LOCALIZATION_LANG_CHANGED = "[Localization] 언어 변경: {0}";

        // LocalizedText 관련
        public const string LOCALIZEDTEXT_NO_COMPONENT = "[LocalizedText] '{0}'에 Text 또는 TextMeshProUGUI 컴포넌트가 없습니다!";
        public const string LOCALIZEDTEXT_EMPTY_KEY = "[LocalizedText] '{0}'의 localizationKey가 비어있습니다!";
        public const string LOCALIZEDTEXT_MANAGER_NOT_INIT = "[LocalizedText] LocalizationManager가 초기화되지 않았습니다!";

        // Player 관련 디버그/로그
        public const string PLAYER_FORCE_STOP_LOADING = "[Player] F1 키: 강제로 로딩 해제!";
        public const string PLAYER_LOADING_WARNING = "[Player] 로딩 중... 이동 불가. (F1 키로 강제 해제 가능)";
        public const string PLAYER_MELEE_HIT = "[Player] 근거리 공격 성공! 대상: {0}";
        public const string PLAYER_SAVED_SCENE = "[Player] 게임씬 '{0}' 상태 저장 완료.";
        public const string PLAYER_SAVE_INVALID_SCENE = "[Player] 저장할 게임씬 이름이 유효하지 않음 → 저장 스킵.";
        public const string PLAYER_RESTORE_STATE = "플레이어 상태 복원: Scene={0}, Pos=({1:F2},{2:F2})";

        // NPC / Dialogue / Quest / Shop 관련
        public const string INTERACT_KEY_LABEL = "E";
        public const string NPC_INTERACT_AVAILABLE = "[NPC] 상호작용 가능: {0}키";
        public const string NPC_CONSOLE_HEADER = "[NPC] DialogueUIManager가 없음. 콘솔로 대화 출력:";
        public const string UI_INTERACT_HINT = "[UI] 상호작용 힌트: {0}";
        public const string DIALOGUE_ALREADY_INTERACTING = "[DialogueUI] 이미 상호작용 중입니다.";
        public const string DIALOGUE_PREFIX = "[Dialogue] ";
        public const string QUEST_OFFER_PREFIX = "[Quest Offer] ";
        public const string QUEST_NO_UI = "[Quest] 선택 UI 없음 - 자동 거부 처리";
        public const string SHOP_OPEN_PREFIX = "[Shop] 상점 열기: ";
        public const string QUEST_ACCEPTED_PREFIX = "[Quest Accepted] ";
        public const string QUEST_DECLINED_PREFIX = "[Quest Declined] ";

        // 기타 공통 디버그 포맷
        public const string FORMAT_POS = "Pos=({0:F2},{1:F2})";
    }

    public enum SceneCategory
    {
        Unknown,
        Town,
        Field,
        Dungeon
    }

    public static class SceneHelpers
    {
        public static bool IsGameScene(string sceneName)
        {
            return !string.IsNullOrEmpty(sceneName) && sceneName.StartsWith(Def_Name.SCENE_NAME_START_GAME);
        }

        public static SceneCategory GetSceneCategory(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return SceneCategory.Unknown;
            if (sceneName.StartsWith(Def_Name.SCENE_PREFIX_TOWN)) return SceneCategory.Town;
            if (sceneName.StartsWith(Def_Name.SCENE_PREFIX_FIELD)) return SceneCategory.Field;
            if (sceneName.StartsWith(Def_Name.SCENE_PREFIX_DUNGEON)) return SceneCategory.Dungeon;
            return SceneCategory.Unknown;
        }
    }   
}