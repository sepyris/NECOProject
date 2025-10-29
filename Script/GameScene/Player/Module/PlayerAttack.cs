using System.Collections;
using UnityEngine;

public class PlayerAttack
{
    public enum AttackType { Melee, Ranged }
    public AttackType attackType = AttackType.Melee;
    public float attackDelay = 0.5f;
    public int attackDamage = 10;
    public float meleeRange = 2f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;

    public bool ControlsLocked = false;

    private bool isAttacking = false;
    public bool IsAttacking => isAttacking;
    private float lastAttackTime = -999f;

    private PlayerMovement movement;
    private PlayerAnimationController animationController;

    public void SetMovement(PlayerMovement movement) => this.movement = movement;
    public void SetAnimationController(PlayerAnimationController anim) => this.animationController = anim;

    public void HandleAttackInput()
    {
        if (ControlsLocked || isAttacking) return;

        if (Input.GetKeyDown(KeyCode.Space) && Time.time - lastAttackTime >= attackDelay)
        {
            movement.ApplyMovement(); // 이동 정지
            animationController?.PlayAnimation("Attack");
            Attack();
            lastAttackTime = Time.time;
            isAttacking = true;
            PlayerController.Instance.StartCoroutine(ResetAttack());
        }
    }

    private void Attack()
    {
        if (attackType == AttackType.Melee) MeleeAttack();
        else RangedAttack();
    }

    private void MeleeAttack()
    {
        Vector2 dir = movement.LastMoveDirection.normalized;
        Vector2 origin = (Vector2)PlayerController.Instance.transform.position + dir * 0.5f;
        int monsterLayer = LayerMask.GetMask("Monster");
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, meleeRange, monsterLayer);
        if (hit.collider != null)
        {
            var monster = hit.collider.GetComponent<MonsterController>();
            if (monster != null) monster.TakeDamage(attackDamage);
        }
        Debug.DrawRay(origin, dir * meleeRange, Color.red, 0.2f);
    }

    private void RangedAttack()
    {
        if (projectilePrefab == null) return;
        Vector2 spawn = (Vector2)PlayerController.Instance.transform.position + movement.LastMoveDirection * 0.5f;
        GameObject proj = GameObject.Instantiate(projectilePrefab, spawn, Quaternion.identity);
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = movement.LastMoveDirection * projectileSpeed;
        GameObject.Destroy(proj, 3f);
    }

    private IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(attackDelay);
        animationController?.PlayAnimation("Idle");
        isAttacking = false;
    }
}
