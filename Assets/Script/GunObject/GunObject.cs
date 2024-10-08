using System.Collections;
using UnityEngine;

public class GunObject : MonoBehaviour
{
    public GameObject bulletObject; // 총알 프리팹
    public Transform playerTransform;
    public Transform firePoint; // 총알이 발사될 위치 (총구)
    public float bulletSpeed = 20f; // 총알의 속도
    public bool bIsPlayer;
    public bool bIsShotOn;
    public float gunAngle;
    private bool bmIsCanShoot; // 발사 가능 여부를 판단하는 변수
    void Start()
    {
        Setting();
    }
    
    void Update()
    {
        if (bIsPlayer)
        {
            RotateGunTowardsMouse();

            // 스페이스바를 눌렀을 때 총알 발사
            if (Input.GetKeyDown(KeyCode.Space) && bmIsCanShoot)
            {
                StartCoroutine(DelayedShoot());
                GameManager.Instance.SendPlayerDataToNetworkShot();
            }
        }

        if (playerTransform.gameObject.GetComponent<PlayerScript>().bIsDead) 
        {
            this.gameObject.SetActive(false); 
        }
    }

    public void Setting() 
    {
        playerTransform = transform.parent.gameObject.GetComponent<Transform>();
        if (playerTransform.gameObject.GetComponent<PlayerScript>().gameType == eGameCharacterType.Enemy)
        {
            bIsPlayer = false;
            bIsShotOn = false;
        }
        else if(playerTransform.gameObject.GetComponent<PlayerScript>().gameType == eGameCharacterType.PLAYER)
        {
            bIsPlayer = true;
        }
        bmIsCanShoot = true;
    }

    void RotateGunTowardsMouse()
    {
        // 마우스의 월드 좌표 얻기
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // z축 값은 필요 없으므로 0으로 설정

        // 총과 마우스 사이의 거리 계산
        Vector2 direction = mousePosition - transform.position;

        // 각도 계산
        gunAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Vector3 player = mousePosition - playerTransform.position;

        // 마우스가 플레이어 오른쪽에 있으면 정방향, 왼쪽에 있으면 반대 방향으로 회전
        if (player.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // 오른쪽을 향함
        }
        else if (player.x < 0)
        {
            transform.localScale = new Vector3(-1, -1, 1); // 왼쪽을 향함
        }

        // 회전 적용
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, gunAngle));

    }

    IEnumerator DelayedShoot()
    {
        bmIsCanShoot = false; // 발사 불가능 상태로 설정
        Shoot();
        yield return new WaitForSeconds(0.2f); // 0.2초 대기
        bmIsCanShoot = true; // 발사 가능 상태로 다시 설정
    }

    void Shoot()
    {
        // 총알을 생성하고 발사하는 메서드
        GameObject bullet = Instantiate(bulletObject, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        // 총알에 힘을 가해 발사
        rb.velocity = firePoint.right * bulletSpeed;
    }


    public void UpdateFromNetworkGunObject(float rotatinZ, float scaleX, bool shotOn)
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotatinZ));
        transform.localScale = new Vector3(scaleX, scaleX, 1);

        if (shotOn && bmIsCanShoot) // 네트워크에서 받은 shotOn 신호 처리
        {
            StartCoroutine(DelayedShoot()); // 즉시 총알 발사
        }
    }
}
