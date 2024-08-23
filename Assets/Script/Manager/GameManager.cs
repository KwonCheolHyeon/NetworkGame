using System.Collections.Generic;
using UnityEngine;

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

    public GameObject playerPrefab;
    public GameObject otherPlayerPrefab;

    public PlayerScript playerScript;
    public GunObject playerGun;

    public List<PlayerScript> otherPlayerScripts = new List<PlayerScript>();
    public List<GunObject> otherPlayerGunsScripts = new List<GunObject>();
    public List<Transform> playerSpawnPoints;
    public CameraScript cameraScript;
    
    private int mSpawnIndex = 0;
    private bool bmIsFirstSetting = true;

    //이전 캐릭터 정보 저장
    private Vector3 mLastPosition;
    private float mLastScaleX;
    private float mLastGunRotationZ;
    private int mLastPlayerHp;
    private bool mLastShotOn;

    private void Initialize()
    {
        cameraScript = Camera.main.GetComponent<CameraScript>();
        NetworkManager.Instance.ConnectStart();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            SetCameraTarget();
        }

        if (playerScript != null && playerScript.gameType == eGameCharacterType.PLAYER && otherPlayerScripts.Count > 0)
        {
            SendPlayerDataToNetwork();
        }
    }

    public void PlayerSetting(int playerId)
    {
        if (bmIsFirstSetting)
        {
            SetupPlayers(playerId);
            bmIsFirstSetting = false;
        }
        else
        {
            SpawnOtherPlayer();
        }
    }

    private void SetupPlayers(int playerId)
    {
        for (int i = 0; i <= playerId; i++)
        {
            if (i == playerId)
            {
                SetupLocalPlayer();
            }
            else
            {
                SpawnOtherPlayer();
            }
        }
    }

    private void SetupLocalPlayer()
    {
        if (playerScript == null && mSpawnIndex < playerSpawnPoints.Count)
        {
            GameObject player = Instantiate(playerPrefab, playerSpawnPoints[mSpawnIndex].position, Quaternion.identity);
            playerScript = player.GetComponent<PlayerScript>();
            playerScript.SetPlayerID(mSpawnIndex);
            playerGun = player.GetComponentInChildren<GunObject>();
            mSpawnIndex++;
        }

        SetCameraTarget();
    }

    private void SpawnOtherPlayer()
    {
        if (mSpawnIndex >= playerSpawnPoints.Count)
        {
            Debug.LogWarning("모든 스폰 위치가 사용되었습니다.");
            return;
        }

        GameObject otherPlayer = Instantiate(otherPlayerPrefab, playerSpawnPoints[mSpawnIndex].position, Quaternion.identity);
        PlayerScript otherPlayerScript = otherPlayer.AddComponent<PlayerScript>();
        otherPlayerScript.SetEnemySetting();
        otherPlayerScript.SetPlayerID(mSpawnIndex);

        GunObject otherGun = otherPlayer.GetComponentInChildren<GunObject>();
        otherGun.Setting();

        otherPlayerScripts.Add(otherPlayerScript);
        otherPlayerGunsScripts.Add(otherGun);
        mSpawnIndex++;
    }

    public void PlayerSYNC(int playerId, float transformX, float transformY, float scaleX, float gunRotationZ, int playerHp, bool shotOn)
    {
        PlayerScript otherPlayer = otherPlayerScripts.Find(p => p.playerID == playerId);
        if (otherPlayer != null)
        {
            otherPlayer.UpdateFromNetworkPlayer(transformX, transformY, scaleX, playerHp);
            otherPlayerGunsScripts[otherPlayerScripts.IndexOf(otherPlayer)].UpdateFromNetworkGunObject(gunRotationZ, scaleX, shotOn);
        }
    }

    private void SendPlayerDataToNetwork()
    {
        Vector3 currentPosition = playerScript.transform.position;
        float currentScaleX = playerScript.transform.localScale.x;
        float currentGunRotationZ = playerGun.gunAngle;
        int currentPlayerHp = playerScript.playerHp;
        bool shotOn = false;

        if (HasPlayerDataChanged(currentPosition, currentScaleX, currentGunRotationZ, currentPlayerHp, shotOn))
        {
            NetworkManager.Instance.SendMovementData(currentPosition.x, currentPosition.y, currentScaleX, currentGunRotationZ, currentPlayerHp, shotOn);
            SaveCurrentPlayerData(currentPosition, currentScaleX, currentGunRotationZ, currentPlayerHp, shotOn);
        }
    }

    public void SendPlayerDataToNetworkShot()
    {
        Vector3 currentPosition = playerScript.transform.position;
        float currentScaleX = playerScript.transform.localScale.x;
        float currentGunRotationZ = playerGun.gunAngle;
        int currentPlayerHp = playerScript.playerHp;
        bool shotOn = true;

        NetworkManager.Instance.SendMovementData(currentPosition.x, currentPosition.y, currentScaleX, currentGunRotationZ, currentPlayerHp, shotOn);
    }

    private bool HasPlayerDataChanged(Vector3 position, float scaleX, float gunRotationZ, int playerHp, bool shotOn)//데이터 바뀐게 있는지 체크
    {
        return position != mLastPosition || scaleX != mLastScaleX || gunRotationZ != mLastGunRotationZ || playerHp != mLastPlayerHp || shotOn != mLastShotOn;
    }

    private void SaveCurrentPlayerData(Vector3 position, float scaleX, float gunRotationZ, int playerHp, bool shotOn)//이전 정보 저장
    {
        mLastPosition = position;
        mLastScaleX = scaleX;
        mLastGunRotationZ = gunRotationZ;
        mLastPlayerHp = playerHp;
        mLastShotOn = shotOn;
    }

    private void SetCameraTarget()
    {
        cameraScript.SettingTarger(playerScript.gameObject);
    }
}
