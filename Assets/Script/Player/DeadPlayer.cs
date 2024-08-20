using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadPlayer : ICharacterState
{
    public void EnterState(PlayerScript character)
    {
        character.animator.SetBool("DeadOn", true);
    }

    public void ExitState(PlayerScript character)
    {
        character.animator.SetBool("DeadOn", false);
    }

    public void UpdateState(PlayerScript character)
    {
        
    }

    public void FixedUpdateState(PlayerScript character)
    {

    }
}
