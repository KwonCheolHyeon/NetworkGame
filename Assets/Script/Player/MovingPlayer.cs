
using UnityEngine;

public class MovingPlayer : ICharacterState
{
    private Vector2 mMovement;
    private float mMoveSpeed = 5f;

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
        if (character.gameType == eGameCharacterType.PLAYER)
        {
            HandleInput(character);
        }
        else
        {
            if (character.mPrevVector == character.transform.position) //이전과 현위치가 같으면 정지 상태
            {
                character.SetState(character.idleState);
            }
        }
    }

    public void FixedUpdateState(PlayerScript character)
    {
        MovePosition(character);
    }
    private void HandleInput(PlayerScript character)
    {
        mMovement.x = Input.GetAxis("Horizontal");
        mMovement.y = Input.GetAxis("Vertical");

        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            character.SetState(character.idleState);
        }
    }

    private void MovePosition(PlayerScript character)
    {
        character.rb.MovePosition(character.rb.position + mMovement * mMoveSpeed * Time.fixedDeltaTime);
    }
}
