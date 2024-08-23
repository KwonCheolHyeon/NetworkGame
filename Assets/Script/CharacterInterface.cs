
public interface ICharacterState
{
    void EnterState(PlayerScript character);  // 상태에 진입할 때 호출되는 메서드
    void UpdateState(PlayerScript character); // 상태가 지속되는 동안 매 프레임마다 호출되는 메서드
    void FixedUpdateState(PlayerScript character); // 상태가 지속되는 동안 매 프레임마다 호출되는 메서드
    void ExitState(PlayerScript character);   // 상태를 벗어날 때 호출되는 메서드
}

public enum eGameCharacterType 
{
    PLAYER,
    Enemy
}