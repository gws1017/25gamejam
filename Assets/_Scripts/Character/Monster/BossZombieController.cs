using System.Collections.Generic;
using UnityEngine;

public class BossZombieController : AIController
{
    private float rotateAngle = 0f;
    [SerializeField] private float rotateSpeed = 10f; // 회전속도
    [SerializeField] private List<float> attackAngles = new List<float>() { 0, 90, 180, 270 }; // 공격전환 각도

    // 직전에 발사한 각도만 기억한다 (리스트로 계속 쌓는 방식은, 공격각 간격이 촘촘해서
    // 판정 구간이 끊김 없이 이어지는 패턴3에서 "리셋될 틈"이 영영 안 생겨 한 바퀴 돌고 나면
    // 리스트가 꽉 차서 영구히 공격을 못 하게 되는 버그가 있었음)
    private float? lastFiredAngle = null;

    public float RotateAngle { get => rotateAngle; set => rotateAngle = value; }
    public float RotateSpeed { get => rotateSpeed; set => rotateSpeed = value; }

    /// <summary>공격 트리거 각도 간격을 교체한다 (패턴1 90도 ↔ 패턴3 10도 전환용).</summary>
    public void SetAttackAngles(List<float> newAngles)
    {
        attackAngles = newAngles;
        lastFiredAngle = null;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        ChangeState(AIState.Spawn);
    }

    //기본 이동이 좀비보스는 달라서 오버라이드하여 구현
    protected override void CalculateAIMovement()
    {
        RotateFromTarget();
    }

    private void RotateFromTarget()
    {
        if (targetPlayer == null) return;

        rotateAngle += rotateSpeed * Time.fixedDeltaTime;

        float rad = rotateAngle * Mathf.Deg2Rad;

        Vector2 center = targetPlayer.position;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * owner.AttackRange;
        Vector2 targetPos = center + offset;

        Vector2 nextPos = Vector2.MoveTowards(rigidBody2D.position, targetPos, moveSpeed * Time.fixedDeltaTime);
        rigidBody2D.MovePosition(nextPos);

        float currentAngle = rotateAngle % 360f;
        if(CanAttack(currentAngle))
        {
            GetComponent<Animator>().SetTrigger("Idle");
            ChangeState(AIState.Attack);
        }
    }

    bool CanAttack(float currentAngle)
    {
        foreach(var angle in attackAngles)
        {
            if(Mathf.Abs(Mathf.DeltaAngle(currentAngle, angle)) <= 5f)
            {
                // 직전에 쏜 각도랑 같은 구간이면 중복 발사 방지. 다르면(= 새 구간에 진입했으면) 발사.
                // 몇 바퀴를 돌아도 값 하나만 비교하므로 리스트가 쌓여서 막히는 일이 없다.
                if (lastFiredAngle.HasValue && Mathf.Approximately(lastFiredAngle.Value, angle)) return false;

                lastFiredAngle = angle;
                return true;
            }
        }
        return false;
    }
}
