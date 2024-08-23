
using UnityEngine;

public class IdlePlayer : ICharacterState
{
    
    public void EnterState(PlayerScript character)
    {
        
    }

    public void ExitState(PlayerScript character)
    {
        
    }

    public void UpdateState(PlayerScript character)
    {
        if (character.gameType == eGameCharacterType.PLAYER)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                character.SetState(character.movingState);
            }
        }
        else 
        {
            if (character.mPrevVector != character.transform.position)//움직이는 중
            {
                character.SetState(character.movingState);
            }
        }
        
    }

    public void FixedUpdateState(PlayerScript character) 
    {
      
    }

    

    
}
