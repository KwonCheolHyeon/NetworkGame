using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public ICharacterState currentState {  get; private set; }
    public ICharacterState idleState = new IdlePlayer();
    public ICharacterState movingState = new MovingPlayer();
    public ICharacterState deadState = new DeadPlayer();
    public eGameCharacterType gameType { get; private set; }
    public Rigidbody2D rb;
    public Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        SetState(idleState);
    }


    void Update()
    {
        if (gameType == eGameCharacterType.PLAYER ) 
        {
            HandleInput();
        }

        if (currentState != null)
        {
            currentState.UpdateState(this);
        }
    }

    private void FixedUpdate()
    {
        currentState.FixedUpdateState(this);
    }

    private void LateUpdate()
    {
        RotatePlayerTowardsMouse();
    }

    public void SetPlayerType()
    {
        gameType = eGameCharacterType.PLAYER;
    }
    public void SetState(ICharacterState newState)
    {
        if (currentState != null)
        {
            currentState.ExitState(this);
        }

        currentState = newState;

        currentState.EnterState(this);
    }

    private void HandleInput() 
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SetState(deadState);
        }
        if(Input.GetKeyDown(KeyCode.R)) 
        {
            SetState(idleState);
        }
    }

    private void RotatePlayerTowardsMouse()
    {
        // 마우스의 월드 좌표 얻기
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // z축 값이 필요 없으므로 0으로 설정

        // 플레이어와 마우스 사이의 거리 계산
        Vector3 direction = mousePosition - transform.position;

        // 마우스가 플레이어 오른쪽에 있으면 정방향, 왼쪽에 있으면 반대 방향으로 회전
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // 오른쪽을 향함
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // 왼쪽을 향함
        }
    }
}
