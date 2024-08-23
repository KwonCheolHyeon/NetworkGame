
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

    public void SettingTarger(GameObject gameObject) 
    {
        targetPlayer = gameObject;
        Vector3 camera = new Vector3(targetPlayer.transform.position.x, targetPlayer.transform.position.y, -10.0f);
        transform.position = camera;
        offset = transform.position - targetPlayer.transform.position;
    }
}
