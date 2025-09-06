using UnityEngine;

public class RobotSpirit : MonoBehaviour
{

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int damage = 5; //로봇 정령 탄환 데미지

    public void Attack(float angle)
    {
        if (bulletPrefab == null) return;
        
        var bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.AngleAxis(angle, Vector3.forward)).GetComponent<Bullet>();

        bulletObject.Init(damage);
        bulletObject.AddIgnoreObject(gameObject); // 본인 무시(로봇정령)
        bulletObject.AddIgnoreObject(transform.parent.gameObject); // 플레이어 무시
    }
}
