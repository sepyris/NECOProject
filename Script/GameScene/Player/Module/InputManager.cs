using System.Collections.Generic;

using System.Linq;
using UnityEngine;

public static class InputManager
{
    public static Vector2 currentInput;

    private static Vector2 savedInputForSceneTransition;
    private static bool hasValidSavedInput = false;

    // **추가: 최근 입력 히스토리 (최근 10프레임)**
    private static Queue<Vector2> inputHistory = new Queue<Vector2>(10);

    public static void UpdateInput()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) h += 1f;
        if (Input.GetKey(KeyCode.UpArrow)) v += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) v -= 1f;

        Vector2 dir = new Vector2(h, v);
        currentInput = dir.magnitude > 0.01f ? dir.normalized : Vector2.zero;

        // **히스토리에 추가**
        inputHistory.Enqueue(currentInput);
        if (inputHistory.Count > 10)
            inputHistory.Dequeue();
    }

    public static void SaveInputForSceneTransition()
    {
        // **방법 1: 현재 키 상태 직접 체크**
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) h += 1f;
        if (Input.GetKey(KeyCode.UpArrow)) v += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) v -= 1f;

        Vector2 directInput = new Vector2(h, v);
        if (directInput.magnitude > 0.01f)
            directInput = directInput.normalized;

        // **방법 2: 히스토리에서 가장 최근의 0이 아닌 입력 찾기**
        Vector2 historyInput = Vector2.zero;
        foreach (var input in inputHistory.Reverse())
        {
            if (input.magnitude > 0.01f)
            {
                historyInput = input;
                break;
            }
        }

        // **둘 중 하나라도 0이 아니면 그것을 사용**
        if (directInput.magnitude > 0.01f)
        {
            savedInputForSceneTransition = directInput;
            Debug.Log($"[InputManager] 직접 체크한 키 상태로 저장: {savedInputForSceneTransition}");
        }
        else if (historyInput.magnitude > 0.01f)
        {
            savedInputForSceneTransition = historyInput;
            Debug.Log($"[InputManager] 히스토리에서 복원: {savedInputForSceneTransition}");
        }
        else
        {
            savedInputForSceneTransition = currentInput;
            Debug.Log($"[InputManager] currentInput 사용: {savedInputForSceneTransition}");
        }

        hasValidSavedInput = true;

        Debug.Log($"[InputManager] 씬 전환용 입력 저장 완료: {savedInputForSceneTransition}");
    }

    public static Vector2 GetSavedInputForSceneTransition()
    {
        if (!hasValidSavedInput)
        {
            Debug.LogWarning("[InputManager] 저장된 입력이 없습니다. Vector2.zero 반환");
            return Vector2.zero;
        }

        Debug.Log($"[InputManager] 저장된 입력 반환: {savedInputForSceneTransition}");
        return savedInputForSceneTransition;
    }

    public static void ClearSavedInput()
    {
        hasValidSavedInput = false;
        savedInputForSceneTransition = Vector2.zero;
        Debug.Log("[InputManager] 저장된 입력 클리어");
    }

    public static Vector2 GetCurrentInput() => currentInput;
}