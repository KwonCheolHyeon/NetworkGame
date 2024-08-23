using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletObject : MonoBehaviour
{
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌한 오브젝트의 태그가 "Player"인지 확인
        if (collision.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerScript>().TakeDamage();
            Destroy(gameObject);
        }

        if (collision.CompareTag("Wall")) 
        {
            // 총알 제거
            Destroy(gameObject);
        }
    }
}
