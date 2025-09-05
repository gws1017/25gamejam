using UnityEngine;

public class RobotSpirit : MonoBehaviour
{

    [SerializeField] GameObject bulletPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Attack(float angle)
    {
        if (bulletPrefab == null) return;

        Instantiate(bulletPrefab, transform.position, Quaternion.AngleAxis(angle, Vector3.forward));
    }
}
