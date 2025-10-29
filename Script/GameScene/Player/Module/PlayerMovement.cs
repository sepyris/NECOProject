using UnityEngine;

public class PlayerMovement
{
    private Rigidbody2D rb;
    public float MoveSpeed = 5f;
    public bool ControlsLocked = false;
    public Vector2 LastMoveDirection { get; private set; } = Vector2.right;
    private Vector2 currentInput;

    public PlayerMovement(Rigidbody2D rb)
    {
        this.rb = rb;
    }

    // ⭐ 외부에서 방향을 강제로 설정할 수 있는 메서드 추가
    public void SetLastDirection(Vector2 direction)
    {
        if (direction.magnitude > 0.01f)
        {
            LastMoveDirection = direction.normalized;
        }
    }

    public void UpdateMovement(Vector2 input)
    {
        if (ControlsLocked) { currentInput = Vector2.zero; return; }
        currentInput = input.normalized;
        if (currentInput.magnitude > 0.01f)
            LastMoveDirection = currentInput;
    }

    public void ApplyMovement()
    {
        if (ControlsLocked) { rb.velocity = Vector2.zero; return; }
        rb.velocity = currentInput * MoveSpeed;
    }

}