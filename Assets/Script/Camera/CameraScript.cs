using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject targetPlayer;
    public Vector3 offset; 
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (targetPlayer != null)
        {
            transform.position = targetPlayer.transform.position + offset;
        }
    }

    void SettingTarger(GameObject gameObject) 
    {
        targetPlayer = gameObject;
        offset = transform.position - targetPlayer.transform.position;
    }
}
