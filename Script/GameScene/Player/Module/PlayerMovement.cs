using UnityEngine;

public class PlayerMovement
{
    public Rigidbody2D rb;
    public float MoveSpeed = 5f;
    public bool ControlsLocked = false;
    public Vector2 LastMoveDirection { get; private set; } = Vector2.right;
    public Vector2 currentInput;

    private const int LOAD_GUARD_FRAMES = 50;
    private int loadGuardCounter = 0;

    // **추가: 추가 대기 프레임**
    private const int POST_LOAD_WAIT_FRAMES = 60;
    private int postLoadWaitCounter = 0;

    public bool IsInLoadGuard => loadGuardCounter > 0;
    public bool IsInPostLoadWait => postLoadWaitCounter > 0;

    // LoadGuard와 PostLoadWait를 하나로 통합
    private int inputRecoveryCounter = 0;
    private const int INPUT_RECOVERY_MAX_FRAMES = 120; // 2초

    public bool IsInInputRecovery => inputRecoveryCounter > 0;

    public PlayerMovement(Rigidbody2D rb)
    {
        this.rb = rb;
        currentInput = Vector2.zero;
        LastMoveDirection = Vector2.right;
    }

    public void SetLastDirection(Vector2 direction)
    {
        if (direction.magnitude > 0.01f)
            LastMoveDirection = direction.normalized;
    }

    public void DecrementLoadGuard()
    {
        if (loadGuardCounter > 0)
        {
            loadGuardCounter--;

            if (loadGuardCounter == 0)
            {
                // LoadGuard 종료 시 PostLoadWait 시작
                postLoadWaitCounter = POST_LOAD_WAIT_FRAMES;
                Debug.LogWarning("[PlayerMovement] LoadGuard 종료! PostLoadWait 시작");
            }
        }
    }

    public void DecrementPostLoadWait()
    {
        if (postLoadWaitCounter > 0)
        {
            postLoadWaitCounter--;

            if (postLoadWaitCounter == 0)
            {
                Debug.LogWarning("[PlayerMovement] PostLoadWait 종료! 정상 입력으로 전환");
            }
        }
    }

    public void SetLoadGuard() => loadGuardCounter = LOAD_GUARD_FRAMES;

    public void UpdateMovement()
    {
        if (ControlsLocked) { currentInput = Vector2.zero; return; }
        currentInput = InputManager.currentInput;
        if (currentInput.magnitude > 0.01f)
            LastMoveDirection = currentInput;
    }

    public void ApplyMovement()
    {
        if (rb == null) return;
        rb.velocity = ControlsLocked ? Vector2.zero : currentInput * MoveSpeed;
    }

    public void StartInputRecovery()
    {
        inputRecoveryCounter = INPUT_RECOVERY_MAX_FRAMES;
    }

    public void UpdateInputRecovery(Vector2 realTimeInput)
    {
        if (inputRecoveryCounter > 0)
        {
            // 실시간 키가 감지되면 즉시 복구 완료
            if (realTimeInput.magnitude > 0.01f)
            {
                currentInput = realTimeInput;
                inputRecoveryCounter = 0; // 즉시 종료
                Debug.LogWarning($"[PlayerMovement] Input 시스템 복구 완료: {currentInput}");
            }
            else
            {
                inputRecoveryCounter--;
                // 저장된 입력 유지
            }
        }
    }
}