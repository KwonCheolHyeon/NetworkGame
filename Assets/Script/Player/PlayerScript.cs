
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public ICharacterState currentState {  get; private set; }
    public ICharacterState idleState { get; private set; }
    public ICharacterState movingState { get; private set; }
    public ICharacterState deadState { get; private set; }
    public eGameCharacterType gameType { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public Animator animator { get; private set; }
    public int playerHp { get; private set; }
    public int playerID { get; private set; }
    public bool bIsDead { get; private set; }
    public Vector2 playerVelocity;
    //상대방일 경우
    public Vector3 mPrevVector;
    private Vector3 mTargetPosition;
    //플레이어 예측 움직임을 적용하기 위한 추가 변수들
    private Vector2 predictedPosition;
    private Vector2 lastReceivedPosition;
    private Vector2 lastReceivedVelocity;
    private float lastReceivedTime;
    private float interpolationFactor = 0.1f;

    private void Awake()
    {
        InitializePlayer();
    }

    private void InitializePlayer()
    {
        gameType = eGameCharacterType.PLAYER;
        playerHp = 100;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bIsDead = false;
        
        idleState = new IdlePlayer();
        movingState = new MovingPlayer();
        deadState = new DeadPlayer();
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

        if (playerHp <= 0 && !bIsDead)
        {
            SetState(deadState);
            bIsDead = true;
        }

        if (gameType == eGameCharacterType.Enemy)
        {
            UpdatePredictedPosition();
        }
    }

    private void FixedUpdate()
    {
        
        //if (gameType == eGameCharacterType.Enemy)
        //{
        //    UpdateEnemyPosition();
        //}
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
        if(Input.GetKeyDown(KeyCode.R)) 
        {
            SetState(idleState);
        }
    }
    private void UpdateEnemyPosition()
    {
        mPrevVector = transform.position;
        transform.position = Vector2.Lerp(transform.position, mTargetPosition, Time.deltaTime * 20);
    }

    private void RotatePlayerTowardsMouse()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // z축 값 초기화

        Vector3 direction = mousePosition - transform.position;
        transform.localScale = new Vector3(direction.x > 0 ? 1 : -1, 1, 1);
    }

    public void TakeDamage() 
    {
        playerHp -= 10;
        
    }

    public void UpdateFromNetworkPlayer(float serverPredictedPositionX, float serverPredictedPositionY, float velocityX, float velocityY, float scaleX, int php)
    {
        
        lastReceivedPosition = new Vector2(serverPredictedPositionX, serverPredictedPositionY);
        lastReceivedVelocity = new Vector2(velocityX, velocityY);
        lastReceivedTime = Time.time;

        playerHp = php;
        transform.localScale = new Vector3(scaleX, 1, 0);

        mTargetPosition = new Vector3(serverPredictedPositionX, serverPredictedPositionY, 0);
    }

    private void UpdatePredictedPosition()
    {
        float timeSinceLastUpdate = Time.time - lastReceivedTime;

        predictedPosition = lastReceivedPosition + lastReceivedVelocity * timeSinceLastUpdate;

        transform.position = Vector2.Lerp(transform.position, predictedPosition, interpolationFactor);
    }

    public void SetPlayerID(int playerid) 
    {
        playerID = playerid;
    }

    public void SetEnemySetting() 
    {
        gameType = eGameCharacterType.Enemy;
    }


}
