using UnityEngine;

public class PlayerAnimationController
{
    private Animator animator;

    public PlayerAnimationController(Animator animator)
    {
        this.animator = animator;
    }

    public void PlayAnimation(string trigger)
    {
        if (animator != null)
            animator.SetTrigger(trigger);
    }
}
