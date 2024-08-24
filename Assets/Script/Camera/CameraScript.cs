
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private GameObject mTargetPlayer;
    private Vector3 mOffset; 
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (mTargetPlayer != null)
        {
            transform.position = mTargetPlayer.transform.position + mOffset;
        }
    }

    public void SettingTarger(GameObject gameObject) 
    {
        mTargetPlayer = gameObject;
        Vector3 camera = new Vector3(mTargetPlayer.transform.position.x, mTargetPlayer.transform.position.y, -10.0f);
        transform.position = camera;
        mOffset = transform.position - mTargetPlayer.transform.position;
    }
}
