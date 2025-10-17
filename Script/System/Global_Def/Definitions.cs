// Def_Name.cs (���� ���)
namespace Definitions
{
    public static class Def_Name
    {
        // �Է� �� �̸�
        public const string HORIZONTAL = "Horizontal";
        public const string VERTICAL = "Vertical";
        public const string GAME_CAMERA = "GameCamera";

        // �� �̸� (�⺻ ������ ���� ����)
        public const string SCENE_NAME_START_GAME = "Game_";
        public const string SCENE_NAME_DEFAULT_GAME = "Game_Town_MainVillage";

        // ī�װ��� ���� (��� ��: Game_Town_Village01)
        public const string SCENE_PREFIX_TOWN = "Game_Town_";
        public const string SCENE_PREFIX_FIELD = "Game_Field_";
        public const string SCENE_PREFIX_DUNGEON = "Game_Dungeon_";

        // Tag �̸�
        public const string PLAYER_TAG = "Player";
        public const string WORLD_BORDER_TAG = "WorldBorder";

        //��Ʈ �̸�
        public const string FONT_ENG = "NotoSans";
        public const string FONT_KOR = "NotoSansKR";
        public const string FONT_JPN = "NotoSansJP";
        public const string FONT_CNA = "NotoSansCN";
        public const string FONT_ENG_SDF = "NotoSans SDF";
        public const string FONT_KOR_SDF = "NotoSansKR SDF";
        public const string FONT_JPN_SDF = "NotoSansJP SDF";
        public const string FONT_CNA_SDF = "NotoSansCN SDF";
    }
    // UI / �α׿� ��� ����
    public static class Def_UI
    {
        // LocalizationManager ����
        public const string LOCALIZATION_CSV_NOT_FOUND = "[Localization] CSV ������ ã�� �� �����ϴ�: Resources/{0}.csv";
        public const string LOCALIZATION_CSV_EMPTY = "[Localization] CSV ������ ����ְų� ����� �ֽ��ϴ�.";
        public const string LOCALIZATION_HEADER_INVALID = "[Localization] CSV ����� �ùٸ��� �ʽ��ϴ�. �ּ� KEY, LANG �ʿ�.";
        public const string LOCALIZATION_DUPLICATE_KEY = "[Localization] �ߺ��� Ű �߰�: {0} (���õ�)";
        public const string LOCALIZATION_LOADED = "[Localization] {0}�� Ű �ε� �Ϸ�. �⺻ ���: {1}";
        public const string LOCALIZATION_NO_DATA = "[Localization] �ٱ��� �����Ͱ� �ε���� �ʾҽ��ϴ�!";
        public const string LOCALIZATION_FALLBACK_WARNING = "[Localization] '{0}'�� {1} ������ ���� �⺻ ���({2}) ���";
        public const string LOCALIZATION_KEY_NOT_FOUND = "[Localization] Ű�� ã�� �� �����ϴ�: {0}";
        public const string LOCALIZATION_ALREADY_SET = "[Localization] �̹� {0} �� �����Ǿ� �ֽ��ϴ�.";
        public const string LOCALIZATION_LANG_CHANGED = "[Localization] ��� ����: {0}";

        // LocalizedText ����
        public const string LOCALIZEDTEXT_NO_COMPONENT = "[LocalizedText] '{0}'�� Text �Ǵ� TextMeshProUGUI ������Ʈ�� �����ϴ�!";
        public const string LOCALIZEDTEXT_EMPTY_KEY = "[LocalizedText] '{0}'�� localizationKey�� ����ֽ��ϴ�!";
        public const string LOCALIZEDTEXT_MANAGER_NOT_INIT = "[LocalizedText] LocalizationManager�� �ʱ�ȭ���� �ʾҽ��ϴ�!";

        // Player ���� �����/�α�
        public const string PLAYER_FORCE_STOP_LOADING = "[Player] F1 Ű: ������ �ε� ����!";
        public const string PLAYER_LOADING_WARNING = "[Player] �ε� ��... �̵� �Ұ�. (F1 Ű�� ���� ���� ����)";
        public const string PLAYER_MELEE_HIT = "[Player] �ٰŸ� ���� ����! ���: {0}";
        public const string PLAYER_SAVED_SCENE = "[Player] ���Ӿ� '{0}' ���� ���� �Ϸ�.";
        public const string PLAYER_SAVE_INVALID_SCENE = "[Player] ������ ���Ӿ� �̸��� ��ȿ���� ���� �� ���� ��ŵ.";
        public const string PLAYER_RESTORE_STATE = "�÷��̾� ���� ����: Scene={0}, Pos=({1:F2},{2:F2})";

        // NPC / Dialogue / Quest / Shop ����
        public const string INTERACT_KEY_LABEL = "E";
        public const string NPC_INTERACT_AVAILABLE = "[NPC] ��ȣ�ۿ� ����: {0}Ű";
        public const string NPC_CONSOLE_HEADER = "[NPC] DialogueUIManager�� ����. �ַܼ� ��ȭ ���:";
        public const string UI_INTERACT_HINT = "[UI] ��ȣ�ۿ� ��Ʈ: {0}";
        public const string DIALOGUE_ALREADY_INTERACTING = "[DialogueUI] �̹� ��ȣ�ۿ� ���Դϴ�.";
        public const string DIALOGUE_PREFIX = "[Dialogue] ";
        public const string QUEST_OFFER_PREFIX = "[Quest Offer] ";
        public const string QUEST_NO_UI = "[Quest] ���� UI ���� - �ڵ� �ź� ó��";
        public const string SHOP_OPEN_PREFIX = "[Shop] ���� ����: ";
        public const string QUEST_ACCEPTED_PREFIX = "[Quest Accepted] ";
        public const string QUEST_DECLINED_PREFIX = "[Quest Declined] ";

        // ��Ÿ ���� ����� ����
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