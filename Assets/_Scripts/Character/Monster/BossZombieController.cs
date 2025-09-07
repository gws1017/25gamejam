using System.Collections.Generic;
using UnityEngine;

public class BossZombieController : AIController
{
    private float rotateAngle = 0f;
    [SerializeField] private float rotateSpeed = 10f; // 회전속도
    [SerializeField] private List<float> attackAngles = new List<float>() { 0, 90, 180, 270 }; // 공격전환 각도

    private List<float> lastAngles = new List<float>();

    public float RotateAngle { get => rotateAngle; set => rotateAngle = value; }
    public float RotateSpeed { get => rotateSpeed; set => rotateSpeed = value; }

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
                if (lastAngles.Contains(angle)) return false;

                lastAngles.Add(angle);
                return true;
            }
        }
        lastAngles = new List<float>();
        return false;
    }
}
