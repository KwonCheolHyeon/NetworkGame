
using UnityEngine;

public class MovingPlayer : ICharacterState
{
   
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
            if (character.mPrevVector == character.transform.position) //������ ����ġ�� ������ ���� ����
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
        character.playerVelocity.x = Input.GetAxis("Horizontal");
        character.playerVelocity.y = Input.GetAxis("Vertical");

        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            character.SetState(character.idleState);
        }
    }

    private void MovePosition(PlayerScript character)
    {
        character.rb.MovePosition(character.rb.position + character.playerVelocity * mMoveSpeed * Time.fixedDeltaTime);
    }
}
