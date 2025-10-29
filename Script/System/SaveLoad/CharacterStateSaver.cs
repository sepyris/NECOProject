using UnityEngine;
public class CharacterStateSaver : MonoBehaviour
{
    // ĳ���� ������Ʈ ���� (Movement, Inventory ��)
    // ...

    // ------------------------------------------------------------------
    // A. ����� ��Ȱ��ȭ �� ȣ�� (�� ���� �� ��)
    // ------------------------------------------------------------------
    public void SaveStateBeforeDeactivation()
    {
        SubSceneData dataToSave = new SubSceneData
        {
            positionX = transform.position.x,
            positionY = transform.position.y,
            positionZ = transform.position.z,
            health = 99, // ����
            // ... �ٸ� ���� ����
        };
        
        GameDataManager.Instance.SaveSubSceneState(dataToSave);
    }

    // ------------------------------------------------------------------
    // B. �� Ȱ��ȭ �� ȣ�� (�� ���� �� ��, �Ǵ� ��ü �ε� ��)
    // ------------------------------------------------------------------
    private void Start()
    {
        // �� Ȱ��ȭ �� ���� �ε� ���� (�ε� ȭ�� ������ ������ ���ٸ� Start���� ó��)
        // ����� �ӽ� ������ �ε�
        SubSceneData savedData = GameDataManager.Instance.LoadSubSceneState();
        RestoreSubSceneState(savedData);
    }

    public void RestoreSubSceneState(SubSceneData data)
    {
        // SubSceneData�� ĳ���� ���� ����
        transform.position = new Vector3(data.positionX, data.positionY, data.positionZ);
        // ... ü��, �κ��丮 �� ����
    }
}