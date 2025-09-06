using System.Collections;
using UnityEngine;

public class RobotSpirit : MonoBehaviour
{

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int damage = 5; //로봇 정령 탄환 데미지
    [SerializeField] private float parryTime = 0.2f; // 패링 가능시간
    [SerializeField] private bool isParrying = false; //패링 가능 상태

    private Coroutine parryCoroutine;
    public bool hasParried = false; //패링 연속 입력 방지 변수
    public bool detectedParryTarget = false; //패링중 대상 감지 여부
    public bool IsParrying => isParrying;
    public int Damage => damage;

    public void Attack(float angle)
    {
        if (bulletPrefab == null) return;
        
        var bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.AngleAxis(angle, Vector3.forward)).GetComponent<Bullet>();

        bulletObject.Init(damage, transform.parent.gameObject);
        bulletObject.AddIgnoreObject(gameObject); // 본인 무시(로봇정령)
        bulletObject.AddIgnoreObject(transform.parent.gameObject); // 플레이어 무시
    }

    //패링 초기화
    public void ClearParryFlags()
    {
        detectedParryTarget = false;
        hasParried = false;
    }

    public void ActiveParry()
    {
        if (parryCoroutine != null)
            return;

        parryCoroutine = StartCoroutine(ActivateParryCorutine());
    }

    IEnumerator ActivateParryCorutine()
    {
        isParrying = true;

        ClearParryFlags();

        yield return new WaitForSeconds(parryTime);
        isParrying = false;

        //주변에 패링 타겟이 있으나 거리가 너무 멀어 실패 처리되었을 경우
        if(hasParried == false && detectedParryTarget)
        {
            Debug.Log("패링 실패");
            GetComponentInParent<PlayerCharacter>().ApplyDamage();
        }
        parryCoroutine = null;
    }
}
