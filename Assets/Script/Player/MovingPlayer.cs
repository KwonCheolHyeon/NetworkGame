using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlayer : ICharacterState
{
    private Vector2 movement;
    private float moveSpeed = 5f;

    public void EnterState(PlayerScript character)
    {
        character.animator.SetBool("RunOn",true);
    }

    public void ExitState(PlayerScript character)
    {
        character.animator.SetBool("RunOn", false);
    }

    public void UpdateState(PlayerScript character)
    {
        HandleInput(character);
    }

    public void FixedUpdateState(PlayerScript character)
    {
        MovePosition(character);
    }
    private void HandleInput(PlayerScript character)
    {
        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");

        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            character.SetState(character.idleState);
        }
    }

    private void MovePosition(PlayerScript character)
    {
        character.rb.MovePosition(character.rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
