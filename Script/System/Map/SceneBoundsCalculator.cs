using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBoundsCalculator : MonoBehaviour
{
    void Start()
    {
        // ���� ���� ��� ��� �� ���
        Bounds sceneBounds = CalculateSceneBounds();
    }

    public Bounds CalculateSceneBounds()
    {
        // ���� Ȱ��ȭ�� ���� ������
        Scene currentScene = SceneManager.GetActiveScene();

        // �ʱ� ���� ���Ѵ�� ����
        Bounds bounds = new Bounds();
        bool firstRenderer = true;

        // ���� ��� ��Ʈ ���� ������Ʈ�� ��ȸ
        GameObject[] rootObjects = currentScene.GetRootGameObjects();
        foreach (GameObject rootObject in rootObjects)
        {
            // �ڽ� ������Ʈ�� ������ ��� Renderer ������Ʈ�� ������
            Renderer[] renderers = rootObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                // ù ��° �������� ���� �ʱ�ȭ
                if (firstRenderer)
                {
                    bounds = renderer.bounds;
                    firstRenderer = false;
                }
                // ���� �������� ��踦 ���� ��迡 ���Խ�Ŵ
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
        }

        return bounds;
    }
}