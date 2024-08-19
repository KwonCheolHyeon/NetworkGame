using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunObject : MonoBehaviour
{
    public Transform playerTransform;
    public bool bIsPlayer;
    void Start()
    {
        playerTransform = transform.parent.GetChild(0).gameObject.GetComponent<Transform>();
        bIsPlayer = false;
    }

    
    void Update()
    {
        if (bIsPlayer) 
        {
            RotateGunTowardsMouse();
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
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;


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
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

    }
}
