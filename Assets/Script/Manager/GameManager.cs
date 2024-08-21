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

    public GameObject playerPrefab; // �÷��̾� ������
    public GameObject otherPlayerPrefab; // �ٸ� �÷��̾� ������

    public PlayerScript playerScript; // ���� �÷��̾�
    public GunObject playerGun; // ���� �÷��̾�

    public List<PlayerScript> otherPlayer = new List<PlayerScript>(); // �ٸ� �÷��̾� ����Ʈ
    public List<GunObject> otherPlayerGun = new List<GunObject>(); // �ٸ� �÷��̾� ����Ʈ
    public List<Transform> playerSpawnTransform; // ���� ��ġ ����Ʈ
    public CameraScript cameraObject; // ī�޶� ��ũ��Ʈ

    private int spawnIndex = 0; // ���� ��� ���� ���� ��ġ �ε���
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
        // ���� �÷��̾� ��ȯ
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
        // �ٸ� �÷��̾� ��ȯ
        if (spawnIndex < playerSpawnTransform.Count)
        {
            GameObject newOtherPlayer = Instantiate(otherPlayerPrefab, playerSpawnTransform[spawnIndex].position, Quaternion.identity);
            PlayerScript pScript = newOtherPlayer.AddComponent<PlayerScript>();
            GunObject gScript = newOtherPlayer.transform.GetChild(0).gameObject.GetComponent<GunObject>();
            otherPlayer.Add(pScript);
            otherPlayerGun.Add(gScript);
            spawnIndex++; // ���� ���� ��ġ�� �̵�
        }
        else
        {
            Debug.LogWarning("��� ���� ��ġ�� ���Ǿ����ϴ�.");
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
