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
    private bool bmSendInformation = false;
    //이전 캐릭터 정보 저장
    private Vector3 mLastPosition;
    private float mLastScaleX;
    private float mLastGunRotationZ;
    private int mLastPlayerHp;
    private bool mLastShotOn;
    private float mLastVelocityX;
    private float mLastVelocityY;

    private void Start()
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

        if (playerScript != null && playerScript.gameType == eGameCharacterType.PLAYER && otherPlayerScripts.Count > 0 && !bmSendInformation)
        {
            bmSendInformation = true;
            InvokeRepeating("SendPlayerDataToNetwork", 0f, 0.1f);
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

    public void PlayerSYNC(int playerId, float transformX, float transformY, float scaleX, float gunRotationZ, int playerHp, bool shotOn, float velocityX, float velocityY)
    {
        PlayerScript otherPlayer = otherPlayerScripts.Find(p => p.playerID == playerId);
        if (otherPlayer != null)
        {
            otherPlayer.UpdateFromNetworkPlayer(transformX, transformY, velocityX, velocityY, scaleX, playerHp);
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
        float velocityX = playerScript.playerVelocity.x;
        float velocityY = playerScript.playerVelocity.y;

        if (HasPlayerDataChanged(currentPosition, currentScaleX, currentGunRotationZ, currentPlayerHp, shotOn, velocityX, velocityY))
        {
            NetworkManager.Instance.SendMovementData(currentPosition.x, currentPosition.y, currentScaleX, currentGunRotationZ, currentPlayerHp, shotOn, velocityX, velocityY);
            SaveCurrentPlayerData(currentPosition, currentScaleX, currentGunRotationZ, currentPlayerHp, shotOn, velocityX, velocityY);
        }
    }

    public void SendPlayerDataToNetworkShot()
    {
        Vector3 currentPosition = playerScript.transform.position;
        float currentScaleX = playerScript.transform.localScale.x;
        float currentGunRotationZ = playerGun.gunAngle;
        int currentPlayerHp = playerScript.playerHp;
        bool shotOn = true;
        float velocityX = playerScript.playerVelocity.x;
        float velocityY = playerScript.playerVelocity.y;
        NetworkManager.Instance.SendMovementData(currentPosition.x, currentPosition.y, currentScaleX, currentGunRotationZ, currentPlayerHp, shotOn, velocityX, velocityY);
    }

    private bool HasPlayerDataChanged(Vector3 position, float scaleX, float gunRotationZ, int playerHp, bool shotOn, float velocityX, float velocityY)//데이터 바뀐게 있는지 체크
    {
        return position != mLastPosition || scaleX != mLastScaleX || gunRotationZ != mLastGunRotationZ || playerHp != mLastPlayerHp || shotOn != mLastShotOn || velocityX != mLastVelocityX || velocityY != mLastVelocityY;
    }

    private void SaveCurrentPlayerData(Vector3 position, float scaleX, float gunRotationZ, int playerHp, bool shotOn, float velocityX, float velocityY)//이전 정보 저장
    {
        mLastPosition = position;
        mLastScaleX = scaleX;
        mLastGunRotationZ = gunRotationZ;
        mLastPlayerHp = playerHp;
        mLastShotOn = shotOn;
        mLastVelocityX = velocityX;
        mLastVelocityY = velocityY;
    }

    private void SetCameraTarget()
    {
        cameraScript.SettingTarger(playerScript.gameObject);
    }
}
