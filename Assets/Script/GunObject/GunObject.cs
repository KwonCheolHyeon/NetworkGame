using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunObject : MonoBehaviour
{
    public GameObject bulletObject; // �Ѿ� ������
    public Transform playerTransform;
    public Transform firePoint; // �Ѿ��� �߻�� ��ġ (�ѱ�)
    public float bulletSpeed = 20f; // �Ѿ��� �ӵ�
    public bool bIsPlayer;
    public bool bIsShotOn;
    public float gunAngle;
    void Start()
    {
        Setting();
    }
    
    void Update()
    {
        if (bIsPlayer)
        {
            RotateGunTowardsMouse();

            // �����̽��ٸ� ������ �� �Ѿ� �߻�
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Shoot();
                GameManager.Instance.SendPlayerDataToNetworkShot();
            }
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
    }

    void RotateGunTowardsMouse()
    {
        // ���콺�� ���� ��ǥ ���
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // z�� ���� �ʿ� �����Ƿ� 0���� ����

        // �Ѱ� ���콺 ������ �Ÿ� ���
        Vector2 direction = mousePosition - transform.position;

        // ���� ���
        gunAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Vector3 player = mousePosition - playerTransform.position;

        // ���콺�� �÷��̾� �����ʿ� ������ ������, ���ʿ� ������ �ݴ� �������� ȸ��
        if (player.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // �������� ����
        }
        else if (player.x < 0)
        {
            transform.localScale = new Vector3(-1, -1, 1); // ������ ����
        }

        // ȸ�� ����
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, gunAngle));

    }
    void Shoot()
    {
        // �Ѿ��� �����ϰ� �߻��ϴ� �޼���
        GameObject bullet = Instantiate(bulletObject, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        // �Ѿ˿� ���� ���� �߻�
        rb.velocity = firePoint.right * bulletSpeed;

        if (!bIsPlayer)
        {
            bIsShotOn = false;
        }
    }


    public void UpdateFromNetworkGunObject(float rotatinZ, float scaleX, bool shotOn)
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotatinZ));
        transform.localScale = new Vector3(scaleX, scaleX, 1);

        if (shotOn)
        {
            Shoot(); // ��� �Ѿ� �߻�
        }
    }
}
