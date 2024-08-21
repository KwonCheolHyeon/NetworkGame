using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<GameManager>();

                    instance.Start();
                }
            }
            return instance;
        }
    }

    public GameObject playerPrefab; // 플레이어 프리팹
    public GameObject otherPlayerPrefab; // 다른 플레이어 프리팹

    public PlayerScript playerScript; // 로컬 플레이어
    public GunObject playerGun; // 로컬 플레이어

    public List<PlayerScript> otherPlayer = new List<PlayerScript>(); // 다른 플레이어 리스트
    public List<GunObject> otherPlayerGun = new List<GunObject>(); // 다른 플레이어 리스트
    public List<Transform> playerSpawnTransform; // 스폰 위치 리스트
    public CameraScript cameraObject; // 카메라 스크립트

    private int spawnIndex = 0; // 현재 사용 중인 스폰 위치 인덱스
    private bool mbIsFirstSetting;
    void Start()
    {
        mbIsFirstSetting = false;
        cameraObject = Camera.main.GetComponent<CameraScript>();
        NetworkManager.Instance.ConnetStart();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V)) 
        {
            SettingCamera();
        }

        if (mbIsFirstSetting) 
        {
            SendPlayerDataToNetwork();
        }
    }
    public void SettingPlayer()
    {
        // 로컬 플레이어 소환
        if (playerScript == null && spawnIndex < playerSpawnTransform.Count)
        {
            GameObject player = Instantiate(playerPrefab, playerSpawnTransform[spawnIndex].position, Quaternion.identity);
            playerScript = player.GetComponent<PlayerScript>();
            playerGun = player.transform.GetChild(0).gameObject.GetComponent<GunObject>();

            spawnIndex++; 
        }
    }

    public void SpawnOtherPlayer()
    {
        // 다른 플레이어 소환
        if (spawnIndex < playerSpawnTransform.Count)
        {
            GameObject newOtherPlayer = Instantiate(otherPlayerPrefab, playerSpawnTransform[spawnIndex].position, Quaternion.identity);
            PlayerScript pScript = newOtherPlayer.AddComponent<PlayerScript>();
            GunObject gScript = newOtherPlayer.transform.GetChild(0).gameObject.GetComponent<GunObject>();
            otherPlayer.Add(pScript);
            otherPlayerGun.Add(gScript);
            spawnIndex++; // 다음 스폰 위치로 이동
        }
        else
        {
            Debug.LogWarning("모든 스폰 위치가 사용되었습니다.");
        }
    }

    public void PlayerSetting(int playerID) 
    {
        if (mbIsFirstSetting)
        {
            if (playerID == 0)
            {
                SettingPlayer(); 
            }
            else
            {
                for (int index = 0; index < playerID; index++)
                {
                    if (index == playerID - 1)
                    {
                        SettingPlayer(); 
                    }
                    else
                    {
                        SpawnOtherPlayer();
                    }
                }
            }
            mbIsFirstSetting = false;
        }
        else 
        {
            SpawnOtherPlayer();
        }
    }

    public void PlayerSYNC(int playerId, float transformX, float transformY, float scaleX, float gunRotationZ, int playerHp, bool mIsShotOn) 
    {
        for (int index = 0; index < otherPlayer.Count; index++) 
        {
            if (otherPlayer[index].playerID == playerId) 
            {
                otherPlayer[index].UpdateFromNetworkPlayer(transformX, transformY, scaleX, playerHp);
                otherPlayerGun[index].UpdateFromNetworkGunObject(gunRotationZ, scaleX, mIsShotOn);
            }
        }
    }

    public void SendPlayerDataToNetwork() 
    {
        float transformX = playerScript.transform.position.x;
        float transformY = playerScript.transform.position.y;
        float scaleX = playerScript.transform.localScale.x;
        float gunRotationZ = playerGun.transform.rotation.z;
        int playerHp = playerScript.playerHp;
        bool shotOn = false;
        NetworkManager.Instance.SendMovementData(transformX, transformY, scaleX, gunRotationZ, playerHp, shotOn);
    }

    public void SendPlayerDataToNetworkShot()
    {
        float transformX = playerScript.transform.position.x;
        float transformY = playerScript.transform.position.y;
        float scaleX = playerScript.transform.localScale.x;
        float gunRotationZ = playerGun.transform.rotation.z;
        int playerHp = playerScript.playerHp;
        bool shotOn = true;
        NetworkManager.Instance.SendMovementData(transformX, transformY, scaleX, gunRotationZ, playerHp, shotOn);
    }

    public void SettingCamera() 
    {
        cameraObject.SettingTarger(playerScript.gameObject);
    }
}
