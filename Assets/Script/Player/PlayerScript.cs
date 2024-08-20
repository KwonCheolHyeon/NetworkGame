using Goldmetal.UndeadSurvivor;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public int playerHp;
    //������ ���
    public Vector3 mPrevVector;
    private Vector3 mTargetPosition;

    private void Awake()
    {
        gameType = eGameCharacterType.PLAYER;
        playerHp = 100;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
    }
    void Start()
    {
        
        SetState(idleState);
    }


    void Update()
    {
        if (gameType == eGameCharacterType.PLAYER)
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
        
        if (gameType == eGameCharacterType.Enemy)
        {
            mPrevVector = transform.position;
            transform.position = Vector2.Lerp(transform.position, mTargetPosition, Time.deltaTime * 10);
        }
        currentState.FixedUpdateState(this);
    }

    private void LateUpdate()
    {
        if (gameType == eGameCharacterType.PLAYER) 
        {
            RotatePlayerTowardsMouse();
        }
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
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // z�� ���� �ʿ� �����Ƿ� 0���� ����

        Vector3 direction = mousePosition - transform.position;

        if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // �������� ����
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // ������ ����
        }
    }

    public void TakeDamage() 
    {
        playerHp -= 10;
        if (playerHp <= 0) 
        {
            SetState(deadState);
        }
    }

    public void UpdateFromNetworkPlayer(float x, float y, float scaleX, int hp)
    {
        mTargetPosition = new Vector3(x, y, 0);
        transform.localScale = new Vector3(scaleX, 1, 0);
        playerHp = hp;
    }
}
