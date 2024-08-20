using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayerScript : MonoBehaviour
{
    public ICharacterState currentState { get; private set; }
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }







}
