using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
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
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            character.SetState(character.movingState);
        }
    }

    public void FixedUpdateState(PlayerScript character) 
    {
      
    }

    

    
}
