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
                    Debug.LogError("씬에 GameManager 오브젝트가 존재하지 않습니다.");
                    return null;
                }

                DontDestroyOnLoad(instance.gameObject);
                instance.Initialize();
            }
            return instance;
        }
    }

    public GameObject playerPrefab; // 플레이어 프리팹
    public GameObject otherPlayerPrefab; // 다른 플레이어 프리팹

    public PlayerScript playerScript; // 로컬 플레이어
    public GunObject playerGun; // 로컬 플레이어

    public List<PlayerScript> otherPlayers = new List<PlayerScript>(); // 다른 플레이어 리스트
    public List<GunObject> otherPlayerGuns = new List<GunObject>(); // 다른 플레이어 리스트
    public List<Transform> playerSpawnTransform; // 스폰 위치 리스트
    public CameraScript cameraScript; // 카메라 스크립트

    private int spawnIndex = 0; // 현재 사용 중인 스폰 위치 인덱스
    private bool mbIsFirstSetting;

    private float mLastTransformX;
    private float mLastTransformY;
    private float mLastScaleX;
    private float mLastGunRotationZ;
    private int mLastPlayerHp;
    private bool mLastShotOn;

    private void Initialize()
    {
        mbIsFirstSetting = true;
        cameraScript = Camera.main.GetComponent<CameraScript>();
        NetworkManager.Instance.ConnectStart();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V)) 
        {
            SettingCamera();
        }

        if (playerScript != null && playerScript.gameType == eGameCharacterType.PLAYER && otherPlayer.Count != 0)
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
            playerScript.SetPlayerID(spawnIndex);
            playerGun = player.transform.GetChild(0).gameObject.GetComponent<GunObject>();

            spawnIndex++; 
        }

        SettingCamera();
    }

    public void SpawnOtherPlayer()
    {
        // 다른 플레이어 소환
        if (spawnIndex < playerSpawnTransform.Count)
        {
            GameObject newOtherPlayer = Instantiate(otherPlayerPrefab, playerSpawnTransform[spawnIndex].position, Quaternion.identity);
            PlayerScript pScript = newOtherPlayer.AddComponent<PlayerScript>();
            pScript.SetEnemySetting();
            pScript.SetPlayerID(spawnIndex);
            GunObject gScript = newOtherPlayer.transform.GetChild(0).gameObject.GetComponent<GunObject>();
            gScript.Setting();
            otherPlayers.Add(pScript);
            otherPlayerGuns.Add(gScript);
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
                for (int index = 0; index <= playerID; index++)
                {
                    if (index == playerID)
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
        PlayerScript otherPlayer = otherPlayers.Find(p => p.playerID == playerId);
        if (otherPlayer != null)
        {
            otherPlayer.UpdateFromNetworkPlayer(transformX, transformY, scaleX, playerHp);
            otherPlayerGuns[otherPlayers.IndexOf(otherPlayer)].UpdateFromNetworkGunObject(gunRotationZ, scaleX, shotOn);
        }
    }

    public void SendPlayerDataToNetwork()
    {
        float transformX = playerScript.transform.position.x;
        float transformY = playerScript.transform.position.y;
        float scaleX = playerScript.transform.localScale.x;
        float gunRotationZ = playerGun.gunAngle;
        int playerHp = playerScript.playerHp;
        bool shotOn = false;

        // 이전 상태와 현재 상태를 비교하여 변경된 경우에만 데이터를 전송
        if (transformX != mLastTransformX || transformY != mLastTransformY || scaleX != mLastScaleX ||
            gunRotationZ != mLastGunRotationZ || playerHp != mLastPlayerHp || shotOn != mLastShotOn)
        {
            NetworkManager.Instance.SendMovementData(transformX, transformY, scaleX, gunRotationZ, playerHp, shotOn);

            // 이전 상태 업데이트
            mLastTransformX = transformX;
            mLastTransformY = transformY;
            mLastScaleX = scaleX;
            mLastGunRotationZ = gunRotationZ;
            mLastPlayerHp = playerHp;
            mLastShotOn = shotOn;
        }
    }

    public void SendPlayerDataToNetworkShot()
    {
        float transformX = playerScript.transform.position.x;
        float transformY = playerScript.transform.position.y;
        float scaleX = playerScript.transform.localScale.x;
        float gunRotationZ = playerGun.gunAngle;
        int playerHp = playerScript.playerHp;
        bool shotOn = true;
        NetworkManager.Instance.SendMovementData(transformX, transformY, scaleX, gunRotationZ, playerHp, shotOn);
    }

    public void SettingCamera() 
    {
        cameraScript.SettingTarger(playerScript.gameObject);
    }
}
