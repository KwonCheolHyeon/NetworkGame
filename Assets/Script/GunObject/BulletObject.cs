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
        // �浹�� ������Ʈ�� �±װ� "Player"���� Ȯ��
        if (collision.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerScript>().TakeDamage();
            Destroy(gameObject);
        }

        if (collision.CompareTag("Wall")) 
        {
            // �Ѿ� ����
            Destroy(gameObject);
        }
    }
}
