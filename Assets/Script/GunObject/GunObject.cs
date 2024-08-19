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
        // 마우스의 월드 좌표 얻기
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // z축 값은 필요 없으므로 0으로 설정

        // 총과 마우스 사이의 거리 계산
        Vector2 direction = mousePosition - transform.position;

        // 각도 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;


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
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

    }
}
