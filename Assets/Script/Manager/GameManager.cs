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

    public GameObject player; // 로컬 플레이어
    public List<GameObject> otherPlayer = new List<GameObject>(); // 다른 플레이어 리스트
    public List<Transform> playerSpawnTransform; // 스폰 위치 리스트
    public CameraScript cameraObject; // 카메라 스크립트

    private int spawnIndex = 0; // 현재 사용 중인 스폰 위치 인덱스
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
        // 로컬 플레이어 소환
        if (player == null && spawnIndex < playerSpawnTransform.Count)
        {
            player = Instantiate(playerPrefab, playerSpawnTransform[spawnIndex].position, Quaternion.identity);
            spawnIndex++; // 다음 스폰 위치로 이동
        }
    }

    public void SpawnOtherPlayer()
    {
        // 다른 플레이어 소환
        if (spawnIndex < playerSpawnTransform.Count)
        {
            GameObject newOtherPlayer = Instantiate(otherPlayerPrefab, playerSpawnTransform[spawnIndex].position, Quaternion.identity);
            otherPlayer.Add(newOtherPlayer);
            spawnIndex++; // 다음 스폰 위치로 이동
        }
        else
        {
            Debug.LogWarning("모든 스폰 위치가 사용되었습니다.");
        }
    }

    public void SettingCamera() 
    {
        cameraObject.SettingTarger(player);
    }
}
