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

    public GameObject player; // ���� �÷��̾�
    public List<GameObject> otherPlayer = new List<GameObject>(); // �ٸ� �÷��̾� ����Ʈ
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
    }
    public void SettingPlayer()
    {
        // ���� �÷��̾� ��ȯ
        if (player == null && spawnIndex < playerSpawnTransform.Count)
        {
            player = Instantiate(playerPrefab, playerSpawnTransform[spawnIndex].position, Quaternion.identity);
            spawnIndex++; 
        }
    }

    public void SpawnOtherPlayer()
    {
        // �ٸ� �÷��̾� ��ȯ
        if (spawnIndex < playerSpawnTransform.Count)
        {
            GameObject newOtherPlayer = Instantiate(otherPlayerPrefab, playerSpawnTransform[spawnIndex].position, Quaternion.identity);
            newOtherPlayer.AddComponent<PlayerScript>();
            otherPlayer.Add(newOtherPlayer);
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

    public void PlayerSYNC() 
    {
        //���⼭ �÷��̾ ������ ������ �������� ����
    }

    public void SettingCamera() 
    {
        cameraObject.SettingTarger(player);
    }
}
