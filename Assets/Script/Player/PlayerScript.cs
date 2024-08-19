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
        // ���콺�� ���� ��ǥ ���
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // z�� ���� �ʿ� �����Ƿ� 0���� ����

        // �÷��̾�� ���콺 ������ �Ÿ� ���
        Vector3 direction = mousePosition - transform.position;

        // ���콺�� �÷��̾� �����ʿ� ������ ������, ���ʿ� ������ �ݴ� �������� ȸ��
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // �������� ����
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // ������ ����
        }
    }
}
