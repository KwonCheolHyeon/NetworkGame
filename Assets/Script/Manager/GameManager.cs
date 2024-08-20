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
    void Start()
    {
        cameraObject = Camera.main.GetComponent<CameraScript>();
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
            spawnIndex++; // ���� ���� ��ġ�� �̵�
        }
    }

    public void SpawnOtherPlayer()
    {
        // �ٸ� �÷��̾� ��ȯ
        if (spawnIndex < playerSpawnTransform.Count)
        {
            GameObject newOtherPlayer = Instantiate(otherPlayerPrefab, playerSpawnTransform[spawnIndex].position, Quaternion.identity);
            otherPlayer.Add(newOtherPlayer);
            spawnIndex++; // ���� ���� ��ġ�� �̵�
        }
        else
        {
            Debug.LogWarning("��� ���� ��ġ�� ���Ǿ����ϴ�.");
        }
    }

    public void SettingCamera() 
    {
        cameraObject.SettingTarger(player);
    }
}
